namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Newtonsoft.Json;

using Api;
using Extensions;
using Godot;

internal abstract class BaseTestExecutor
{

    protected static string GodotBin { get; set; } = System.Environment.GetEnvironmentVariable("GODOT_BIN")
        ?? throw new ArgumentNullException("Godot runtime is not set! Set env 'GODOT_BIN' is missing!");

    protected static EventHandler ExitHandler(IFrameworkHandle frameworkHandle) => new((sender, e) =>
    {
        Console.Out.Flush();
        if (sender is Process p)
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Godot ends with exit code: {p.ExitCode}");
    });

    protected static DataReceivedEventHandler StdErrorProcessor(IFrameworkHandle frameworkHandle) => new((sender, args) =>
    {
        var message = args.Data?.Trim();
        if (string.IsNullOrEmpty(message))
            return;
        // we do log errors to stdout otherwise running `dotnet test` from console will fail with exit code 1
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Error: {message}");
    });

    protected static string WriteTestRunnerConfig(Dictionary<string, List<TestCase>> groupedTestSuites)
    {
        try
        { CleanupRunnerConfigurations(); }
        catch (Exception) { }

        var fileName = $"GdUnitRunner_{Guid.NewGuid()}.cfg";
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

        var testConfig = new TestRunnerConfig
        {
            Included = groupedTestSuites.ToDictionary(
                suite => suite.Key,
                suite => suite.Value.Select(t => new TestCaseConfig { Name = t.GetPropertyValue(TestCaseExtensions.TestCaseNameProperty, t.FullyQualifiedName) })
            )
        };

        File.WriteAllText(filePath, JsonConvert.SerializeObject(testConfig, Formatting.Indented));
        return filePath;
    }

    private static void CleanupRunnerConfigurations()
        => Directory.GetFiles(Directory.GetCurrentDirectory(), "GdUnitRunner_*.cfg")
            .ToList()
            .ForEach(File.Delete);

    protected static void AttachDebuggerIfNeed(IRunContext runContext, IFrameworkHandle frameworkHandle, Process process)
    {
        if (!runContext.IsBeingDebugged)
            return;
        if (frameworkHandle is IFrameworkHandle2 fh2)
            fh2.AttachDebuggerToProcess(pid: process.Id);
        else
        {
            if (frameworkHandle.ToString()!.Contains("JetBrains"))
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Detected JetBrains debugger run.");
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Attaching the debugger to the process... {process}");
                // Attempt to attach the debugger to the process
                try
                {
                    //Process.Start("rider", $"attach-to-process {process.Id} \"{FindSolutionFile()}\"");

                }
                catch (Exception ex)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error, $"Failed to attach the debugger: {ex.Message}");
                }
            }
        }
    }

    private static string FindSolutionFile([CallerFilePath] string? callerFile = null)
    {
        var dir = Path.GetDirectoryName(callerFile) ?? throw new InvalidOperationException("could not resolve solution directory");
        do
        {
            var slnFiles = Directory.GetFiles(dir, "*.sln", SearchOption.TopDirectoryOnly);
            if (slnFiles.Length == 1)
                return slnFiles[0];

            dir = Path.GetDirectoryName(dir);
        } while (dir != null);

        throw new InvalidOperationException("Could not find solution");
    }

    protected static DataReceivedEventHandler TestEventProcessor(IFrameworkHandle frameworkHandle, IEnumerable<TestCase> tests) => new((sender, args) =>
    {
        var json = args.Data?.Trim();
        if (string.IsNullOrEmpty(json))
            return;

        if (json.StartsWith("GdUnitTestEvent:"))
        {
            json = json.TrimPrefix("GdUnitTestEvent:");
            var e = JsonConvert.DeserializeObject<TestEvent>(json)!;

            switch (e.Type)
            {
                case TestEvent.TYPE.TESTSUITE_BEFORE:
                    //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Execute Test Suite '{e.SuiteName}'");
                    break;
                case TestEvent.TYPE.TESTCASE_BEFORE:
                {
                    var testCase = FindTestCase(tests, e);
                    if (testCase == null)
                    {
                        //frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case {e.FullyQualifiedName}");
                        return;
                    }
                    frameworkHandle.RecordStart(testCase);
                }
                break;
                case TestEvent.TYPE.TESTCASE_AFTER:
                {
                    var testCase = FindTestCase(tests, e);
                    if (testCase == null)
                    {
                        //frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {e.FullyQualifiedName}");
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
                        testResult.ErrorStackTrace = report.StackTrace;
                    }
                    frameworkHandle.RecordResult(testResult);
                    frameworkHandle.RecordEnd(testCase, testResult.Outcome);
                }
                break;
                case TestEvent.TYPE.TESTSUITE_AFTER:
                    //frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{e.AsTestOutcome()}");
                    break;
                case TestEvent.TYPE.INIT:
                    break;
                case TestEvent.TYPE.STOP:
                    break;
            }
            return;
        }
        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"stdout: {json}");
    });

    private static TestCase? FindTestCase(IEnumerable<TestCase> tests, TestEvent e)
        => tests.FirstOrDefault(t => e.FullyQualifiedName.Equals(t.FullyQualifiedName, StringComparison.Ordinal));

    protected static string? LookupGodotProjectPath(string classPath)
    {
        var currentDir = new DirectoryInfo(classPath).Parent;
        while (currentDir != null)
        {
            if (currentDir.EnumerateFiles("project.godot").Any())
                return currentDir.FullName;
            currentDir = currentDir.Parent;
        }
        return null;
    }
}
