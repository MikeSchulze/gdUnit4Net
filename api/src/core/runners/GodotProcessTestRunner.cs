﻿namespace GdUnit4.Core.Runners;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

using Api;

using Events;

using Environment = System.Environment;

internal sealed class GodotProcessTestRunner : BaseTestRunner
{
    private const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter";

    private Process? process;

    internal GodotProcessTestRunner(ITestEngineLogger logger, IDebuggerFramework debuggerFramework) : base(new GodotGdUnit4RestClient(logger), logger)
        => DebuggerFramework = debuggerFramework;

    private object ProcessLock { get; } = new();
    private IDebuggerFramework DebuggerFramework { get; }
    private string? WorkingDirectory { get; set; }

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
        Logger.LogError($"{message}");
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
                new ProcessStartInfo(@$"{GodotBin}", BuildGodotArguments())
                {
                    StandardOutputEncoding = Encoding.Default,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = WorkingDirectory
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
        process.CancelErrorRead();
        process.CancelOutputRead();
        process.ErrorDataReceived -= StdErrorProcessor;
        process.Exited -= ExitHandler;
        process.Dispose();
    }

    private void InitRuntimeEnvironment()
    {
        if (WorkingDirectory != null)
            return;
        WorkingDirectory = LookupGodotProjectPath(Environment.CurrentDirectory)
                           ?? throw new InvalidOperationException("Cannot determine the godot.project! The workingDirectory is not set");
        if (Directory.Exists(WorkingDirectory))
        {
            Directory.SetCurrentDirectory(WorkingDirectory);
            Logger.LogInfo($"Current directory set to: {WorkingDirectory}");
        }

        InstallTestRunnerClasses();
    }

    private string LookupGodotProjectPath(string assemblyPath)
    {
        Logger.LogInfo($"Search 'godot.project' at {assemblyPath}");
        var currentDir = new DirectoryInfo(assemblyPath).Parent;
        while (currentDir != null)
        {
            if (currentDir.EnumerateFiles("project.godot").Any())
                return currentDir.FullName;
            currentDir = currentDir.Parent;
        }

        throw new FileNotFoundException("Godot project file '\"project.godot' does not exist");
    }

    private void InstallTestRunnerClasses()
    {
        var destinationFolderPath = Path.Combine(WorkingDirectory!, @$"{TEMP_TEST_RUNNER_DIR}");
        if (!Directory.Exists(destinationFolderPath))
            Directory.CreateDirectory(destinationFolderPath);

        var sceneRunnerSource = Path.Combine(destinationFolderPath, "GdUnit4TestRunnerScene.cs");
        // check if the scene runner already installed
        if (File.Exists(sceneRunnerSource))
            return;

        Logger.LogInfo($"Installing GdUnit4 TestRunner at {destinationFolderPath}...");

        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GdUnit4TestRunnerScene.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
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

    private string BuildGodotArguments()
        => $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GdUnit4TestRunnerScene.cs";
}
