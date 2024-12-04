namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Core.Events;
using Core.Extensions;
using Core.Reporting;

using Extensions;

using Godot;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Newtonsoft.Json;

using Utilities;

internal sealed class TestEventReportServer : IAsyncDisposable, ITestEventListener
{
    private readonly NamedPipeServerStream server = new(TestAdapterReporter.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

    public TestEventReportServer(IFrameworkHandle framework, IReadOnlyList<TestCase> testCases)
    {
        Framework = framework;
        TestCases = testCases;
        DetailedOutput = new[] { Ide.VisualStudio, Ide.VisualStudioCode, Ide.JetBrainsRider }.Contains(IdeDetector.Detect(Framework));
    }

    private IFrameworkHandle Framework { get; }
    private IReadOnlyList<TestCase> TestCases { get; }
    internal int CompletedTests { get; private set; }
    private bool DetailedOutput { get; }

    public async ValueTask DisposeAsync()
    {
        if (server.IsConnected)
            server.Close();
        await server.DisposeAsync();
    }

    public bool IsFailed { get; set; }

    public void Dispose()
    {
        if (server.IsConnected)
            server.Close();
        server.Dispose();
    }

    public void PublishEvent(TestEvent e)
    {
        switch (e.Type)
        {
            case TestEvent.TYPE.TESTSUITE_BEFORE:
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestSuite: {e.FullyQualifiedName} Processing...");
                break;

            case TestEvent.TYPE.TESTCASE_BEFORE:
            {
                var testCase = FindTestCase(e);
                if (testCase == null)
                {
                    // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                    if (FindParameterizedTestCase(e))
                        return;
                    Framework.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case {e.FullyQualifiedName}");
                    return;
                }

                Framework.RecordStart(testCase);
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestCase: {e.FullyQualifiedName} Processing...");
                break;
            }

            case TestEvent.TYPE.TESTCASE_AFTER:
            {
                var testCase = FindTestCase(e);
                if (testCase == null)
                {
                    // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                    if (FindParameterizedTestCase(e))
                        return;
                    Framework.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {e.FullyQualifiedName}");
                    return;
                }

                var testResult = new TestResult(testCase)
                {
                    DisplayName = testCase.DisplayName,
                    Outcome = e.AsTestOutcome(),
                    EndTime = DateTimeOffset.Now,
                    Duration = e.ElapsedInMs
                };

                e.Reports.ForEach(report => AddTestReport(report, testResult));

                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestCase: {testCase.DisplayName} {testResult.Outcome}\n");
                Framework.RecordResult(testResult);
                Framework.RecordEnd(testCase, testResult.Outcome);
                CompletedTests += 1;
                break;
            }

            case TestEvent.TYPE.TESTSUITE_AFTER:
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestSuite: {e.FullyQualifiedName}: {e.AsTestOutcome()}\n");
                break;

            case TestEvent.TYPE.INIT:
                break;
            case TestEvent.TYPE.STOP:
                break;
        }
    }


    internal async Task Start()
    {
        Framework.SendMessage(TestMessageLevel.Informational, "GdUnit4.TestEventReportServer:: Wait for connecting GdUnit4 test report client.");
        await server.WaitForConnectionAsync();

        Framework.SendMessage(TestMessageLevel.Informational, $"GdUnit4.TestEventReportServer:: Connected. {server.GetImpersonationUserName()}");


        using CancellationTokenSource tokenSource = new(TimeSpan.FromMinutes(10));
        using var reader = new StreamReader(server);

        while (server.IsConnected)
            try
            {
                if (tokenSource.Token.IsCancellationRequested)
                {
                    Framework.SendMessage(TestMessageLevel.Warning, "GdUnit4.TestEventReportServer:: Operation timed out.");
                    break;
                }

                var json = await reader.ReadLineAsync(tokenSource.Token);
                if (string.IsNullOrEmpty(json))
                    continue;

                ProcessTestEvent(json);
            }
            catch (IOException e)
            {
                Framework.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: Client has disconnected by '{e.Message}'");
                break;
            }
            catch (Exception ex)
            {
                if (server.IsConnected)
                {
                    Framework.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: {ex.Message}");
                    Framework.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: StackTrace: {ex.StackTrace}");
                    break;
                }
            }

        Framework.SendMessage(TestMessageLevel.Informational, "GdUnit4.TestEventReportServer:: Disconnected.");
    }

    private void ProcessTestEvent(string json)
    {
        if (!json.StartsWith("GdUnitTestEvent:"))
        {
            Framework.SendMessage(TestMessageLevel.Informational, $"GdUnit4.TestEventReportServer:: {json}");
            return;
        }

        json = json.TrimPrefix("GdUnitTestEvent:");
        var e = JsonConvert.DeserializeObject<TestEvent>(json)!;
        PublishEvent(e);
    }

// ReSharper disable once UnusedMethodReturnValue.Local
    private TestResult AddTestReport(TestReport report, TestResult testResult) => IdeDetector.Detect(Framework) switch
    {
        Ide.JetBrainsRider => AddRiderTestReport(report, testResult),
        Ide.VisualStudio => AddVisualStudio2022TestReport(report, testResult),
        Ide.VisualStudioCode => AddVisualStudioCodeTestReport(report, testResult),
        Ide.Unknown => AddDefaultTestReport(report, testResult),
        _ => AddDefaultTestReport(report, testResult)
    };

    private TestResult AddRiderTestReport(TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                Framework.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage.FormatMessageColored(report.Type)));
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                normalizedMessage = normalizedMessage.Replace("WARNING:\n", "");
                testResult.ErrorMessage = "Warning Detected!";
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, normalizedMessage.FormatMessageColored(report.Type)));
                Framework.SendMessage(TestMessageLevel.Warning, $"Warning:\n{normalizedMessage.Indent()}");
                break;
            case TestReport.ReportType.SUCCESS:
                break;
            case TestReport.ReportType.FAILURE:
            case TestReport.ReportType.TERMINATED:
            case TestReport.ReportType.INTERRUPTED:
            case TestReport.ReportType.ABORT:
            default:
                testResult.ErrorMessage = normalizedMessage;
                testResult.ErrorStackTrace = report.StackTrace;
                Framework.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddVisualStudio2022TestReport(TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                normalizedMessage = normalizedMessage.Replace("WARNING:", "Warning:");
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Warning, normalizedMessage);
                break;

            case TestReport.ReportType.SUCCESS:
                break;
            case TestReport.ReportType.FAILURE:
            case TestReport.ReportType.TERMINATED:
            case TestReport.ReportType.INTERRUPTED:
            case TestReport.ReportType.ABORT:
            default:
                testResult.ErrorMessage = normalizedMessage;
                testResult.ErrorStackTrace = report.StackTrace;
                Framework.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddVisualStudioCodeTestReport(TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                testResult.ErrorMessage = normalizedMessage;
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Warning, $"{normalizedMessage.Replace("WARNING:", "Warning:")}");
                break;
            case TestReport.ReportType.SUCCESS:
                break;
            case TestReport.ReportType.FAILURE:
            case TestReport.ReportType.TERMINATED:
            case TestReport.ReportType.INTERRUPTED:
            case TestReport.ReportType.ABORT:
            default:
                testResult.ErrorMessage = normalizedMessage;
                testResult.ErrorStackTrace = report.StackTrace;
                Framework.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddDefaultTestReport(TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd().Replace("WARNING:", "Warning:");

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Informational, "Standard Output:");
                foreach (var message in normalizedMessage.Split("\n")) Framework.SendMessage(TestMessageLevel.Informational, $"stdout:    {message}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                testResult.ErrorMessage = normalizedMessage;
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                Framework.SendMessage(TestMessageLevel.Warning, normalizedMessage);
                break;
            case TestReport.ReportType.SUCCESS:
                break;
            case TestReport.ReportType.FAILURE:
            case TestReport.ReportType.TERMINATED:
            case TestReport.ReportType.INTERRUPTED:
            case TestReport.ReportType.ABORT:
            default:
                testResult.ErrorMessage = normalizedMessage;
                testResult.ErrorStackTrace = report.StackTrace;
                Framework.SendMessage(TestMessageLevel.Error, normalizedMessage);
                break;
        }

        return testResult;
    }


    private TestCase? FindTestCase(TestEvent e)
        => TestCases.FirstOrDefault(t => e.FullyQualifiedName.Equals(t.FullyQualifiedName, StringComparison.Ordinal));

    private bool FindParameterizedTestCase(TestEvent e)
        => TestCases.Any(t => t.FullyQualifiedName.StartsWith(e.FullyQualifiedName, StringComparison.Ordinal));
}
