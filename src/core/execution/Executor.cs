using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace GdUnit3.Executions
{
    public sealed class Executor : Godot.Reference, IExecutor
    {
        [Godot.Signal]
        public delegate void ExecutionCompleted();

        private List<ITestEventListener> _eventListeners = new List<ITestEventListener>();

        private class GdTestEventListenerDelegator : ITestEventListener
        {
            private readonly Godot.Object _listener;

            public GdTestEventListenerDelegator(Godot.Object listener)
            {
                _listener = listener;
            }
            public void PublishEvent(TestEvent testEvent) => _listener.Call("PublishEvent", testEvent);
        }

        public IExecutor AddGdTestEventListener(Godot.Object listener)
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

        /// <summary>
        /// Execute a testsuite, is called externally from Godot test suite runner
        /// </summary>
        /// <param name="testSuite"></param>
        public async void Execute(CsNode testSuite)
        {
            try
            {
                var includedTests = testSuite.GetChildren()
                    .Cast<CsNode>()
                    .ToList()
                    .Select(node => node.Name)
                    .ToList();
                await ExecuteInternally(new TestSuite(testSuite.ResourcePath(), includedTests));
            }
            catch (Exception e)
            {
                Godot.GD.PushError(e.Message);
            }
            finally
            {
                testSuite.Free();
            }
        }

        internal async Task ExecuteInternally(TestSuite testSuite)
        {
            if (!ReportOrphanNodesEnabled)
                Godot.GD.PushWarning("!!! Reporting orphan nodes is disabled. Please check GdUnit settings.");
            try
            {

                using (ExecutionContext context = new ExecutionContext(testSuite, _eventListeners, ReportOrphanNodesEnabled))
                {
                    var task = new TestSuiteExecutionStage(testSuite).Execute(context);
                    task.GetAwaiter().OnCompleted(() => EmitSignal(nameof(ExecutionCompleted)));
                    await task;
                }
            }
            catch (Exception e)
            {
                // unexpected exceptions
                Godot.GD.PushError(e.Message);
                Godot.GD.PushError(e.StackTrace);
            }
            finally
            {
                testSuite.Dispose();
            }
        }
    }
}
