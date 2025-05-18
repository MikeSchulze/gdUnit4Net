// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

using Api;

using Monitoring;

using Reporting;

internal sealed class ExecutionContext : IDisposable
{
    private int iteration;

    public ExecutionContext(TestSuite testInstance, IEnumerable<ITestEventListener> eventListeners, bool reportOrphanNodesEnabled, bool isEngineMode)
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
        IsEngineMode = isEngineMode;
    }

    public ExecutionContext(ExecutionContext context, params object?[] methodArguments)
        : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled,
        context.IsEngineMode)
    {
        ReportCollector = context.ReportCollector;
        context.SubExecutionContexts.Add(this);
        TestCaseName = context.TestCaseName;
        CurrentTestCase = context.CurrentTestCase;
        MethodArguments = methodArguments;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count == 1
            ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0
            : 0;
        FullyQualifiedName = TestCase.BuildFullyQualifiedName(TestSuite.Instance.GetType().FullName!, TestCaseName, new TestCaseAttribute(methodArguments));
    }

    // used for dynamic datapoint tests
    public ExecutionContext(ExecutionContext context, string displayName)
        : this(context.TestSuite, context.EventListeners,
        context.ReportOrphanNodesEnabled,
        context.IsEngineMode)
    {
        ReportCollector = context.ReportCollector;
        context.SubExecutionContexts.Add(this);
        CurrentTestCase = context.CurrentTestCase;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count == 1
            ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0
            : 0;
        TestCaseName = context.TestCaseName;
        FullyQualifiedName = TestCase.BuildFullyQualifiedName(TestSuite.Instance.GetType().FullName!, displayName, new TestCaseAttribute());
        DisplayName = displayName;
    }

    public ExecutionContext(ExecutionContext context, TestCase testCase)
        : this(context.TestSuite, context.EventListeners, context.ReportOrphanNodesEnabled, context.IsEngineMode)
    {
        context.SubExecutionContexts.Add(this);
        CurrentTestCase = testCase;
        CurrentIteration = CurrentTestCase?.TestCaseAttributes.Count == 1
            ? CurrentTestCase?.TestCaseAttributes.ElementAt(0).Iterations ?? 0
            : 0;
        IsSkipped = CurrentTestCase?.IsSkipped ?? false;
        TestCaseName = TestCase.BuildDisplayName(testCase.Name, testCase.TestCaseAttribute);
        FullyQualifiedName = TestCase.BuildFullyQualifiedName(TestSuite.Instance.GetType().FullName!, testCase.Name, testCase.TestCaseAttribute);
    }

    public bool IsEngineMode { get; set; }

    private TimeSpan ExecutionTimeout { get; } = TimeSpan.FromSeconds(30);

    public bool IsCaptureStdOut
    {
        get;
        set;
    }

    = true;

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
    }

    = string.Empty;

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

    private List<ITestReport> CollectReports => ReportCollector.Reports;

    private int SkippedCount => SubExecutionContexts.Count(context => context.IsSkipped);

    private int FailureCount => ReportCollector.Failures.Count();

    private int ErrorCount => ReportCollector.Errors.Count();

    private string FullyQualifiedName { get; }

    private string? DisplayName { get; }

    public void Dispose()
    {
        Disposables.ForEach(disposable =>
        {
            try
            {
                disposable.Dispose();
            }
            catch (ObjectDisposedException e)
            {
                _ = e;
            }
        });
        Stopwatch.Stop();
    }

    public bool IsExpectingToFailWithException(Exception? exception, MethodInfo? mi)
    {
        var attribute = mi?.GetCustomAttribute<ThrowsExceptionAttribute>();
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

    private IDictionary<TestEvent.StatisticKey, object> BuildStatistics(int orphanCount)
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
            .Before(TestSuite.ResourcePath, TestSuite.Name, TestSuite.TestCaseCount, BuildStatistics(0), CollectReports)
            .WithFullyQualifiedName(FullyQualifiedName));

    public void FireAfterEvent() =>
        FireTestEvent(TestEvent
            .After(TestSuite.ResourcePath, TestSuite.Name, BuildStatistics(OrphanCount(false)), CollectReports)
            .WithFullyQualifiedName(FullyQualifiedName));

    public void FireBeforeTestEvent() =>
        FireTestEvent(TestEvent
            .BeforeTest(CurrentTestCase!.Id, TestSuite.ResourcePath, TestSuite.Name, TestCaseName)
            .WithFullyQualifiedName(FullyQualifiedName)
            .WithDisplayName(DisplayName));

    public void FireAfterTestEvent() =>
        FireTestEvent(TestEvent
            .AfterTest(CurrentTestCase!.Id, TestSuite.ResourcePath, TestSuite.Name, TestCaseName, BuildStatistics(OrphanCount(true)), CollectReports)
            .WithFullyQualifiedName(FullyQualifiedName)
            .WithDisplayName(DisplayName));

    public static void RegisterDisposable(IDisposable disposable) =>
        Current?.Disposables.Add(disposable);

    public void PrintDebug(string name = "")
        => Console.WriteLine($"{name} test context {TestSuite.Name} {TestCaseName} error: {IsError} failed: {IsFailed} skipped: {IsSkipped}");

    public TimeSpan GetExecutionTimeout(TestCaseAttribute testAttribute) =>
        testAttribute.Timeout == -1 ? ExecutionTimeout : TimeSpan.FromMilliseconds(testAttribute.Timeout);
}
