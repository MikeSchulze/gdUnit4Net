namespace GdUnit4.Executions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

using GdUnit4.Core;

public sealed partial class Executor : Godot.RefCounted, IExecutor
{
    [Godot.Signal]
    public delegate void ExecutionCompletedEventHandler();

    private readonly List<ITestEventListener> eventListeners = new();

    private class GdTestEventListenerDelegator : ITestEventListener
    {
        private readonly Godot.GodotObject listener;

        public bool IsFailed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public GdTestEventListenerDelegator(Godot.GodotObject listener)
            => this.listener = listener;

        public void PublishEvent(TestEvent testEvent)
        {
            Godot.Collections.Dictionary<string, Godot.Variant> data = new()
            {
                { "type", testEvent.Type.ToVariant() },
                { "resource_path", testEvent.ResourcePath.ToVariant()  },
                { "suite_name", testEvent.SuiteName.ToVariant()  },
                { "test_name", testEvent.TestName.ToVariant()  },
                { "total_count", testEvent.TotalCount.ToVariant()  },
                { "statistics", ToGdUnitEventStatistics(testEvent.Statistics) }
            };

            if (testEvent.Reports.Any())
            {
                var serializedReports = testEvent.Reports.Select(report => report.Serialize()).ToGodotArray();
                data.Add("reports", serializedReports);
            }
            listener.Call("PublishEvent", data);
        }

        private Godot.Collections.Dictionary<Godot.Variant, Godot.Variant> ToGdUnitEventStatistics(IDictionary<TestEvent.STATISTIC_KEY, object> statistics)
        {
            var converted = new Godot.Collections.Dictionary<Godot.Variant, Godot.Variant>();
            foreach (var (key, value) in statistics)
                converted[key.ToString().ToLower().ToVariant()] = value.ToVariant();
            return converted;
        }
    }

    public IExecutor AddGdTestEventListener(Godot.GodotObject listener)
    {
        // I want to using anonyms implementation to remove the extra delegator class
        eventListeners.Add(new GdTestEventListenerDelegator(listener));
        return this;
    }

    internal void AddTestEventListener(ITestEventListener listener)
        => eventListeners.Add(listener);

    private bool ReportOrphanNodesEnabled => GdUnit4Settings.IsVerboseOrphans();

    public bool IsExecutable(Godot.Node node) => node is CsNode;

    private class GdUnitRunnerConfig
    {
        public Dictionary<string, IEnumerable<string>> Included { get; set; } = new();
    }

    /// <summary>
    /// Loads the list of included tests from GdUnitRunner.cfg if exists
    /// </summary>
    /// <param name="testSuite"></param>
    /// <returns></returns>
    private IEnumerable<string>? LoadTestFilter(CsNode testSuite)
    {
        // try to load runner config written by gdunit4 plugin
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "addons/gdUnit4/GdUnitRunner.cfg");
        if (!File.Exists(configPath))
            return null;

        var testSuitePath = testSuite.ResourcePath();
        var json = File.ReadAllText(configPath);
        var runnerConfig = JsonConvert.DeserializeObject<GdUnitRunnerConfig>(json);
        // Filter by testSuitePath and add values from runnerConfig.Included to the list
        var filteredTests = runnerConfig?.Included
            .Where(entry => entry.Key.EndsWith(testSuitePath))
            .SelectMany(entry => entry.Value);
        return filteredTests?.Any() == true ? filteredTests : null;
    }

    /// <summary>
    /// Execute a testsuite, is called externally from Godot test suite runner
    /// </summary>
    /// <param name="testSuite"></param>
    public void Execute(CsNode testSuite)
    {
        try
        {
            var includedTests = LoadTestFilter(testSuite);
            var task = ExecuteInternally(new TestSuite(testSuite.ResourcePath(), includedTests, true, true));
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

    internal async Task ExecuteInternally(TestSuite testSuite)
    {
        try
        {
            if (!ReportOrphanNodesEnabled)
                Console.WriteLine("Warning!!! Reporting orphan nodes is disabled. Please check GdUnit settings.");
            await ISceneRunner.SyncProcessFrame;
            using ExecutionContext context = new(testSuite, eventListeners, ReportOrphanNodesEnabled);
            await new TestSuiteExecutionStage(testSuite).Execute(context);
        }
        // handle unexpected exceptions
        catch (Exception e)
        {
            Console.Error.WriteLine($"Unexpected Exception: {e.Message} \nStackTrace: {e.StackTrace}");
        }
        finally
        {
            testSuite.Dispose();
        }
    }
}
