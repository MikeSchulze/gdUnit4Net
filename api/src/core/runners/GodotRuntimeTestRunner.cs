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
    internal const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter";

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
    private string GodotBin
    {
        get
        {
            var godotPath = Environment.GetEnvironmentVariable("GODOT_BIN");
            if (string.IsNullOrEmpty(godotPath))
            {
                var message = "Godot runtime is not configured. The environment variable 'GODOT_BIN' is not set or empty. Please set it to the Godot executable path.";
                Logger.LogError(message);
                throw new InvalidOperationException(message);
            }

            if (File.Exists(godotPath))
                return godotPath;

            var errorMessage = $"The Godot executable was not found at path: {godotPath}";
            Logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
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
            if (p.ExitCode == 0)
                Logger.LogInfo($"{source} ends with exit code: {p.ExitCode}\n");
            else
                Logger.LogError($"{source} ends with exit code: {p.ExitCode}\n");
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
                new ProcessStartInfo(GodotBin, BuildGodotArguments(settings))
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
        InstallTestRunnerClasses(Environment.CurrentDirectory, GodotBin);

    internal bool InstallTestRunnerClasses(string workingDirectory, string godotBinary)
    {
        var destinationFolderPath = Path.Combine(workingDirectory, @$"{TEMP_TEST_RUNNER_DIR}");
        if (!Directory.Exists(destinationFolderPath))
            Directory.CreateDirectory(destinationFolderPath);

        var sceneRunnerSource = Path.Combine(destinationFolderPath, "GdUnit4TestRunnerScene.cs");
        // check if the scene runner already installed
        if (File.Exists(sceneRunnerSource))
            return true;

        Logger.LogInfo("======== Installing GdUnit4 Godot Runtime Test Runner ========");
        Logger.LogInfo($"Installing GdUnit4TestRunnerScene at {destinationFolderPath}");

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GdUnit4TestRunnerSceneTemplate.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        content = content.Replace("GdUnit4TestRunnerSceneTemplate", "GdUnit4TestRunnerScene");
        File.WriteAllText(sceneRunnerSource, content, Encoding.UTF8);

        try
        {
            // recompile the project
            var processStartInfo = new ProcessStartInfo($"{godotBinary}", @"--path . --headless --build-solutions --quit-after 1000")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = workingDirectory
            };

            Logger.LogInfo($"Working dir {workingDirectory}");
            Logger.LogInfo($"Run Rebuild Godot Project ... {godotBinary} {processStartInfo.Arguments}");
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
                CleanupRunnerOnFailure(sceneRunnerSource);
                return false;
            }

            compileProcess.BeginErrorReadLine();
            compileProcess.BeginOutputReadLine();
            compileProcess.WaitForExit(100);


            // The compile project can take a while, and we need to wait until it finishes
            // Calculate how many iterations we need based on the compile process timeout
            const int checkIntervalMs = 100; // Check every 100ms
            var maxRetries = settings.CompileProcessTimeout / checkIntervalMs;

            var waitRetry = 0;
            while (!compileProcess.HasExited && waitRetry++ < maxRetries)
                Thread.Sleep(checkIntervalMs);

            // If the process has not finished within the timeout period, we kill it manually
            if (!compileProcess.HasExited)
            {
                Logger.LogError($"""
                                 ╔═══════════════════════ Godot compilation TIMEOUT ═════════════════════════════════════════════════════════════════════╗

                                   Godot project compilation did not complete within the configured timeout of {settings.CompileProcessTimeout}ms.

                                   Possible reasons:
                                   - Your Godot project may be large or complex, requiring more time to compile
                                   - Your system may be under heavy load or has limited resources
                                   - There might be a compilation issue causing Godot to hang

                                   ACTION REQUIRED:
                                   To increase the compilation timeout, set the 'CompileProcessTimeout' property in your GdUnit4 settings.

                                   Add or modify the following in your .runsettings file:
                                   <GdUnit4>
                                       <CompileProcessTimeout>60000</CompileProcessTimeout>  <!-- 60 seconds -->
                                   </GdUnit4>

                                   The process will now be forcefully terminated, which may result in incomplete compilation.

                                 ╚══════════════════════════════════════════════════════════════════════════════════════════════════════════════════════╝
                                 """);

                compileProcess.Kill(true);
                CleanupRunnerOnFailure(sceneRunnerSource);
            }

            var isSuccess = compileProcess.ExitCode == 0;
            if (!isSuccess)
                CleanupRunnerOnFailure(sceneRunnerSource);

            CloseProcess(compileProcess);
            return isSuccess;
        }
        catch (Exception e)
        {
            Logger.LogError($"Install GdUnit4 `TestRunner` fails with: {e.Message}\n {e.StackTrace}");
            CleanupRunnerOnFailure(sceneRunnerSource);
            return false;
        }
    }

    /// <summary>
    ///     Cleans up the installed runner file when compilation fails to ensure
    ///     a fresh installation on the next run.
    /// </summary>
    /// <param name="runnerFilePath">Path to the runner file to remove</param>
    private void CleanupRunnerOnFailure(string runnerFilePath)
    {
        try
        {
            if (File.Exists(runnerFilePath))
            {
                Logger.LogInfo($"Cleaning up runner file at {runnerFilePath} due to compilation failure");
                File.Delete(runnerFilePath);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Failed to clean up runner file: {ex.Message}");
            // We don't want to throw here as this is just cleanup
        }
    }

    private static string BuildGodotArguments(TestEngineSettings testEngineSettings)
        => $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GdUnit4TestRunnerScene.cs {testEngineSettings.Parameters}";
}
