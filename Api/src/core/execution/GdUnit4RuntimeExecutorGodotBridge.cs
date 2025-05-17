// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Extensions;

using Godot;
using Godot.Collections;

using Newtonsoft.Json;

using Runners;

internal class GdUnit4RuntimeExecutorGodotBridge
{
    private TestEngineSettings Settings { get; } = new();

#pragma warning disable CA1859
    private ITestEngineLogger Logger { get; } = new GodotLogger();
#pragma warning restore CA1859

    public async Task ExecuteAsync(List<TestSuiteNode> testSuiteNodes, Callable listener, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Run(
                    async () =>
                    {
                        var testListener = new GdUnit4TestEventListener(listener);

                        foreach (var testSuiteNode in testSuiteNodes)
                        {
                            var response = await new ExecuteTestSuiteCommand(testSuiteNode, Settings.CaptureStdOut, true)
                                .Execute(testListener);
                            ValidateResponse(response);
                        }
                    }, cancellationToken)
                .WaitAsync(cancellationToken);
        }
        catch (TimeoutException)
        {
            Logger.LogError("Failed to connect: Connection timeout");
        }
        catch (OperationCanceledException)
        {
            Logger.LogInfo("Running tests are cancelled.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
        }
    }

    private static void ValidateResponse(Response response)
    {
        if (response.StatusCode != HttpStatusCode.InternalServerError)
            return;
        var exception = JsonConvert.DeserializeObject<Exception>(response.Payload);
        throw new InvalidOperationException("The GdUnit test server returned an unexpected status code.", exception);
    }
}

internal class GdUnit4TestEventListener : ITestEventListener
{
    private readonly Callable listener;

    internal GdUnit4TestEventListener(Callable listener) => this.listener = listener;

    public int CompletedTests { get; set; }

    public bool IsFailed
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void PublishEvent(ITestEvent testEvent) => EmitTestEvent(testEvent as TestEvent);

    private void EmitTestEvent(TestEvent? testEvent)
    {
        if (testEvent == null)
            return;

        var data = new Dictionary
        {
            { "type", testEvent.Type.ToVariant() },
            { "guid", testEvent.Id.ToString() },
            { "resource_path", testEvent.ResourcePath.ToVariant() },
            { "suite_name", testEvent.SuiteName.ToVariant() },
            { "test_name", testEvent.TestName.ToVariant() },
            { "total_count", testEvent.TotalCount.ToVariant() },
            { "statistics", ToGdUnitEventStatistics(testEvent.Statistics) }
        };

        if (testEvent.Reports.Count != 0)
        {
            var serializedReports = testEvent.Reports.Select(report => report.Serialize()).ToGodotArray();
            data.Add("reports", serializedReports);
        }

        listener.Call(data);
    }

    private static Godot.Collections.Dictionary<Variant, Variant> ToGdUnitEventStatistics(IDictionary<TestEvent.STATISTIC_KEY, object> statistics)
    {
        var converted = new Godot.Collections.Dictionary<Variant, Variant>();
        foreach (var (key, value) in statistics)
            converted[key.ToString().ToLower().ToVariant()] = value.ToVariant();
        return converted;
    }
}
