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
        Logger.LogInfo($"{message}");
    };

    private EventHandler ExitHandler => (sender, _) =>
    {
        Console.Out.Flush();
        if (sender is Process p)
            Logger.LogInfo($"Godot ends with exit code: {p.ExitCode}");
    };

    public override void Cancel()
    {
        base.Cancel();
        lock (ProcessLock)
        {
            process?.Kill(true);
            process?.WaitForExit(1000);
            CloseProcess();
        }
    }

    public new void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        lock (ProcessLock)
        {
            InitRuntimeEnvironment();
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
                process.Exited += ExitHandler;
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                if (DebuggerFramework.IsDebugAttach)
                    DebuggerFramework.AttachDebuggerToProcess(process);
            }

            base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);

            if (!process.WaitForExit(1000)) // 30 second timeout
                process.Kill();
            CloseProcess();
        }
    }

    private void CloseProcess()
    {
        if (process == null)
            return;
        //process.CancelErrorRead();
        //process.CancelOutputRead();
        process.ErrorDataReceived -= StdErrorProcessor;
        process.Exited -= ExitHandler;
        process.Dispose();
        process = null;
    }

    private void InitRuntimeEnvironment() =>
        InstallTestRunnerClasses();


    private void InstallTestRunnerClasses()
    {
        var destinationFolderPath = Path.Combine(Environment.CurrentDirectory, @$"{TEMP_TEST_RUNNER_DIR}");
        if (!Directory.Exists(destinationFolderPath))
            Directory.CreateDirectory(destinationFolderPath);

        var sceneRunnerSource = Path.Combine(destinationFolderPath, "GdUnit4TestRunnerScene.cs");
        // check if the scene runner already installed
        if (File.Exists(sceneRunnerSource))
            return;

        Logger.LogInfo($"Installing GdUnit4 TestRunner at {destinationFolderPath}...");

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GdUnit4TestRunnerSceneTemplate.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        content = content.Replace("GdUnit4TestRunnerSceneTemplate", "GdUnit4TestRunnerScene");
        File.WriteAllText(sceneRunnerSource, content);
        /*
        // compile the scene
        try
        {
            Logger.LogInfo($"Compile GdUnit4 TestRunner at {destinationFolderPath}...");
            var processStartInfo = new ProcessStartInfo($"{GodotBin}", @"--path . --headless --build-solutions --quit-after 1000")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = false,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = @$"{WorkingDirectory}"
            };

            Logger.LogInfo($"Run Rebuild ... {GodotBin} {processStartInfo.Arguments} at {WorkingDirectory}");
            using var compileProcess = new Process();
            compileProcess.StartInfo = processStartInfo;
            compileProcess.EnableRaisingEvents = true;
            compileProcess.OutputDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (string.IsNullOrEmpty(message))
                    return;

                Logger.LogInfo($"{message}");
            };
            compileProcess.ErrorDataReceived += (_, args) =>
            {
                var message = args.Data?.Trim();
                if (string.IsNullOrEmpty(message))
                    return;

                Logger.LogError($"{message}");
            };
            //compileProcess.Exited += ExitHandler;
            compileProcess.Start();
            compileProcess.BeginErrorReadLine();
            compileProcess.BeginOutputReadLine();

            if (!compileProcess.WaitForExit(5000)) // 5 second timeout
                compileProcess.Kill(true);

            Logger.LogInfo($"GdUnit4 TestRunner successfully installed: {compileProcess.ExitCode}");
        }
        catch (Exception e)
        {
            Logger.LogError(@$"Install GdUnit4 `TestRunner` fails with: {e.Message}");
        }
        */
    }

    private string BuildGodotArguments(TestEngineSettings testEngineSettings)
        => $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GdUnit4TestRunnerScene.cs {testEngineSettings.Parameters}";
}
