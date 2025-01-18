namespace GdUnit4.Core.Runners;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

using Api;

using Execution;

using Environment = System.Environment;

/// <summary>
///     Test runner implementation that executes tests in a separate Godot runtime process.
///     Handles process management, IPC, and test execution coordination with the Godot engine.
/// </summary>
internal sealed class GodotRuntimeTestRunner : BaseTestRunner
{
    /// <summary>
    ///     Directory name for temporary test runner files.
    /// </summary>
    private const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter";

    private readonly TestEngineSettings settings;

    private Process? process;

    /// <summary>
    ///     Initializes a new instance of the GodotRuntimeTestRunner.
    /// </summary>
    /// <param name="logger">The test engine logger for diagnostic output.</param>
    /// <param name="debuggerFramework">Framework for debugging support.</param>
    /// <param name="settings">Test engine configuration settings.</param>
    internal GodotRuntimeTestRunner(ITestEngineLogger logger, IDebuggerFramework debuggerFramework, TestEngineSettings settings)
        : base(new GodotRuntimeExecutor(logger), logger, settings)
    {
        this.settings = settings;
        DebuggerFramework = debuggerFramework;
    }

    private object ProcessLock { get; } = new();
    private IDebuggerFramework DebuggerFramework { get; }

    /// <summary>
    ///     Gets the path to the Godot executable from environment variables.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when Godot executable path is not configured or found.</exception>
    private static string GodotBin
    {
        get
        {
            var godotPath = Environment.GetEnvironmentVariable("GODOT_BIN");
            if (string.IsNullOrEmpty(godotPath))
                throw new InvalidOperationException(
                    "Godot runtime is not configured. The environment variable 'GODOT_BIN' is not set or empty. Please set it to the Godot executable path.");
            if (!File.Exists(godotPath))
                throw new InvalidOperationException($"The Godot executable was not found at path: {godotPath}");
            return godotPath;
        }
    }

    private DataReceivedEventHandler StdErrorProcessor => (_, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;
        // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
        Logger.LogInfo($":: {message}");
    };

    private EventHandler ExitHandler(string source = "") => (sender, _) =>
    {
        Console.Out.Flush();
        if (sender is Process p)
            Logger.LogInfo($"{source} ends with exit code: {p.ExitCode}\n");
    };

    public override void Cancel()
    {
        base.Cancel();
        lock (ProcessLock)
        {
            process?.Kill(true);
            process?.WaitForExit(1000);
            CloseProcess(process);
        }
    }

    public new void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        lock (ProcessLock)
        {
            if (!InitRuntimeEnvironment())
                return;

            Logger.LogInfo("======== Running GdUnit4 Godot Runtime Test Runner ========");

            var processStartInfo =
                new ProcessStartInfo(@$"{GodotBin}", BuildGodotArguments(settings))
                {
                    StandardOutputEncoding = Encoding.Default,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = Environment.CurrentDirectory
                };

            if (DebuggerFramework.IsDebugProcess)
                process = DebuggerFramework.LaunchProcessWithDebuggerAttached(processStartInfo);
            else
            {
                process = new Process { StartInfo = processStartInfo };
                process.EnableRaisingEvents = true;
                process.ErrorDataReceived += StdErrorProcessor;
                process.Exited += ExitHandler("GdUnit4 Godot Runtime Test Runner");
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                if (DebuggerFramework.IsDebugAttach)
                    DebuggerFramework.AttachDebuggerToProcess(process);
            }

            base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);

            process.WaitForExit(2000);
            // wait until the process has finished
            var waitRetry = 0;
            while (!process.HasExited && waitRetry++ < 10)
                Thread.Sleep(100);
            // If the process not finished until 10 retries, we kill it manually
            if (!process.HasExited)
            {
                Logger.LogInfo("GdUnit4 Godot Runtime Test Runner is not terminated, force process kill.");
                process.Kill(true);
            }

            CloseProcess(process);
        }
    }

    private void CloseProcess(Process? processToClose)
    {
        if (processToClose == null)
            return;
        try
        {
            processToClose.CancelErrorRead();
            processToClose.CancelOutputRead();
            processToClose.ErrorDataReceived -= StdErrorProcessor;
            processToClose.Exited -= ExitHandler();
            processToClose.Dispose();
        }
        catch (Exception)
        {
            // ignore
        }
        finally
        {
            process = null;
        }
    }

    private bool InitRuntimeEnvironment() =>
        InstallTestRunnerClasses();

    private bool InstallTestRunnerClasses()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var destinationFolderPath = Path.Combine(Environment.CurrentDirectory, @$"{TEMP_TEST_RUNNER_DIR}");
        if (!Directory.Exists(destinationFolderPath))
            Directory.CreateDirectory(destinationFolderPath);

        var sceneRunnerSource = Path.Combine(destinationFolderPath, "GdUnit4TestRunnerScene.cs");
        // check if the scene runner already installed
        if (File.Exists(sceneRunnerSource))
            return true;

        Logger.LogInfo("======== Installing GdUnit4 Godot Runtime Test Runner ========");

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GdUnit4TestRunnerSceneTemplate.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        content = content.Replace("GdUnit4TestRunnerSceneTemplate", "GdUnit4TestRunnerScene");
        File.WriteAllText(sceneRunnerSource, content, Encoding.UTF8);

        try
        {
            // recompile the project
            var processStartInfo = new ProcessStartInfo($"{GodotBin}", @"--path . --headless --build-solutions --quit-after 100")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            Logger.LogInfo($"Run Rebuild Godot Project ... {GodotBin} {processStartInfo.Arguments}");
            using var compileProcess = new Process();
            compileProcess.StartInfo = processStartInfo;
            compileProcess.EnableRaisingEvents = true;
            compileProcess.OutputDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (string.IsNullOrEmpty(message))
                    return;

                Logger.LogInfo($".. {message}");
            };
            compileProcess.ErrorDataReceived += StdErrorProcessor;
            compileProcess.Exited += ExitHandler("Rebuild Godot Project");
            if (!compileProcess.Start())
            {
                Logger.LogError(@"Rebuild Godot Project fails on process start, exit ..");
                return false;
            }

            compileProcess.BeginErrorReadLine();
            compileProcess.BeginOutputReadLine();
            compileProcess.WaitForExit(5000);

            // The compile project can take a while, and we need to wait it is finises
            var waitRetry = 0;
            while (!compileProcess.HasExited && waitRetry++ < 200)
                Thread.Sleep(100);
            // If the process not finished until 200 retries, we kill it manually
            if (!compileProcess.HasExited)
            {
                Logger.LogInfo("Rebuild Godot Project is not correct terminated, force kill process now.");
                compileProcess.Kill(true);
            }

            var isSuccess = compileProcess.ExitCode == 0;
            CloseProcess(compileProcess);
            return isSuccess;
        }
        catch (Exception e)
        {
            Logger.LogError($"Install GdUnit4 `TestRunner` fails with: {e.Message}\n {e.StackTrace}");
            return false;
        }
    }

    private static string BuildGodotArguments(TestEngineSettings testEngineSettings)
        => $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GdUnit4TestRunnerScene.cs {testEngineSettings.Parameters}";
}
