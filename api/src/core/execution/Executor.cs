namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Api;

using Extensions;

using Godot;

using Newtonsoft.Json;

public partial class Executor : RefCounted, IExecutor
{
    [Signal]
    public delegate void ExecutionCompletedEventHandler();

    private readonly List<ITestEventListener> eventListeners = new();

    private static bool ReportOrphanNodesEnabled => GdUnit4Settings.IsVerboseOrphans();

    public IExecutor AddGdTestEventListener(GodotObject listener)
    {
        // I want to use anonymous implementation to remove the extra delegator class
        eventListeners.Add(new GdTestEventListenerDelegator(listener));
        return this;
    }

    public bool IsExecutable(Node node) => node is CsNode;

    /// <summary>
    ///     Execute a testsuite, is called externally from Godot test suite runner
    /// </summary>
    /// <param name="testSuite"></param>
    public void Execute(CsNode testSuite)
    {
        try
        {
            var includedTests = LoadTestFilter(testSuite);
            var task = ExecuteInternally(new TestSuite(testSuite.ResourcePath(), includedTests, true, true), new TestRunnerConfig());
            // use this call as workaround to hold the signal list, it is disposed for some unknown reason.
            // could be related to https://github.com/godotengine/godot/issues/84254
            GetSignalConnectionList("ExecutionCompleted");
            task.GetAwaiter().OnCompleted(() => EmitSignal(SignalName.ExecutionCompleted));
        }
        // handle unexpected exceptions
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unexpected Exception: {e.Message} \nStackTrace: {e.StackTrace}");
        }
        finally
        {
            testSuite.Free();
        }
    }

    internal void AddTestEventListener(ITestEventListener listener)
        => eventListeners.Add(listener);

    /// <summary>
    ///     Loads the list of included tests from GdUnitRunner.cfg if exists
    /// </summary>
    /// <param name="testSuite"></param>
    /// <returns></returns>
    private static List<string>? LoadTestFilter(CsNode testSuite)
    {
        // try to load runner config written by GdUnit4 plugin
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "addons/gdUnit4/GdUnitRunner.cfg");
        if (!File.Exists(configPath))
            return null;

        var testSuitePath = testSuite.ResourcePath();
        var json = File.ReadAllText(configPath);
        var runnerConfig = JsonConvert.DeserializeObject<GdUnitRunnerConfig>(json);
        // Filter by testSuitePath and add values from runnerConfig.Included to the list
        var filteredTests = runnerConfig?.Included
            .Where(entry => entry.Key.EndsWith(testSuitePath))
            .SelectMany(entry => entry.Value)
            .ToList();
        return filteredTests?.Count > 0 ? filteredTests : null;
    }

    internal async Task ExecuteInternally(TestSuite testSuite, TestRunnerConfig runnerConfig)
    {
        try
        {
            if (!ReportOrphanNodesEnabled)
                Console.WriteLine("Warning!!! Reporting orphan nodes is disabled. Please check GdUnit settings.");
            await GodotObjectExtensions.SyncProcessFrame;
            using ExecutionContext context = new(testSuite, eventListeners, ReportOrphanNodesEnabled, true);
            context.IsCaptureStdOut = runnerConfig.CaptureStdOut;
            await new TestSuiteExecutionStage(testSuite).Execute(context);
        }
        // handle unexpected exceptions
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync($"Unexpected Exception: {e.Message} \nStackTrace: {e.StackTrace}");
        }
        finally
        {
            testSuite.Dispose();
        }
    }

    private class GdTestEventListenerDelegator : ITestEventListener
    {
        private readonly GodotObject listener;

        public GdTestEventListenerDelegator(GodotObject listener)
            => this.listener = listener;

        public bool IsFailed
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int CompletedTests { get; set; }

        public void PublishEvent(ITestEvent e)
        {
            var testEvent = (e as TestEvent)!;
            Godot.Collections.Dictionary<string, Variant> data = new()
            {
                { "type", e.Type.ToVariant() },
                { "resource_path", testEvent.ResourcePath.ToVariant() },
                { "suite_name", testEvent.SuiteName.ToVariant() },
                { "test_name", testEvent.TestName.ToVariant() },
                { "total_count", testEvent.TotalCount.ToVariant() },
                { "statistics", ToGdUnitEventStatistics(testEvent.Statistics) }
            };

            if (e.Reports.Count > 0)
            {
                var serializedReports = e.Reports.Select(report => report.Serialize()).ToGodotArray();
                data.Add("reports", serializedReports);
            }

            listener.Call("PublishEvent", data);
        }

        public void Dispose() => listener.Dispose();

        private Godot.Collections.Dictionary<Variant, Variant> ToGdUnitEventStatistics(IDictionary<TestEvent.STATISTIC_KEY, object> statistics)
        {
            var converted = new Godot.Collections.Dictionary<Variant, Variant>();
            foreach (var (key, value) in statistics)
                converted[key.ToString().ToLower().ToVariant()] = value.ToVariant();
            return converted;
        }
    }

    private class GdUnitRunnerConfig
    {
        [JsonProperty] public Dictionary<string, IEnumerable<string>> Included { get; private set; } = new();
    }
}
