namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;

using Api;

using Core.Events;
using Core.Extensions;
using Core.Reporting;

using Extensions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Utilities;

using static Api.ITestEvent.EventType;

using TestCase = Microsoft.VisualStudio.TestPlatform.ObjectModel.TestCase;

internal sealed class TestEventReportListener : ITestEventListener
{
    public TestEventReportListener(IFrameworkHandle framework, IReadOnlyList<TestCase> testCases)
    {
        Framework = framework;
        TestCases = testCases;
        DetailedOutput = new[] { Ide.VisualStudio, Ide.VisualStudioCode, Ide.JetBrainsRider }.Contains(IdeType);
        framework.SendMessage(TestMessageLevel.Informational, $"Detected IDE {IdeType}");
    }

    private IFrameworkHandle Framework { get; }
    private IReadOnlyList<TestCase> TestCases { get; }
    private bool DetailedOutput { get; }

    private Ide IdeType => IdeDetector.Detect(Framework);
    public int CompletedTests { get; set; }
    public bool IsFailed { get; set; }

    public void Dispose()
    {
    }

    public void PublishEvent(ITestEvent testEvent)
    {
        switch (testEvent.Type)
        {
            case SuiteBefore:
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestSuite: {testEvent.FullyQualifiedName} Processing...");
                break;

            case TestBefore:
            {
                var testCase = FindTestCase(testEvent);
                if (testCase == null)
                {
                    // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                    if (FindParameterizedTestCase(testEvent))
                        return;
                    Framework.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case Id: {testEvent.Id}");
                    return;
                }

                Framework.RecordStart(testCase);
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestCase: {testCase.FullyQualifiedName} Processing...");
                break;
            }

            case TestAfter:
            {
                var testCase = FindTestCase(testEvent);
                if (testCase == null)
                {
                    // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                    if (FindParameterizedTestCase(testEvent))
                        return;
                    Framework.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {testEvent.FullyQualifiedName}");
                    return;
                }

                var testResult = new TestResult(testCase)
                {
                    DisplayName = IdeType == Ide.DotNet ? testCase.FullyQualifiedName : testCase.DisplayName,
                    Outcome = testEvent.AsTestOutcome(),
                    EndTime = DateTimeOffset.Now,
                    Duration = testEvent.ElapsedInMs
                };

                (testEvent as TestEvent)?.Reports.ForEach(report => AddTestReport(report, testResult));

                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestCase: {testCase.FullyQualifiedName} {testResult.Outcome}");
                Framework.RecordResult(testResult);
                Framework.RecordEnd(testCase, testResult.Outcome);
                CompletedTests += 1;
                break;
            }

            case SuiteAfter:
                if (DetailedOutput)
                    Framework.SendMessage(TestMessageLevel.Informational, $"TestSuite: {testEvent.FullyQualifiedName}: {testEvent.AsTestOutcome()}\n");
                break;

            case Init:
                break;
            case Stop:
                break;
        }
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
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd().Replace("WARNING:", "Warning:").Replace("ERROR:", "Error:");

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                foreach (var message in normalizedMessage.Split("\n"))
                    Framework.SendMessage(TestMessageLevel.Informational, HtmlEncoder.Default.Encode($"    {message}"));
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

    private TestCase? FindTestCase(ITestEvent e)
        => TestCases.FirstOrDefault(t => e.Id.Equals(t.Id));

    private bool FindParameterizedTestCase(ITestEvent e)
        => TestCases.Any(t => t.FullyQualifiedName.StartsWith(e.FullyQualifiedName, StringComparison.Ordinal));
}
