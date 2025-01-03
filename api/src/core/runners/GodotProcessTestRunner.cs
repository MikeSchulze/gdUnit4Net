namespace GdUnit4.Core.Runners;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Events;

using Environment = System.Environment;

internal sealed class GodotProcessTestRunner : BaseTestRunner
{
    private const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter";

    internal GodotProcessTestRunner(ITestEngineLogger logger) : base(new GodotGdUnit4RestClient(logger), logger) { }

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

    protected static DataReceivedEventHandler StdErrorProcessor => (_, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;
        // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
        Console.WriteLine($"{message}");
    };

    protected static EventHandler ExitHandler => (sender, _) =>
    {
        Console.Out.Flush();
        //if (sender is Process p)
        //  frameworkHandle?.SendMessage(TestMessageLevel.Informational, $"Godot ends with exit code: {p.ExitCode}");
    };

    public new void RunAndWait2(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        Task.Run(async () =>
        {
            await using var server = new GodotGdUnit4RestServer(Logger);
            await server.Start();

            while (true) await server.Process();
        });

        base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);


        Thread.Sleep(1000);
    }

    public new void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
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

        var pProcess = new Process { StartInfo = processStartInfo };
        pProcess.EnableRaisingEvents = true;
        pProcess.ErrorDataReceived += StdErrorProcessor;
        pProcess.Exited += ExitHandler;

        pProcess.Start();
        pProcess.BeginErrorReadLine();
        pProcess.BeginOutputReadLine();
        // --> i need here to attach the current running debugger
        Console.WriteLine($"Debugger: {Debugger.IsAttached}");
        //DebuggerUtils.AttachDebuggerToProcess(pProcess.Id);


        base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);

        if (!pProcess.WaitForExit(1000)) // 30 second timeout
            pProcess.Kill();
        pProcess.CancelErrorRead();
        pProcess.CancelOutputRead();
        pProcess.ErrorDataReceived -= StdErrorProcessor;
        pProcess.Exited -= ExitHandler;
        pProcess.Dispose();
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
        //if (Directory.Exists(destinationFolderPath))
        //    return;
        Directory.CreateDirectory(destinationFolderPath);

        Logger.LogInfo($"Installing GdUnit4 `TestRunner` at {destinationFolderPath}...");
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("GdUnit4.src.core.runners.GodotTestRunnerScene.cs");
        using var reader = new StreamReader(stream!);
        var content = reader.ReadToEnd();
        File.WriteAllText(Path.Combine(destinationFolderPath, "GodotTestRunnerScene.cs"), content);
    }

    private string BuildGodotArguments()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var args =
            $"--path . -d -s res://{TEMP_TEST_RUNNER_DIR}/GodotTestRunnerScene.cs --test-assembly {assembly.Location} --test-suite ClassName --test-case MethodName";


        return args;
    }
}
