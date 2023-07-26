using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using GdUnit4.Executions.Monitors;

namespace GdUnit4.Executions
{
    internal sealed class ExecutionContext : IDisposable
    {
        public ExecutionContext(TestSuite testInstance, IEnumerable<ITestEventListener> eventListeners, bool reportOrphanNodesEnabled)
        {
            Thread.SetData(Thread.GetNamedDataSlot("ExecutionContext"), this);
            MemoryPool = new MemoryPool();
            OrphanMonitor = new OrphanNodesMonitor(reportOrphanNodesEnabled);
            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            ReportOrphanNodesEnabled = reportOrphanNodesEnabled;
            FailureReporting = true;
            TestSuite = testInstance;
            EventListeners = eventListeners;
            ReportCollector = new TestReportCollector();
            SubExecutionContexts = new List<ExecutionContext>();
            Disposables = new List<IDisposable>();
        }

        public ExecutionContext(ExecutionContext context, params object[] methodArguments) : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
        {
            ReportCollector = context.ReportCollector;
            context.SubExecutionContexts.Add(this);
            TestCaseName = context.TestCaseName;
            CurrentTestCase = context.CurrentTestCase;
            MethodArguments = methodArguments;
            IsSkipped = CurrentTestCase?.IsSkipped ?? false;
            CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count() == 1 ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0 : 0;
        }

        public ExecutionContext(ExecutionContext context, TestCase testCase) : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
        {
            context.SubExecutionContexts.Add(this);
            TestCaseName = testCase.Name;
            CurrentTestCase = testCase;
            CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count() == 1 ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0 : 0;
            IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        }

        public ExecutionContext(ExecutionContext context, TestCase testCase, TestCaseAttribute testCaseAttribute) : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
        {
            context.SubExecutionContexts.Add(this);
            TestCaseName = BuildTestCaseName(testCase.Name, testCaseAttribute);
            CurrentTestCase = testCase;
            CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count() == 1 ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0 : 0;
            IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        }

        public bool ReportOrphanNodesEnabled
        { get; private set; }

        public bool FailureReporting
        { get; set; }

        public OrphanNodesMonitor OrphanMonitor
        { get; set; }

        public MemoryPool MemoryPool
        { get; set; }

        public Stopwatch Stopwatch
        { get; private set; }

        public TestSuite TestSuite
        { get; private set; }

        private List<IDisposable> Disposables
        { get; set; }

        public static ExecutionContext? Current => Thread.GetData(Thread.GetNamedDataSlot("ExecutionContext")) as ExecutionContext;

        private IEnumerable<ITestEventListener> EventListeners
        { get; set; }

        private List<ExecutionContext> SubExecutionContexts
        { get; set; }

        public TestCase? CurrentTestCase
        { get; set; }

        public string TestCaseName
        { get; set; } = "";

        public object[] MethodArguments { get; private set; } = { };

        private long Duration => Stopwatch.ElapsedMilliseconds;

        private int _iteration;
        public int CurrentIteration
        {
            get => _iteration--;
            set => _iteration = value;
        }

        public TestReportCollector ReportCollector
        { get; private set; }

        public bool IsFailed => ReportCollector.Failures.Any() || SubExecutionContexts.Any(context => context.IsFailed);

        public bool IsError => ReportCollector.Errors.Any() || SubExecutionContexts.Any(context => context.IsError);

        public bool IsWarning => ReportCollector.Warnings.Any() || SubExecutionContexts.Any(context => context.IsWarning);

        public bool IsSkipped
        { get; private set; }

        public IEnumerable<TestReport> CollectReports => ReportCollector.Reports;

        private int SkippedCount => SubExecutionContexts.Count(context => context.IsSkipped);

        private int FailureCount => ReportCollector.Failures.Count();

        private int ErrorCount => ReportCollector.Errors.Count();

        public int OrphanCount(bool recursive)
        {
            var orphanCount = OrphanMonitor.OrphanCount;
            if (recursive)
                orphanCount += SubExecutionContexts.Select(context => context.OrphanMonitor.OrphanCount).Sum();
            return orphanCount;
        }

        public IDictionary BuildStatistics(int orphanCount)
        {
            return TestEvent.BuildStatistics(
                orphanCount,
                IsError, ErrorCount,
                IsFailed, FailureCount,
                IsWarning,
                IsSkipped, SkippedCount,
                Duration);
        }

        public void FireTestEvent(TestEvent e) =>
            EventListeners.ToList().ForEach(l => l.PublishEvent(e));

        public void FireBeforeEvent() =>
            FireTestEvent(TestEvent.Before(TestSuite.ResourcePath, TestSuite.Name, TestSuite.TestCaseCount));

        public void FireAfterEvent() =>
            FireTestEvent(TestEvent.After(TestSuite.ResourcePath, TestSuite.Name, BuildStatistics(OrphanCount(false)), CollectReports));

        public void FireBeforeTestEvent() =>
            FireTestEvent(TestEvent.BeforeTest(TestSuite.ResourcePath, TestSuite.Name, TestCaseName));

        public void FireAfterTestEvent() =>
            FireTestEvent(TestEvent.AfterTest(TestSuite.ResourcePath, TestSuite.Name, TestCaseName, BuildStatistics(OrphanCount(true)), CollectReports));


        public static void RegisterDisposable(IDisposable disposable) =>
            ExecutionContext.Current?.Disposables.Add(disposable);

        public void Dispose()
        {
            Disposables.ForEach(disposable =>
            {
                try { disposable.Dispose(); }
                catch (ObjectDisposedException e) { _ = e; }
            });
            Stopwatch.Stop();
        }

        private static string BuildTestCaseName(string testName, TestCaseAttribute attribute)
        {
            if (attribute.Arguments.Count() > 1)
                return attribute.TestName != null ? attribute.TestName : $"{testName} [{attribute.Arguments.Formated()}]";
            return testName;
        }

        public void PrintDebug(string name = "")
        {
            Godot.GD.PrintS(name, "test context", TestSuite.Name, TestCaseName, "error:" + IsError, "failed:" + IsFailed, "skipped:" + IsSkipped);
        }
    }

}
