namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

using Events;

using Monitoring;

using Reporting;

internal sealed class ExecutionContext : IDisposable
{
    // TODO handle engine mode
    public bool IsEngineMode = false;
    private int iteration;

    public ExecutionContext(TestSuite testInstance, IEnumerable<ITestEventListener> eventListeners, bool reportOrphanNodesEnabled, bool isEngineMode = false)
    {
        Thread.SetData(Thread.GetNamedDataSlot("ExecutionContext"), this);
        MemoryPool = new MemoryPool(reportOrphanNodesEnabled && isEngineMode);
        Stopwatch = new Stopwatch();
        Stopwatch.Start();

        ReportOrphanNodesEnabled = reportOrphanNodesEnabled;
        FailureReporting = true;
        TestSuite = testInstance;
        EventListeners = eventListeners;
        ReportCollector = new TestReportCollector();
        SubExecutionContexts = new List<ExecutionContext>();
        Disposables = new List<IDisposable>();
        FullyQualifiedName = TestSuite.Instance.GetType().FullName!;
    }

    public ExecutionContext(ExecutionContext context, params object?[] methodArguments) : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
    {
        ReportCollector = context.ReportCollector;
        context.SubExecutionContexts.Add(this);
        TestCaseName = context.TestCaseName;
        CurrentTestCase = context.CurrentTestCase;
        MethodArguments = methodArguments;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count() == 1
            ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0
            : 0;
    }

    public ExecutionContext(ExecutionContext context, TestCase testCase) : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
    {
        context.SubExecutionContexts.Add(this);
        TestCaseName = TestCase.BuildDisplayName(testCase.Name);
        FullyQualifiedName = TestCase.BuildFullyQualifiedName(TestSuite.Instance.GetType().FullName!, testCase.Name, null);
        CurrentTestCase = testCase;
        CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count() == 1
            ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0
            : 0;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
    }

    public ExecutionContext(ExecutionContext context, TestCase testCase, TestCaseAttribute testCaseAttribute)
        : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled)
    {
        context.SubExecutionContexts.Add(this);
        TestCaseName = TestCase.BuildDisplayName(testCase.Name, testCaseAttribute);
        FullyQualifiedName = TestCase.BuildFullyQualifiedName(TestSuite.Instance.GetType().FullName!, testCase.Name, testCaseAttribute);
        CurrentTestCase = testCase;
        CurrentIteration = 0;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
    }

    private TimeSpan ExecutionTimeout { get; } = TimeSpan.FromSeconds(30);

    public bool IsCaptureStdOut
    {
        get;
        set;
    } = true;

    private bool ReportOrphanNodesEnabled
    {
        get;
    }

    public bool FailureReporting
    {
        get;
        set;
    }


    public MemoryPool MemoryPool
    {
        get;
    }

    private Stopwatch Stopwatch
    {
        get;
    }

    public TestSuite TestSuite
    {
        get;
    }

    private List<IDisposable> Disposables
    {
        get;
    }

    public static ExecutionContext? Current => Thread.GetData(Thread.GetNamedDataSlot("ExecutionContext")) as ExecutionContext;

    private IEnumerable<ITestEventListener> EventListeners
    {
        get;
    }

    private List<ExecutionContext> SubExecutionContexts
    {
        get;
    }

    public TestCase? CurrentTestCase
    {
        get;
        set;
    }

    public string TestCaseName
    {
        get;
        set;
    } = "";

    public object?[] MethodArguments { get; private set; } = Array.Empty<object?>();

    private long Duration => Stopwatch.ElapsedMilliseconds;

    public int CurrentIteration
    {
        get => iteration--;
        set => iteration = value;
    }

    public TestReportCollector ReportCollector
    {
        get;
    }

    public bool IsFailed => ReportCollector.Failures.Any() || SubExecutionContexts.Any(context => context.IsFailed);

    public bool IsError => ReportCollector.Errors.Any() || SubExecutionContexts.Any(context => context.IsError);

    private bool IsWarning => ReportCollector.Warnings.Any() || SubExecutionContexts.Any(context => context.IsWarning);

    public bool IsSkipped
    {
        get;
    }

    private IEnumerable<TestReport> CollectReports => ReportCollector.Reports;

    private int SkippedCount => SubExecutionContexts.Count(context => context.IsSkipped);

    private int FailureCount => ReportCollector.Failures.Count();

    private int ErrorCount => ReportCollector.Errors.Count();

    private string FullyQualifiedName { get; }

    public void Dispose()
    {
        Disposables.ForEach(disposable =>
        {
            try
            {
                disposable.Dispose();
            }
            catch (ObjectDisposedException e) { _ = e; }
        });
        Stopwatch.Stop();
    }

    public bool IsExpectingToFailWithException(Exception? exception)
    {
        var attribute = CurrentTestCase?.MethodInfo.GetCustomAttribute<ThrowsExceptionAttribute>();
        if (attribute == null)
            return false;

        if (exception == null)
            attribute.ThrowExpectingExceptionExpected();

        return attribute.Verify(exception!);
    }

    private int OrphanCount(bool recursive)
    {
        var orphanCount = MemoryPool.OrphanCount;
        if (recursive)
            orphanCount += SubExecutionContexts.Select(context => context.MemoryPool.OrphanCount).Sum();
        return orphanCount;
    }

    private IDictionary<TestEvent.STATISTIC_KEY, object> BuildStatistics(int orphanCount)
        => TestEvent.BuildStatistics(
            orphanCount,
            IsError, ErrorCount,
            IsFailed, FailureCount,
            IsWarning,
            IsSkipped, SkippedCount,
            Duration);

    private void FireTestEvent(TestEvent e) =>
        EventListeners.ToList().ForEach(l => l.PublishEvent(e));

    public void FireBeforeEvent() =>
        FireTestEvent(TestEvent
            .Before(TestSuite.ResourcePath, TestSuite.Name, TestSuite.TestCaseCount)
            .WithFullyQualifiedName(FullyQualifiedName));

    public void FireAfterEvent() =>
        FireTestEvent(TestEvent
            .After(TestSuite.ResourcePath, TestSuite.Name, BuildStatistics(OrphanCount(false)), CollectReports)
            .WithFullyQualifiedName(FullyQualifiedName));

    public void FireBeforeTestEvent() =>
        FireTestEvent(TestEvent
            .BeforeTest(TestSuite.ResourcePath, TestSuite.Name, TestCaseName)
            .WithFullyQualifiedName(FullyQualifiedName));

    public void FireAfterTestEvent() =>
        FireTestEvent(TestEvent
            .AfterTest(TestSuite.ResourcePath, TestSuite.Name, TestCaseName, BuildStatistics(OrphanCount(true)), CollectReports)
            .WithFullyQualifiedName(FullyQualifiedName));


    public TestEvent TearDownTestEvent(Guid id) =>
        TestEvent.TearDownTest(id, BuildStatistics(OrphanCount(true)), CollectReports.ToList());


    public static void RegisterDisposable(IDisposable disposable) =>
        Current?.Disposables.Add(disposable);

    public void PrintDebug(string name = "")
        => Console.WriteLine($"{name} test context {TestSuite.Name} {TestCaseName} error: {IsError} failed: {IsFailed} skipped: {IsSkipped}");

    public TimeSpan GetExecutionTimeout(TestCaseAttribute testAttribute) =>
        testAttribute.Timeout == -1 ? ExecutionTimeout : TimeSpan.FromMilliseconds(testAttribute.Timeout);
}
