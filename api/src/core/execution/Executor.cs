using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace GdUnit4.Executions
{
    public sealed partial class Executor : Godot.RefCounted, IExecutor
    {
        [Godot.Signal]
        public delegate void ExecutionCompletedEventHandler();

        private List<ITestEventListener> _eventListeners = new();

        private class GdTestEventListenerDelegator : ITestEventListener
        {
            private readonly Godot.GodotObject _listener;

            public bool IsFailed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public GdTestEventListenerDelegator(Godot.GodotObject listener)
            {
                _listener = listener;
            }

            public void PublishEvent(TestEvent testEvent)
            {
                Godot.Collections.Dictionary<string, Godot.Variant> data = new()
                {
                    { "type", testEvent.Type.ToVariant() },
                    { "resource_path", testEvent.ResourcePath.ToVariant()  },
                    { "suite_name", testEvent.SuiteName.ToVariant()  },
                    { "test_name", testEvent.TestName.ToVariant()  },
                    { "total_count", testEvent.TotalCount.ToVariant()  },
                    { "statistics", toGdUnitEventStatisitics(testEvent.Statistics) }
                };

                if (testEvent.Reports.Count() != 0)
                {
                    var serializedReports = testEvent.Reports.Select(report => report.Serialize()).ToGodotArray();
                    data.Add("reports", serializedReports);
                }
                _listener.Call("PublishEvent", data);
            }

            private Godot.Collections.Dictionary<Godot.Variant, Godot.Variant> toGdUnitEventStatisitics(IDictionary<TestEvent.STATISTIC_KEY, object> statistics)
            {
                var converted = new Godot.Collections.Dictionary<Godot.Variant, Godot.Variant>();
                foreach (var (key, value) in statistics)
                    converted[key.ToString().ToLower().ToVariant()] = value.ToVariant();
                return converted;
            }
        }

        public IExecutor AddGdTestEventListener(Godot.GodotObject listener)
        {
            // I want to using anonymus implementation to remove the extra delegator class
            _eventListeners.Add(new GdTestEventListenerDelegator(listener));
            return this;
        }

        public void AddTestEventListener(ITestEventListener listener)
        {
            _eventListeners.Add(listener);
        }

        public bool ReportOrphanNodesEnabled { get; set; } = true;

        public bool IsExecutable(Godot.Node node) => node is CsNode;


        /// <summary>
        /// Execute a testsuite, is called externally from Godot test suite runner
        /// </summary>
        /// <param name="testSuite"></param>
        public void Execute(CsNode testSuite)
        {
            try
            {
                var includedTests = testSuite.GetChildren()
                    .Cast<CsNode>()
                    .ToList()
                    .Select(node => node.Name.ToString())
                    .ToList();
                var task = ExecuteInternally(new TestSuite(testSuite.ResourcePath(), includedTests));
                // use this call as workaround to hold the signal list, it is disposed for some unknown reason.
                // could be related to https://github.com/godotengine/godot/issues/84254
                GetSignalConnectionList("ExecutionCompleted");
                task.GetAwaiter().OnCompleted(() => EmitSignal(SignalName.ExecutionCompleted));
            }
            // handle unexpected exceptions
            catch (Exception e)
            {
                Console.Error.WriteLine("Unexpected Exception: %s \nStackTrace: %s", e.Message, e.StackTrace);
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
                using ExecutionContext context = new(testSuite, _eventListeners, ReportOrphanNodesEnabled);
                await new TestSuiteExecutionStage(testSuite).Execute(context);
            }
            // handle unexpected exceptions
            catch (Exception e)
            {
                Console.Error.WriteLine("Unexpected Exception: %s \nStackTrace: %s", e.Message, e.StackTrace);
            }
            finally
            {
                testSuite.Dispose();
            }
        }
    }
}
