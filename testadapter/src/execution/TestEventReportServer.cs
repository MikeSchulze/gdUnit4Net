namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Core.Extensions;

using Extensions;

using Godot;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Newtonsoft.Json;

using Utilities;

internal sealed class TestEventReportServer : IDisposable, IAsyncDisposable
{
    private readonly NamedPipeServerStream server = new(TestAdapterReporter.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

    public async ValueTask DisposeAsync()
    {
        if (server.IsConnected)
            server.Close();
        await server.DisposeAsync();
    }

    public void Dispose()
    {
        if (server.IsConnected)
            server.Close();
        server.Dispose();
    }

    internal async Task Start(IFrameworkHandle frameworkHandle, IReadOnlyList<TestCase> tests)
    {
        var detailedOutput = new[] { Ide.VisualStudio, Ide.VisualStudioCode, Ide.JetBrainsRider }.Contains(IdeDetector.Detect(frameworkHandle));
        frameworkHandle.SendMessage(TestMessageLevel.Informational, "GdUnit4.TestEventReportServer:: Wait for connecting GdUnit4 test report client.");
        await server.WaitForConnectionAsync();

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"GdUnit4.TestEventReportServer:: Connected. {server.GetImpersonationUserName()}");


        using CancellationTokenSource tokenSource = new(TimeSpan.FromMinutes(10));
        using var reader = new StreamReader(server);

        while (server.IsConnected)
            try
            {
                if (tokenSource.Token.IsCancellationRequested)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Warning, "GdUnit4.TestEventReportServer:: Operation timed out.");
                    break;
                }

                var json = await reader.ReadLineAsync(tokenSource.Token);
                if (string.IsNullOrEmpty(json))
                    continue;

                ProcessTestEvent(frameworkHandle, tests, json, detailedOutput);
            }
            catch (IOException e)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: Client has disconnected by '{e.Message}'");
                break;
            }
            catch (Exception ex)
            {
                if (server.IsConnected)
                {
                    frameworkHandle.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: {ex.Message}");
                    frameworkHandle.SendMessage(TestMessageLevel.Error, $"GdUnit4.TestEventReportServer:: StackTrace: {ex.StackTrace}");
                    break;
                }
            }

        frameworkHandle.SendMessage(TestMessageLevel.Informational, "GdUnit4.TestEventReportServer:: Disconnected.");
    }


    private void ProcessTestEvent(IFrameworkHandle frameworkHandle, IReadOnlyList<TestCase> tests, string json, bool detailedOutput)
    {
        if (json.StartsWith("GdUnitTestEvent:"))
        {
            json = json.TrimPrefix("GdUnitTestEvent:");
            var e = JsonConvert.DeserializeObject<TestEvent>(json)!;

            switch (e.Type)
            {
                case TestEvent.TYPE.TESTSUITE_BEFORE:
                    if (detailedOutput)
                        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestSuite: {e.FullyQualifiedName} Processing...");


                    break;
                case TestEvent.TYPE.TESTCASE_BEFORE:
                {
                    var testCase = FindTestCase(tests, e);
                    if (testCase == null)
                    {
                        // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                        if (FindParameterizedTestCase(tests, e))
                            return;
                        frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case {e.FullyQualifiedName}");
                        return;
                    }

                    frameworkHandle.RecordStart(testCase);
                    if (detailedOutput)
                        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestCase: {e.FullyQualifiedName} Processing...");
                    break;
                }
                case TestEvent.TYPE.TESTCASE_AFTER:
                {
                    var testCase = FindTestCase(tests, e);
                    if (testCase == null)
                    {
                        // check is the event just the parent of parameterized tests we do ignore it because all children will be executed
                        if (FindParameterizedTestCase(tests, e))
                            return;
                        frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {e.FullyQualifiedName}");
                        return;
                    }

                    var testResult = new TestResult(testCase)
                    {
                        DisplayName = testCase.DisplayName,
                        Outcome = e.AsTestOutcome(),
                        EndTime = DateTimeOffset.Now,
                        Duration = e.ElapsedInMs
                    };

                    e.Reports.ForEach(report => AddTestReport(frameworkHandle, report, testResult));

                    if (detailedOutput)
                        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestCase: {testCase.DisplayName} {testResult.Outcome}\n");
                    frameworkHandle.RecordResult(testResult);
                    frameworkHandle.RecordEnd(testCase, testResult.Outcome);
                    break;
                }
                case TestEvent.TYPE.TESTSUITE_AFTER:
                    if (detailedOutput)
                        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestSuite: {e.FullyQualifiedName}: {e.AsTestOutcome()}\n");


                    break;
                case TestEvent.TYPE.INIT:
                    break;
                case TestEvent.TYPE.STOP:
                    break;
            }

            return;
        }

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"GdUnit4.TestEventReportServer:: {json}");
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private TestResult AddTestReport(IFrameworkHandle frameworkHandle, TestReport report, TestResult testResult) => IdeDetector.Detect(frameworkHandle) switch
    {
        Ide.JetBrainsRider => AddRiderTestReport(frameworkHandle, report, testResult),
        Ide.VisualStudio => AddVisualStudio2022TestReport(frameworkHandle, report, testResult),
        Ide.VisualStudioCode => AddVisualStudioCodeTestReport(frameworkHandle, report, testResult),
        Ide.Unknown => AddDefaultTestReport(frameworkHandle, report, testResult),
        _ => AddDefaultTestReport(frameworkHandle, report, testResult)
    };

    private TestResult AddRiderTestReport(IFrameworkHandle frameworkHandle, TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage.FormatMessageColored(report.Type)));
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                normalizedMessage = normalizedMessage.Replace("WARNING:\n", "");
                testResult.ErrorMessage = "Warning Detected!";
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.AdditionalInfoCategory, normalizedMessage.FormatMessageColored(report.Type)));
                frameworkHandle.SendMessage(TestMessageLevel.Warning, $"Warning:\n{normalizedMessage.Indent()}");
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
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddVisualStudio2022TestReport(IFrameworkHandle frameworkHandle, TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                normalizedMessage = normalizedMessage.Replace("WARNING:", "Warning:");
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Warning, normalizedMessage);
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
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddVisualStudioCodeTestReport(IFrameworkHandle frameworkHandle, TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd();

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Informational, $"Standard Output:\n{normalizedMessage.Indent()}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                testResult.ErrorMessage = normalizedMessage;
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Warning, $"{normalizedMessage.Replace("WARNING:", "Warning:")}");
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
                frameworkHandle.SendMessage(TestMessageLevel.Error, $"Error:\n{normalizedMessage.Indent()}");
                break;
        }

        return testResult;
    }

    private TestResult AddDefaultTestReport(IFrameworkHandle frameworkHandle, TestReport report, TestResult testResult)
    {
        var normalizedMessage = report.Message.RichTextNormalize().TrimEnd().Replace("WARNING:", "Warning:");

        switch (report.Type)
        {
            case TestReport.ReportType.STDOUT:
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardOutCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "Standard Output:");
                foreach (var message in normalizedMessage.Split("\n")) frameworkHandle.SendMessage(TestMessageLevel.Informational, $"stdout:    {message}");
                break;

            case TestReport.ReportType.WARN:
            case TestReport.ReportType.ORPHAN:
                // for now, we report in category error
                // see https://developercommunity.visualstudio.com/t/Test-Explorer-not-show-additional-report/10768871?port=1025&fsid=1427bd7b-5ee3-4b74-9bc6-3f3f4663546c
                testResult.ErrorMessage = normalizedMessage;
                testResult.Messages.Add(new TestResultMessage(TestResultMessage.StandardErrorCategory, normalizedMessage));
                frameworkHandle.SendMessage(TestMessageLevel.Warning, normalizedMessage);
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
                frameworkHandle.SendMessage(TestMessageLevel.Error, normalizedMessage);
                break;
        }

        return testResult;
    }


    private static TestCase? FindTestCase(IEnumerable<TestCase> tests, TestEvent e)
        => tests.FirstOrDefault(t => e.FullyQualifiedName.Equals(t.FullyQualifiedName, StringComparison.Ordinal));

    private static bool FindParameterizedTestCase(IEnumerable<TestCase> tests, TestEvent e)
        => tests.Any(t => t.FullyQualifiedName.StartsWith(e.FullyQualifiedName, StringComparison.Ordinal));
}
