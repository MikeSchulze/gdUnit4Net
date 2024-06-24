namespace GdUnit4.TestAdapter.Execution;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Extensions;

using Godot;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Newtonsoft.Json;

internal sealed class TestEventReportServer : IDisposable, IAsyncDisposable
{
    private readonly NamedPipeServerStream server;

    public TestEventReportServer()
        => server = new NamedPipeServerStream(TestAdapterReporter.PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

    public async ValueTask DisposeAsync() => await server.DisposeAsync();

    public void Dispose() => server.Dispose();

    internal async Task Start(IFrameworkHandle frameworkHandle, IReadOnlyList<TestCase> tests)
    {
        frameworkHandle.SendMessage(TestMessageLevel.Informational, "TestEventReportServer:: Wait for connecting GdUnit4 test report client.");
        await server.WaitForConnectionAsync();
        using CancellationTokenSource tokenSource = new(TimeSpan.FromMinutes(10));
        using var reader = new StreamReader(server);
        while (server.IsConnected)
            try
            {
                var json = await reader.ReadLineAsync(tokenSource.Token);
                if (string.IsNullOrEmpty(json)) continue;

                ProcessTestEvent(frameworkHandle, tests, json);
            }
            catch (IOException)
            {
                frameworkHandle.SendMessage(TestMessageLevel.Informational, "TestEventReportServer:: Client disconnected.");
                break;
            }
            catch (Exception ex)
            {
                if (server.IsConnected)
                    frameworkHandle.SendMessage(TestMessageLevel.Error, $"TestEventReportServer:: Error: {ex.Message}");
            }
    }

    private void ProcessTestEvent(IFrameworkHandle frameworkHandle, IReadOnlyList<TestCase> tests, string json)
    {
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
                        //frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_BEFORE: cant find test case {e.FullyQualifiedName}");
                        return;
                    frameworkHandle.RecordStart(testCase);
                    break;
                }
                case TestEvent.TYPE.TESTCASE_AFTER:
                {
                    var testCase = FindTestCase(tests, e);
                    if (testCase == null)
                        //frameworkHandle.SendMessage(TestMessageLevel.Error, $"TESTCASE_AFTER: cant find test case {e.FullyQualifiedName}");
                        return;
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
                    break;
                }
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

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"TestEventReportServer:: {json}");
    }

    private static TestCase? FindTestCase(IEnumerable<TestCase> tests, TestEvent e)
        => tests.FirstOrDefault(t => e.FullyQualifiedName.Equals(t.FullyQualifiedName, StringComparison.Ordinal));
}
