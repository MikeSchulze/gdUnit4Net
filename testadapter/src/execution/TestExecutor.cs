namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Settings;

internal sealed class TestExecutor : BaseTestExecutor, ITestExecutor
{
    private const string TempTestRunnerDir = "gdunit4_testadapter";

    private Process? pProcess;
    private readonly GdUnit4Settings gdUnit4Settings;

#pragma warning disable IDE0052 // Remove unread private members
    private int ParallelTestCount { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

    private int SessionTimeOut { get; set; }

    private object CancelLock { get; set; } = new();
    private object ProcessLock { get; set; } = new();

    public TestExecutor(RunConfiguration configuration, GdUnit4Settings gdUnit4Settings)
    {
        ParallelTestCount = configuration.MaxCpuCount == 0
            ? 1
            : configuration.MaxCpuCount;
        SessionTimeOut = (int)(configuration.TestSessionTimeout == 0
                ? ITestExecutor.DEFAULT_SESSION_TIMEOUT
                : configuration.TestSessionTimeout);

        this.gdUnit4Settings = gdUnit4Settings;
    }

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, List<TestCase> testCases)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Start executing tests, {testCases.Count} TestCases total.");
        // TODO split into multiple threads by using 'ParallelTestCount'
        var groupedTests = testCases
            .GroupBy(t => t.CodeFilePath!)
            .ToDictionary(group => group.Key, group => group.ToList());

        var workingDirectory = LookupGodotProjectPath(groupedTests.First().Key);
        _ = workingDirectory ?? throw new InvalidOperationException($"Cannot determine the godot.project! The workingDirectory is not set");

        if (Directory.Exists(workingDirectory))
        {
            Directory.SetCurrentDirectory(workingDirectory);
            frameworkHandle.SendMessage(TestMessageLevel.Informational, "Current directory set to: " + Directory.GetCurrentDirectory());
        }
        InstallTestRunnerAndBuild(frameworkHandle, workingDirectory);
        var configName = WriteTestRunnerConfig(groupedTests);
        var debugArg = runContext.IsBeingDebugged ? "-d" : "";
        if (runContext.IsBeingDebugged && frameworkHandle.ToString()!.Contains("JetBrains"))
            debugArg += " --attach_debugger";

        //var filteredTestCases = filterExpression != null
        //    ? testCases.FindAll(t => filterExpression.MatchTestCase(t, (propertyName) =>
        //    {
        //        SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
        //        return t.GetPropertyValue(testProperty);
        //    }) == false)
        //    : testCases;
        var testRunnerScene = "res://gdunit4_testadapter/TestAdapterRunner.tscn";//Path.Combine(workingDirectory, @$"{temp_test_runner_dir}/TestRunner.tscn");
        var arguments = $"{debugArg} --path . {testRunnerScene} --testadapter --configfile=\"{configName}\" {gdUnit4Settings.Parameters}";
        frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run with args {arguments}");
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", arguments)
        {
            StandardOutputEncoding = Encoding.Default,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = @$"{workingDirectory}"
        };

        lock (ProcessLock)
            using (pProcess = new() { StartInfo = processStartInfo })
            {
                pProcess.EnableRaisingEvents = true;
                pProcess.OutputDataReceived += TestEventProcessor(frameworkHandle, testCases);
                pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
                pProcess.Exited += ExitHandler(frameworkHandle);
                pProcess.Start();
                pProcess.BeginErrorReadLine();
                pProcess.BeginOutputReadLine();
                AttachDebuggerIfNeed(runContext, frameworkHandle, pProcess);
                while (!pProcess.WaitForExit(SessionTimeOut))
                    Thread.Sleep(100);
                try
                {
                    pProcess.Kill(true);
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run TestRunner ends with {pProcess.ExitCode}");
                }
                catch (Exception e)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Run TestRunner ends with: {e.Message}");
                }
                finally
                {
                    File.Delete(configName);
                }
            }

    }

    private void InstallTestRunnerAndBuild(IFrameworkHandle frameworkHandle, string workingDirectory)
    {
        var destinationFolderPath = Path.Combine(workingDirectory, @$"{TempTestRunnerDir}");
        if (Directory.Exists(destinationFolderPath))
            return;
        frameworkHandle.SendMessage(TestMessageLevel.Informational, "Install GdUnit4 TestRunner");
        InstallTestRunnerClasses(destinationFolderPath);
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @$"--path . --headless --build-solutions --quit-after 2000")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = @$"{workingDirectory}",
        };

        using Process process = new();
        process.StartInfo = processStartInfo;
        frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Rebuild ...");
        process.Start();
        while (!process.WaitForExit(5000)) Thread.Sleep(100);
        try
        {
            process.Kill(true);
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestRunner installed: {process.ExitCode}");
        }
        catch (Exception e)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, @$"TestRunner ends with: {e.Message}");
        }
    }

    public void Cancel(IFrameworkHandle frameworkHandle)
    {
        lock (CancelLock)
            try
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Cancel triggered on {pProcess}");
                pProcess?.Kill(true);
                pProcess?.WaitForExit();
            }
            catch (Exception e)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, @$"TestRunner ends with: {e.Message}");
            }
    }

    private static void InstallTestRunnerClasses(string destinationFolderPath)
    {
        Directory.CreateDirectory(destinationFolderPath);
        var srcTestRunner = """
            namespace GdUnit4;

            using System;
            using System.Diagnostics;
            using System.Linq;
            using System.Threading;

            public partial class TestAdapterRunner : Api.TestRunner
            {
                public override async void _Ready() {
                    var cmdArgs = Godot.OS.GetCmdlineArgs();

                    if (cmdArgs.Contains("--attach_debugger"))
                    {
                        Console.WriteLine("Debug process recognized, wait for debugger is attached ...");
                        while (!Debugger.IsAttached)
                        {
                            Thread.Sleep(200);
                            Console.WriteLine("Waiting for attaching debugger ...");
                        }
                        Console.WriteLine("The debugger is successfully attached, continue test run..");
                    }
                    await RunTests();
                }
            }

            """;

        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.cs"), srcTestRunner);

        var srcTestRunnerScene = $"""
            [gd_scene load_steps=2 format=3 uid="uid://5o7l4yufw1rw"]

            [ext_resource type="Script" path="res://gdunit4_testadapter/TestAdapterRunner.cs" id="1"]

            [node name="Control" type="Control"]
            layout_mode = 3
            anchors_preset = 0
            script = ExtResource("1")

            """;
        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.tscn"), srcTestRunnerScene);
    }

    public void Dispose()
    {
        pProcess?.Dispose();
        GC.SuppressFinalize(this);
    }
}
