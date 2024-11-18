namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Core.Extensions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Settings;

using Utilities;

internal sealed class TestExecutor : BaseTestExecutor, ITestExecutor
{
    private const string TEMP_TEST_RUNNER_DIR = "gdunit4_testadapter";
    private readonly GdUnit4Settings gdUnit4Settings;

    private Process? pProcess;

    public TestExecutor(RunConfiguration configuration, GdUnit4Settings gdUnit4Settings)
    {
        ParallelTestCount = configuration.MaxCpuCount == 0
            ? 1
            : configuration.MaxCpuCount;
        SessionTimeOut = (int)(configuration.TestSessionTimeout == 0
            ? ITestExecutor.DefaultSessionTimeout
            : configuration.TestSessionTimeout);
        ResultsDirectory = configuration.ResultsDirectory;

        this.gdUnit4Settings = gdUnit4Settings;
    }

    private string ResultsDirectory { get; }
    private object CancelLock { get; } = new();
    private object ProcessLock { get; } = new();

#pragma warning disable IDE0052 // Remove unread private members
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private int ParallelTestCount { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

    private int SessionTimeOut { get; }

    public void Cancel()
    {
        lock (CancelLock)
            try
            {
                Console.WriteLine("Cancel triggered");
                if (pProcess != null && !pProcess.WaitForExit(0))
                    pProcess?.Kill(true);
            }
            catch (Exception)
            {
                Console.Error.WriteLine("Cancel triggered");
                //fh.SendMessage(TestMessageLevel.Error, @$"TestRunner ends with: {e.Message}");
            }
    }

    public void Dispose()
        => pProcess?.Dispose();

    public void Run(IFrameworkHandle frameworkHandle, IRunContext runContext, IReadOnlyList<TestCase> testCases)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Start executing tests, {testCases.Count} TestCases total.");
        // TODO split into multiple threads by using 'ParallelTestCount'
        var groupedTests = testCases
            .GroupBy(t => t.CodeFilePath!)
            .ToDictionary(group => group.Key, group => group.ToList());

        var workingDirectory = LookupGodotProjectPath(groupedTests.First().Key);
        _ = workingDirectory ?? throw new InvalidOperationException("Cannot determine the godot.project! The workingDirectory is not set");

        if (Directory.Exists(workingDirectory))
        {
            Directory.SetCurrentDirectory(workingDirectory);
            frameworkHandle.SendMessage(TestMessageLevel.Informational, "Current directory set to: " + Directory.GetCurrentDirectory());
        }

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Detected Running IDE: {IdeDetector.Detect(frameworkHandle)}");

        InstallTestRunnerAndBuild(frameworkHandle, workingDirectory);
        var configName = WriteTestRunnerConfig(groupedTests, gdUnit4Settings);
        var debugArg = runContext.IsBeingDebugged ? "-d" : "";

        using var eventServer = new TestEventReportServer();
        // ReSharper disable once AccessToDisposedClosure
        var testEventServerTask = Task.Run(() => eventServer.Start(frameworkHandle, testCases));

        //var filteredTestCases = filterExpression != null
        //    ? testCases.FindAll(t => filterExpression.MatchTestCase(t, (propertyName) =>
        //    {
        //        SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
        //        return t.GetPropertyValue(testProperty);
        //    }) == false)
        //    : testCases;
        var testRunnerScene = "res://gdunit4_testadapter/TestAdapterRunner.tscn";
        //  --log-file godot.log

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
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // if we run on JetBrains Rider in debug mode we need to setup the debugging in a custom way
                if (runContext.IsBeingDebugged && IsJetBrainsRider(frameworkHandle))
                    pProcess = RunDebugRider(frameworkHandle, processStartInfo);
                else
                {
                    // spawn a new child process to run Godot to execute the tests
                    pProcess = new Process { StartInfo = processStartInfo };
                    pProcess.EnableRaisingEvents = true;
                    pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
                    pProcess.Exited += ExitHandler(frameworkHandle);
                    pProcess.Start();
                    pProcess.BeginErrorReadLine();
                    pProcess.BeginOutputReadLine();
                    AttachDebuggerIfNeed(runContext, frameworkHandle, pProcess);
                }

                if (pProcess != null && pProcess.WaitForExit(SessionTimeOut))
                    frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run TestRunner ends with {pProcess.ExitCode}");
                else
                {
                    stopwatch.Stop();
                    var message = $"""

                                   ╔═══════════════════════ TEST SESSION TIMEOUT ═══════════════════════════════════════╗

                                     Test execution exceeded maximum allowed time:
                                       • Timeout: {TimeSpan.FromMilliseconds(SessionTimeOut).Humanize()}
                                       • Total tests: {testCases.Count}
                                       • Completed tests: {eventServer.CompletedTests}
                                       • Time elapsed: {stopwatch.Elapsed.Humanize()}

                                     ACTION REQUIRED: Please increase 'TestSessionTimeout' in your '.runsettings' file

                                   ╚════════════════════════════════════════════════════════════════════════════════════╝
                                   """;
                    frameworkHandle.SendMessage(TestMessageLevel.Error, message);
                    pProcess?.Kill(true);
                }
            }
            catch (Exception e)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Run TestRunner ends with an Exception: {e.Message}\n {e.StackTrace}");
            }
            finally
            {
                if (pProcess is IDisposable disposable)
                    disposable.Dispose();
                File.Delete(configName);
                // wait until all event messages are processed or the client is disconnected
                testEventServerTask.Wait(TimeSpan.FromSeconds(1));
                CollectGodotLogFile(frameworkHandle);
            }
    }

    private static bool IsJetBrainsRider(IFrameworkHandle frameworkHandle)
    {
        var version = frameworkHandle.GetType().Assembly.GetName().Version;
        if (frameworkHandle is not IFrameworkHandle2
            || !frameworkHandle.GetType().ToString().Contains("JetBrains")
            || version < new Version("2.16.1.14"))
            return false;

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"JetBrains Rider detected {version}");
        return true;
    }

    private static Process RunDebugRider(IFrameworkHandle frameworkHandle, ProcessStartInfo psi)
    {
        // EnableShutdownAfterTestRun is not working we need to use SafeHandle the get the process running until the ExitCode is getted
        frameworkHandle.EnableShutdownAfterTestRun = true;
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Debug process started {psi.FileName} {psi.WorkingDirectory} {psi.Arguments}");
        var processId = frameworkHandle.LaunchProcessWithDebuggerAttached(psi.FileName, psi.WorkingDirectory, psi.Arguments, psi.Environment);
        return Process.GetProcessById(processId);
    }

    private void InstallTestRunnerAndBuild(IFrameworkHandle frameworkHandle, string workingDirectory)
    {
        var destinationFolderPath = Path.Combine(workingDirectory, @$"{TEMP_TEST_RUNNER_DIR}");
        if (Directory.Exists(destinationFolderPath))
            return;
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Installing GdUnit4 `TestRunner` at {destinationFolderPath}...");
        InstallTestRunnerClasses(destinationFolderPath);
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @"--path . --headless --build-solutions --quit-after 1000")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = @$"{workingDirectory}"
        };

        try
        {
            using Process process = new();
            process.StartInfo = processStartInfo;
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Rebuild ... {GodotBin} {processStartInfo.Arguments} at {workingDirectory}");
            process.Start();
            while (!process.WaitForExit(5000))
                Thread.Sleep(500);
            process.Kill(true);
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"GdUnit4 `TestRunner` successfully installed: {process.ExitCode}");
        }
        catch (Exception e)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Install GdUnit4 `TestRunner` fails with: {e.Message}");
        }
    }

    private static void InstallTestRunnerClasses(string destinationFolderPath)
    {
        Directory.CreateDirectory(destinationFolderPath);
        var srcTestRunner = """
                            namespace GdUnit4.TestAdapter;

                            public partial class TestAdapterRunner : Api.TestRunner
                            {
                                public override void _Ready()
                                    => _ = RunTests();
                            }

                            """;

        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.cs"), srcTestRunner);

        var srcTestRunnerScene = """
                                 [gd_scene load_steps=2 format=3 uid="uid://5o7l4yufw1rw"]

                                 [ext_resource type="Script" path="res://gdunit4_testadapter/TestAdapterRunner.cs" id="1"]

                                 [node name="Control" type="Control"]
                                 layout_mode = 3
                                 anchors_preset = 0
                                 script = ExtResource("1")

                                 """;
        File.WriteAllText(Path.Combine(destinationFolderPath, "TestAdapterRunner.tscn"), srcTestRunnerScene);
    }

    private void CollectGodotLogFile(IFrameworkHandle frameworkHandle)
    {
        var godotProjectFile = Path.Combine(Directory.GetCurrentDirectory(), "project.godot");
        if (!File.Exists(godotProjectFile))
            return;

        frameworkHandle.SendMessage(TestMessageLevel.Informational, "Append Godot logfile to the Results.");
        try
        {
            var projectSettings = GodotProjectSettings.LoadFromFile(godotProjectFile);
            var godotLogFile = projectSettings.Debug.FileLogging.LogPath;
            if (!File.Exists(godotLogFile))
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Can't copy the Godot logfile, it is not found at: {godotLogFile}");
                return;
            }

            var resultDir = Path.Combine(Directory.GetCurrentDirectory(), ResultsDirectory);
            if (!Directory.Exists(resultDir))
                Directory.CreateDirectory(resultDir);

            var godotLogFileCopy = Path.Combine(Directory.GetCurrentDirectory(), ResultsDirectory, "godot.log");
            File.Copy(godotLogFile, godotLogFileCopy, true);
        }
        catch (Exception e)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, $"Can't copy the Godot logfile: {e.Message}");
        }
    }
}
