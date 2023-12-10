using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
using Newtonsoft.Json;
using Godot;
using System.IO;

namespace GdUnit4.TestAdapter;

[ExtensionUri(ExecutorUri)]
public class GdUnit4TestExecutor : ITestExecutor
{

    private Process? pProcess = null;

    ///<summary>
    /// The Uri used to identify the NUnitExecutor
    ///</summary>
    public const string ExecutorUri = "executor://GdUnit4.TestAdapter/v1";


    /// <summary>
    /// Runs only the tests specified by parameter 'tests'. 
    /// </summary>
    /// <param name="tests">Tests to be run.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        _ = tests ?? throw new Exception("Argument 'tests' is null, abort!");
        _ = runContext ?? throw new Exception("Argument 'runContext' is null abort!");
        _ = frameworkHandle ?? throw new Exception("Argument 'frameworkHandle' is null, abort!");
        var godotBin = System.Environment.GetEnvironmentVariable("GODOT_BIN")
            ?? throw new Exception("Godot runtime is not set! Set evn 'GODOT_BIN' is missing!");
        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"RunConfiguration: {runConfiguration.TestSessionTimeout}");
        var settings = XmlRunSettingsUtilities.GetTestRunParameters(runContext.RunSettings?.SettingsXml);

        foreach (var key in settings.Keys)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{key} = '{settings[key]}'");
        }

        Dictionary<string, List<TestCase>> groupedTests = tests
            .GroupBy(t => t.CodeFilePath!)
            .ToDictionary(group => group.Key, group => group.ToList());

        //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"RunTests {groupedTests.Keys.Formated()}");

        foreach (var key in groupedTests.Keys)
        {
            var classPath = key;
            List<TestCase> testCases = groupedTests[key];

            var workingDirectory = LookupGodotProjectPath(classPath);

            using (pProcess = new())
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Execute test's on: {classPath}");
                pProcess.StartInfo.WorkingDirectory = @$"{workingDirectory}";
                pProcess.StartInfo.FileName = @$"{godotBin}";
                pProcess.StartInfo.Arguments = @$"-d --path {workingDirectory} --testadapter --testsuites='{classPath} --verbose'";
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.RedirectStandardInput = true;
                pProcess.EnableRaisingEvents = true;
                pProcess.OutputDataReceived += TestEventProcessor(frameworkHandle, testCases);
                pProcess.ErrorDataReceived += StdErrorProcessor(frameworkHandle);
                pProcess.Exited += ExitHandler(frameworkHandle);
                pProcess.Start();
                AttachDebugerIfNeed(runContext, frameworkHandle, pProcess);

                pProcess.BeginErrorReadLine();
                pProcess.BeginOutputReadLine();
                //pProcess.WaitForExit((int)runConfiguration.TestSessionTimeout);
                pProcess.WaitForExit();
            };

        }
    }

    private void AttachDebugerIfNeed(IRunContext runContext, IFrameworkHandle frameworkHandle, Process process)
    {
        if (runContext.IsBeingDebugged && frameworkHandle is IFrameworkHandle2 fh2)
            fh2.AttachDebuggerToProcess(pid: process.Id);
    }

    /// <summary>
    /// Runs 'all' the tests present in the specified 'containers'. 
    /// </summary>
    /// <param name="containers">Path to test container files to look for tests in.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<string>? containers, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        frameworkHandle?.SendMessage(TestMessageLevel.Warning, $"RunTests:containers ${containers}");
    }

    /// <summary>
    /// Cancel the execution of the tests.
    /// </summary>
    public void Cancel()
    {
        if (pProcess != null)
        {
            pProcess.Refresh();
            if (pProcess.HasExited)
                return;
            pProcess.Kill(true);
        }
    }


    private static EventHandler ExitHandler(IFrameworkHandle frameworkHandle) => new((sender, e)
        => frameworkHandle.SendMessage(TestMessageLevel.Informational, "Exited: " + e.GetType()));

    private static DataReceivedEventHandler StdErrorProcessor(IFrameworkHandle frameworkHandle) => new((sender, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;
        frameworkHandle.SendMessage(TestMessageLevel.Error, $"stderr: {message}");
    });

    private static DataReceivedEventHandler TestEventProcessor(IFrameworkHandle frameworkHandle, IEnumerable<TestCase> tests) => new((sender, args) =>
    {
        var json = args.Data?.Trim();
        if (string.IsNullOrEmpty(json))
            return;

        if (json.StartsWith("GdUnitTestEvent:"))
        {
            json = json.TrimPrefix("GdUnitTestEvent:");
            TestEvent e = JsonConvert.DeserializeObject<TestEvent>(json)!;

            switch (e.Type)
            {
                case TestEvent.TYPE.TESTSUITE_BEFORE:
                    {
                        //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Run Test Suite {e.SuiteName}");
                    }
                    break;
                case TestEvent.TYPE.TESTCASE_BEFORE:
                    {
                        var testCase = tests.FirstOrDefault(t => t.DisplayName == e.TestName);
                        if (testCase == null)
                        {
                            frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case {e.TestName}");
                            return;
                        }
                        frameworkHandle.RecordStart(testCase);
                    }
                    break;
                case TestEvent.TYPE.TESTCASE_AFTER:
                    {
                        var testCase = tests.FirstOrDefault(t => t.DisplayName == e.TestName);
                        if (testCase == null)
                        {
                            frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {e.TestName}");
                            return;
                        }
                        var testResult = new TestResult(testCase)
                        {
                            DisplayName = testCase.DisplayName,
                            Outcome = e.AsTestOutcome(),
                            EndTime = DateTimeOffset.Now,
                            Duration = e.ElapsedInMs
                        };
                        foreach (var report in e.Reports)
                        {
                            testResult.ErrorMessage = report.Message.RichTextNormalize();
                            testResult.ErrorStackTrace = $"StackTrace    at {testCase.FullyQualifiedName}() in {testCase.CodeFilePath}:line {report.LineNumber}";
                        }
                        frameworkHandle.RecordResult(testResult);
                        frameworkHandle.RecordEnd(testCase, testResult.Outcome);
                    }
                    break;
                case TestEvent.TYPE.TESTSUITE_AFTER:
                    {
                        //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Run Test Suite {e.SuiteName} {e.AsTestOutcome()}");
                    }
                    break;
            }
            return;
        }
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"stdout: {json}");
    });

    private static string? LookupGodotProjectPath(string classPath)
    {
        DirectoryInfo? currentDir = new DirectoryInfo(classPath).Parent;
        while (currentDir != null)
        {
            if (currentDir.EnumerateFiles("project.godot").Any())
                return currentDir.FullName;
            currentDir = currentDir.Parent;
        }
        return null;
    }
}
