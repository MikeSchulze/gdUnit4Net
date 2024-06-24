namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.Win32.SafeHandles;

using Settings;

internal sealed class TestExecutor : BaseTestExecutor, ITestExecutor
{
    private const string TempTestRunnerDir = "gdunit4_testadapter";
    private readonly GdUnit4Settings gdUnit4Settings;

    private Process? pProcess;

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

    private object CancelLock { get; } = new();
    private object ProcessLock { get; } = new();

#pragma warning disable IDE0052 // Remove unread private members
    private int ParallelTestCount { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

    private int SessionTimeOut { get; }

    public void Cancel()
    {
        lock (CancelLock)
        {
            Console.WriteLine("Cancel triggered");
            try
            {
                pProcess?.Kill(true);
                pProcess?.WaitForExit();
            }
            catch (Exception)
            {
                //fh.SendMessage(TestMessageLevel.Error, @$"TestRunner ends with: {e.Message}");
            }
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

        InstallTestRunnerAndBuild(frameworkHandle, workingDirectory);
        var configName = WriteTestRunnerConfig(groupedTests);
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
            if (runContext.IsBeingDebugged && frameworkHandle is IFrameworkHandle2 fh2 && fh2.GetType().ToString().Contains("JetBrains") &&
                fh2.GetType().Assembly.GetName().Version >= new Version("2.16.1.14"))
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"JetBrains Rider detected {fh2.GetType().Assembly.GetName().Version}");
                RunDebugRider(fh2, processStartInfo);
                File.Delete(configName);
            }
            else
                using (pProcess = new Process { StartInfo = processStartInfo })
                    try
                    {
                        pProcess.EnableRaisingEvents = true;
                        pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
                        pProcess.Exited += ExitHandler(frameworkHandle);
                        pProcess.Start();
                        pProcess.BeginErrorReadLine();
                        pProcess.BeginOutputReadLine();
                        AttachDebuggerIfNeed(runContext, frameworkHandle, pProcess);
                        pProcess.WaitForExit(SessionTimeOut);
                        frameworkHandle.SendMessage(TestMessageLevel.Informational, @$"Run TestRunner ends with {pProcess.ExitCode}");
                        pProcess.Kill(true);
                    }
                    catch (Exception e)
                    {
                        frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Run TestRunner ends with: {e.Message}");
                    }
                    finally { File.Delete(configName); }

        // wait until all event messages are processed or the client is disconnected
        testEventServerTask.Wait(TimeSpan.FromSeconds(2));
    }

    private void RunDebugRider(IFrameworkHandle2 fh2, ProcessStartInfo psi)
    {
        // EnableShutdownAfterTestRun is not working we need to use SafeHandle the get the process running until the ExitCode is getted
        fh2.EnableShutdownAfterTestRun = true;
        Console.WriteLine($"Debug process started {psi.FileName} {psi.WorkingDirectory} {psi.Arguments}");
        var processId = fh2.LaunchProcessWithDebuggerAttached(psi.FileName, psi.WorkingDirectory, psi.Arguments, psi.Environment);
        pProcess = Process.GetProcessById(processId);
        SafeProcessHandle? processHandle = null;
        try
        {
            processHandle = pProcess.SafeHandle;
            var isExited = pProcess.WaitForExit(SessionTimeOut);
            // it never exits on macOS ?
            Console.WriteLine($"Process exited: HasExited: {pProcess.HasExited} {isExited} {processHandle}");
            // enforce kill the process has also no affect on macOS
            pProcess.Kill(true);
            Console.WriteLine($"Process exited: HasExited: {pProcess.HasExited} {processHandle.IsClosed}");
            // this line fails on macOS, maybe the SafeHandle works only on windows
            //fh2.SendMessage(TestMessageLevel.Informational, @$"Run TestRunner ends with {pProcess.ExitCode}");
        }
        finally
        {
            processHandle?.Dispose();
        }
    }

    private void InstallTestRunnerAndBuild(IFrameworkHandle frameworkHandle, string workingDirectory)
    {
        var destinationFolderPath = Path.Combine(workingDirectory, @$"{TempTestRunnerDir}");
        if (Directory.Exists(destinationFolderPath))
            return;
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Installing GdUnit4 `TestRunner` at {destinationFolderPath}...");
        InstallTestRunnerClasses(destinationFolderPath);
        var processStartInfo = new ProcessStartInfo(@$"{GodotBin}", @"--path . --headless --build-solutions --quit-after 20")
        {
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            RedirectStandardInput = false,
            UseShellExecute = false,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            WorkingDirectory = @$"{workingDirectory}"
        };

        using Process process = new() { StartInfo = processStartInfo };
        frameworkHandle.SendMessage(TestMessageLevel.Informational, @"Rebuild ...");
        process.Start();
        while (!process.WaitForExit(5000))
            Thread.Sleep(100);
        try
        {
            process.Kill(true);
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"GdUnit4 `TestRunner` successfully installed: {process.ExitCode}");
        }
        catch (Exception e)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, @$"Install GdUnit4 `TestRunner` ends with: {e.Message}");
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
}
