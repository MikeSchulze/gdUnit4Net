using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System;
using GdUnit4.TestAdapter.Settings;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace GdUnit4.TestAdapter.Execution;

internal class TestExecutor : BaseTestExecutor, ITestExecutor
{
    const string temp_test_runner_dir = "gdunit4_testadapter";

    private Process? pProcess = null;
    private readonly GdUnit4Settings gdUnit4Settings;

    private int ParallelTestCount { get; set; }

    private int SessionTimeOut { get; set; }

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

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, IEnumerable<TestCase> testCases)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Start executing tests, {testCases.Count()} TestCases total.");
        // TODO split into multiple threads by using 'ParallelTestCount'
        Dictionary<string, List<TestCase>> groupedTests = testCases
            .GroupBy(t => t.CodeFilePath!)
            .ToDictionary(group => group.Key, group => group.ToList());


        var workingDirectory = LookupGodotProjectPath(groupedTests.First().Key);
        _ = workingDirectory ?? throw new ArgumentNullException(nameof(workingDirectory), "Cannot determine the godot.project!");
        InstallTestRunnerAndBuild(frameworkHandle, workingDirectory);

        frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run tests -------->");
        var configName = WriteTestRunnerConfig(groupedTests);
        var debugArg = runContext.IsBeingDebugged ? "-d" : "";

        //var filteredTestCases = filterExpression != null
        //    ? testCases.FindAll(t => filterExpression.MatchTestCase(t, (propertyName) =>
        //    {
        //        SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
        //        return t.GetPropertyValue(testProperty);
        //    }) == false)
        //    : testCases;
        string testRunnerScene = "res://gdunit4_testadapter/TestAdapterRunner.tscn";//Path.Combine(workingDirectory, @$"{temp_test_runner_dir}/TestRunner.tscn");
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @$"{debugArg} --path {workingDirectory} {testRunnerScene} --testadapter --configfile='{configName}' {gdUnit4Settings.Parameters}")
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

        using (pProcess = new() { StartInfo = processStartInfo })
        {
            pProcess.EnableRaisingEvents = true;
            pProcess.OutputDataReceived += TestEventProcessor(frameworkHandle, testCases);
            pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
            pProcess.Exited += ExitHandler(frameworkHandle);
            pProcess.Start();
            AttachDebuggerIfNeed(runContext, frameworkHandle, pProcess);

            pProcess.BeginErrorReadLine();
            pProcess.BeginOutputReadLine();
            while (!pProcess.WaitForExit(SessionTimeOut))
            {
                Thread.Sleep(100);
            }
            try
            {
                pProcess.Kill(true);
                frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run TestRunner ends with {pProcess.ExitCode}");
            }
            catch (Exception e)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Run TestRunner ends with {e.Message}");
            }
            File.Delete(configName);
        };
    }

    private void InstallTestRunnerAndBuild(IFrameworkHandle frameworkHandle, string workingDirectory)
    {
        string destinationFolderPath = Path.Combine(workingDirectory, @$"{temp_test_runner_dir}");
        if (Directory.Exists(destinationFolderPath))
        {
            return;
        }
        frameworkHandle.SendMessage(TestMessageLevel.Informational, "Install GdUnit4 TestRunner");
        InstallTestRunnerClasses(destinationFolderPath);
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @$"--path {workingDirectory} --headless --build-solutions --quit-after 20")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = @$"{workingDirectory}",
        };

        using Process process = new() { StartInfo = processStartInfo };
        frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Rebuild ...");
        process.Start();
        while (!process.WaitForExit(5000))
        {
            Thread.Sleep(100);
        }
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

    public void Cancel()
    {
        lock (this)
        {
            Console.WriteLine("Cancel triggered");
            try
            {
                pProcess?.Kill(true);
                pProcess?.WaitForExit();
            }
            catch (Exception)
            {
                //frameworkHandle.SendMessage(TestMessageLevel.Error, @$"TestRunner ends with: {e.Message}");
            }
        }
    }

    private static void InstallTestRunnerClasses(string destinationFolderPath)
    {
        Directory.CreateDirectory(destinationFolderPath);
        string srcTestRunner = """
            namespace GdUnit4.TestAdapter;

            public partial class TestAdapterRunner : Api.TestRunner
            {
                public override void _Ready()
                    => _ = RunTests();
            }

            """;

        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.cs"), srcTestRunner);

        string srcTestRunnerScene = $"""
            [gd_scene load_steps=2 format=3 uid="uid://5o7l4yufw1rw"]

            [ext_resource type="Script" path="res://gdunit4_testadapter/TestAdapterRunner.cs" id="1"]

            [node name="Control" type="Control"]
            layout_mode = 3
            anchors_preset = 0
            script = ExtResource("1")

            """;
        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.tscn"), srcTestRunnerScene);
    }
}
