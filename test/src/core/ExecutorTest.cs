namespace GdUnit4.Tests.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using Api;

using GdUnit4.Asserts;
using GdUnit4.Core;
using GdUnit4.Core.Commands;
using GdUnit4.Core.Discovery;
using GdUnit4.Core.Execution;
using GdUnit4.Core.Extensions;
using GdUnit4.Core.Reporting;
using GdUnit4.Core.Runners;

using Godot;

using Resources;

using static Assertions;

using static Api.ITestEvent.EventType;
using static Api.ITestReport.ReportType;

using TestCase = GdUnit4.Core.Execution.TestCase;

[RequireGodotRuntime]
[TestSuite]
public class ExecutorTest : ITestEventListener, IDisposable
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
#pragma warning disable CS0649
    // enable to verbose debug event
    private readonly bool verbose;
#pragma warning restore CS0649
    private Executor? executor;
    private List<ITestEvent> CollectedEvents { get; } = new();
    private static CodeNavigationDataProvider? NavigationDataProvider { get; set; }

    public void Dispose()
    {
        NavigationDataProvider?.Dispose();
        executor?.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool IsFailed { get; set; }
    public int CompletedTests { get; set; }

    void ITestEventListener.PublishEvent(ITestEvent testEvent)
    {
        if (verbose)
        {
            var e = (testEvent as TestEvent)!;
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"Event Type: {testEvent.Type}, SuiteName: {e.SuiteName}, TestName: {e.TestName}, Statistics: {e.Statistics}");
            Console.WriteLine($"ErrorCount: {e.ErrorCount}, FailedCount: {e.FailedCount}, OrphanCount: {e.OrphanCount}");
            var reports = new List<ITestReport>(e.Reports).ConvertAll(r => new TestReport(r.Type, r.LineNumber, r.Message.RichTextNormalize()));
            if (verbose)
                reports.ForEach(r => Console.WriteLine($"Reports -> {r}"));
        }

        CollectedEvents.Add(testEvent);
    }

    [Before]
    public void ClassSetup()
    {
        var assembly = typeof(ExecutorTest).Assembly;
        var location = assembly.ManifestModule.FullyQualifiedName;

        // Fallback approach if needed
        if (location == "<Unknown>" || !File.Exists(location))
        {
            // Try to find the DLL in the execution directory
            var assemblyName = assembly.GetName().Name;
            var executionPath = AppDomain.CurrentDomain.BaseDirectory;
            location = Path.Combine(executionPath, $"{assemblyName}.dll");
        }

        NavigationDataProvider = new CodeNavigationDataProvider(location);
        executor = new Executor();
        executor.AddTestEventListener(this);
    }

    [BeforeTest]
    public void InitTest()
        => ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, true);

    [AfterTest]
    public void TeardownTest()
        => ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, true);

    private async Task<List<ITestEvent>> ExecuteTestSuite<T>(bool reportOrphans = true)
    {
        var type = typeof(T);
        var testSuiteName = type.Name;
        CollectedEvents.Clear();

        if (verbose)
            Console.WriteLine($"Execute {testSuiteName}.");
        var testSuiteNode = LoadTestSuiteNode(type);

        CollectedEvents.Clear();
        var response = await new ExecuteTestSuiteCommand(testSuiteNode, false, reportOrphans).Execute(this);
        AssertThat(response.StatusCode).IsEqual(HttpStatusCode.OK);

        if (verbose)
            Console.WriteLine($"Execution {testSuiteName} done.");
        return CollectedEvents;
    }

    private List<ITuple> ExpectedEvents(string suiteName, params string[] testCaseNames)
    {
        var expectedEvents = new List<ITuple> { Tuple(SuiteBefore, suiteName, "Before", testCaseNames.Length) };
        foreach (var testCase in testCaseNames)
        {
            expectedEvents.Add(Tuple(TestBefore, suiteName, testCase, 0));
            expectedEvents.Add(Tuple(TestAfter, suiteName, testCase, 0));
        }

        expectedEvents.Add(Tuple(SuiteAfter, suiteName, "After", 0));
        return expectedEvents;
    }

    private IEnumerableAssert<object?> AssertTestCaseNames(List<ITestEvent> events) =>
        AssertArray(events).ExtractV(Extr("Type"), Extr("SuiteName"), Extr("TestName"), Extr("TotalCount"));

    private IEnumerableAssert<object?> AssertEventCounters(List<ITestEvent> events) =>
        AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("ErrorCount"), Extr("FailedCount"), Extr("OrphanCount"));

    private IEnumerableAssert<object?> AssertEventStates(List<ITestEvent> events) =>
        AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("IsSuccess"), Extr("IsWarning"), Extr("IsFailed"), Extr("IsError"));

    private IEnumerableAssert<object?> AssertReports(List<ITestEvent> events)
    {
        var extractedEvents = events.ConvertAll(@event =>
        {
            var e = (@event as TestEvent)!;
            var reports = new List<ITestReport>(e.Reports)
                // we exclude standard out reports
                .FindAll(r => r.Type != Stdout)
                .ConvertAll(r => new TestReport(r.Type, r.LineNumber, r.Message.RichTextNormalize()));
            return new
            {
                e.TestName,
                e.Type,
                Reports = reports
            };
        });
        return AssertArray(extractedEvents).ExtractV(Extr("Type"), Extr("TestName"), Extr("Reports"));
    }

    private static List<ITuple> ExpectedTestCase(string suiteName, string testName, List<object[]> testCaseParams)
    {
        //var expectedEvents = new List<ITuple> { Tuple(TESTCASE_BEFORE, suiteName, testName, 0) };
        var expectedEvents = new List<ITuple>();
        foreach (var testCaseParam in testCaseParams)
        {
            var testCaseName = TestCase.BuildDisplayName(testName, new TestCaseAttribute(testCaseParam));
            expectedEvents.Add(Tuple(TestBefore, suiteName, testCaseName, 0));
            expectedEvents.Add(Tuple(TestAfter, suiteName, testCaseName, 0));
        }

        // expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testName, 0));
        return expectedEvents;
    }

    private static TestSuiteNode LoadTestSuiteNode(Type testSuiteType)
    {
        ITestEngineLogger logger = new GodotLogger();
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var tests = TestCaseDiscoverer.DiscoverTests(logger, NavigationDataProvider!, assemblyLocation, testSuiteType).ToList();
        var first = tests.First();

        var assemblyNode = new TestAssemblyNode
        {
            Id = Guid.NewGuid(),
            ParentId = Guid.Empty,
            AssemblyPath = first.AssemblyPath,
            Suites = new List<TestSuiteNode>()
        };

        var testSuiteNode = new TestSuiteNode
        {
            Id = Guid.NewGuid(),
            ParentId = assemblyNode.Id,
            ManagedType = first.ManagedType,
            AssemblyPath = assemblyNode.AssemblyPath,
            Tests = new List<TestCaseNode>()
        };

        var testCaseNodes = tests
            .Select(descriptor => new TestCaseNode
            {
                Id = descriptor.Id,
                ParentId = testSuiteNode.Id,
                ManagedMethod = descriptor.ManagedMethod,
                AttributeIndex = descriptor.AttributeIndex,
                LineNumber = descriptor.LineNumber,
                RequireRunningGodotEngine = descriptor.RequireRunningGodotEngine
            });
        testSuiteNode.Tests.AddRange(testCaseNodes);
        assemblyNode.Suites.Add(testSuiteNode);
        return testSuiteNode;
    }

    [TestCase(Description = "Verifies the complete test suite ends with success and no failures are reported.")]
    public async Task ExecuteSuccess()
    {
        var events = await ExecuteTestSuite<TestSuiteAllStagesSuccess>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAllStagesSuccess", "TestCase1", "TestCase2"));

        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 0, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 0, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", true, false, false, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", true, false, false, false),
            Tuple(SuiteAfter, "After", true, false, false, false)
        );

        // all success no reports expected
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>()),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport>()),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies report a failure on stage 'Before'.")]
    public async Task ExecuteFailureOnStageBefore()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnStageBefore>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageBefore", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'Before()' and commits one failure
        // where is reported finally at TESTSUITE_AFTER event
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 0, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 0, 0),
            // report failure failed_count = 1
            Tuple(SuiteAfter, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", true, false, false, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // one failure at Before()
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>()),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport>()),
            Tuple(SuiteAfter, "After", new List<TestReport> { new(Failure, 12, "failed on Before()") }));
    }

    [TestCase(Description = "Verifies report a failure on stage 'After'.")]
    public async Task ExecuteFailureOnStageAfter()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnStageAfter>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageAfter", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'After()' and commits one failure
        // where is reported finally at TESTSUITE_AFTER event
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 0, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 0, 0),
            // report failure failed_count = 1
            Tuple(SuiteAfter, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", true, false, false, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // one failure at After()
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>()),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport>()),
            Tuple(SuiteAfter, "After", new List<TestReport> { new(Failure, 16, "failed on After()") }));
    }

    [TestCase(Description = "Verifies report a failure on stage 'BeforeTest'.")]
    public async Task ExecuteFailureOnStageBeforeTest()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnStageBeforeTest>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageBeforeTest", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'BeforeTest()' and commits one failure on each test case
        // because is in scope of test execution
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 1, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 1, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", false, false, true, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // BeforeTest() failure report is append to each test
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport> { new(Failure, 20, "failed on BeforeTest()") }),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport> { new(Failure, 20, "failed on BeforeTest()") }),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies report a failure on stage 'AfterTest'.")]
    public async Task ExecuteFailureOnStageAfterTest()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnStageAfterTest>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageAfterTest", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'AfterTest()' and commits one failure on each test case
        // because is in scope of test execution
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 1, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 1, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", false, false, true, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // AfterTest() failure report is append to each test
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport> { new(Failure, 24, "failed on AfterTest()") }),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport> { new(Failure, 24, "failed on AfterTest()") }),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies a failure is reports for a single test case.")]
    public async Task ExecuteFailureOnTestCase1()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnTestCase1>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnTestCase1", "TestCase1", "TestCase2"));

        // we expect the test case 'TestCase1' is failing  and commits one failure
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 1, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 0, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", false, false, true, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // only 'TestCase1' reports a failure
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>
            {
                new(Failure, 27, """
                                 Expecting be equal:
                                     "TestCase1"
                                  but is
                                     "invalid"
                                 """)
            }),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport>()),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies multiple failures are report's for different stages.")]
    public async Task ExecuteFailureOnMultiStages()
    {
        var events = await ExecuteTestSuite<TestSuiteFailOnMultiStages>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnMultiStages", "TestCase1", "TestCase2"));

        // we expect failing on multiple stages
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            // TestCase1 has a failure plus one from 'BeforeTest'
            Tuple(TestAfter, "TestCase1", 0, 2, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            // the second test has no failures but one from 'BeforeTest'
            Tuple(TestAfter, "TestCase2", 0, 1, 0),
            // and one failure is on stage 'After' found
            Tuple(SuiteAfter, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", false, false, true, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // only 'TestCase1' reports a 'real' failure plus test setup stage failures
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>
            {
                new(Failure, 20, "failed on BeforeTest()"),
                new(Failure, 28, """
                                 Expecting be empty:
                                  but is
                                     "TestCase1"
                                 """)
            }),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport> { new(Failure, 20, "failed on BeforeTest()") }),
            Tuple(SuiteAfter, "After", new List<TestReport> { new(Failure, 16, "failed on After()") }));
    }

    [TestCase(Description = "GD-63: Execution must detect orphan nodes in the different test stages.")]
    public async Task ExecuteFailureOrphanNodesDetected()
    {
        var events = await ExecuteTestSuite<TestSuiteFailAndOrphansDetected>();
        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailAndOrphansDetected", "TestCase1", "TestCase2"));

        // we expect orphans detected on multiple stages
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            // TestCase1 ends with a warning and in sum 5 orphans detected
            // 2 from stage 'BeforeTest' + 3 from test itself
            Tuple(TestAfter, "TestCase1", 0, 0, 5),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            // TestCase2 ends with a one failure and in sum 6 orphans detected
            // 2 from stage 'BeforeTest' + 4 from test itself
            Tuple(TestAfter, "TestCase2", 0, 1, 6),
            // and one orphan detected from stage 'Before'
            Tuple(SuiteAfter, "After", 0, 0, 1)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            // test case has only warnings
            Tuple(TestAfter, "TestCase1", false, true, false, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            //  test case has a failure and warnings
            Tuple(TestAfter, "TestCase2", false, true, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, true, true, false)
        );
        // only 'TestCase2' reports a 'real' failure plus test setup stage failures
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            // ends with warnings
            Tuple(TestAfter, "TestCase1", new List<TestReport>
                {
                    new(Warning, 0, """
                                    WARNING:
                                        Detected <2> orphan nodes during test setup stage!
                                        Check SetupTest:38 and TearDownTest:46 for unfreed instances!
                                    """),
                    new(Warning, 52, """
                                     WARNING:
                                         Detected <3> orphan nodes during test execution!
                                     """)
                }
            ),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            // ends with failure and warnings
            Tuple(TestAfter, "TestCase2", new List<TestReport>
                {
                    new(Warning, 0, """
                                    WARNING:
                                        Detected <2> orphan nodes during test setup stage!
                                        Check SetupTest:38 and TearDownTest:46 for unfreed instances!
                                    """),
                    new(Warning, 61, """
                                     WARNING:
                                         Detected <4> orphan nodes during test execution!
                                     """),
                    new(Failure, 66, """
                                     Expecting be empty:
                                      but is
                                         "TestCase2"
                                     """)
                }
            ),
            // and one orphan detected at stage 'After'
            Tuple(SuiteAfter, "After", new List<TestReport>
            {
                new(Warning, 0, """
                                WARNING:
                                    Detected <1> orphan nodes during test suite setup stage!
                                    Check SetupSuite:27 and TearDownSuite:34 for unfreed instances!
                                """)
            })
        );
    }

    [TestCase(Description = "GD-62: Execution must ignore detect orphan nodes if is disabled.")]
    public async Task ExecuteFailureOrphanNodesDetectionDisabled()
    {
        // simulate test suite execution with disabled orphan detection
        ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, false);
        var events = await ExecuteTestSuite<TestSuiteFailAndOrphansDetected>(false);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailAndOrphansDetected", "TestCase1", "TestCase2"));

        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 0, 0, 0),
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 1, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", true, false, false, false),
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            //  test case has a failure
            Tuple(TestAfter, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // only 'TestCase2' reports a failure, orphans are not reported
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport>()),
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            // ends with failure
            Tuple(TestAfter, "TestCase2", new List<TestReport>
            {
                new(Failure, 66, """
                                 Expecting be empty:
                                  but is
                                     "TestCase2"
                                 """)
            }),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "GD-66: The execution must be aborted by a test timeout.")]
    public async Task ExecuteAbortOnTimeOut()
    {
        var events = await ExecuteTestSuite<TestSuiteAbortOnTestTimeout>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAbortOnTestTimeout", "TestCase1", "TestCase2", "TestCase3", "TestCase4", "TestCase5"));

        // "ErrorCount", "FailedCount", "OrphanCount"
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),

            // expect test error by a timeout
            Tuple(TestBefore, "TestCase1", 0, 0, 0),
            Tuple(TestAfter, "TestCase1", 1, 0, 0),

            // expect test failed by reported failure
            Tuple(TestBefore, "TestCase2", 0, 0, 0),
            Tuple(TestAfter, "TestCase2", 0, 1, 0),

            // expect test succeeded
            Tuple(TestBefore, "TestCase3", 0, 0, 0),
            Tuple(TestAfter, "TestCase3", 0, 0, 0),

            // expect to fail, invalid method signature
            Tuple(TestBefore, "TestCase4", 0, 0, 0),
            Tuple(TestAfter, "TestCase4", 0, 1, 0),

            // expect test succeeded
            Tuple(TestBefore, "TestCase5", 0, 0, 0),
            Tuple(TestAfter, "TestCase5", 0, 0, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );

        // IsSuccess", "IsWarning", "IsFailed", "IsError"
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            //  test case is marked as error because of timeout
            Tuple(TestBefore, "TestCase1", true, false, false, false),
            Tuple(TestAfter, "TestCase1", false, false, false, true),

            //  test case is marked as failure
            Tuple(TestBefore, "TestCase2", true, false, false, false),
            Tuple(TestAfter, "TestCase2", false, false, true, false),

            //  test case is succeeded
            Tuple(TestBefore, "TestCase3", true, false, false, false),
            Tuple(TestAfter, "TestCase3", true, false, false, false),

            //  test fails by invalid method signature
            Tuple(TestBefore, "TestCase4", true, false, false, false),
            Tuple(TestAfter, "TestCase4", false, false, true, false),

            //  test case is succeeded
            Tuple(TestBefore, "TestCase5", true, false, false, false),
            Tuple(TestAfter, "TestCase5", true, false, false, false),

            // report suite is not success, is failed and has a error
            Tuple(SuiteAfter, "After", false, false, true, true)
        );
        AssertReports(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            // reports a test interruption due to a timeout
            Tuple(TestBefore, "TestCase1", new List<TestReport>()),
            Tuple(TestAfter, "TestCase1", new List<TestReport> { new(Interrupted, 32, "The execution has timed out after 1s.") }
            ),

            // reports a test failure
            Tuple(TestBefore, "TestCase2", new List<TestReport>()),
            Tuple(TestAfter, "TestCase2", new List<TestReport>
                {
                    new(Failure, 43, """
                                     Expecting be equal:
                                         'False' but is 'True'
                                     """)
                }
            ),

            // succeeds with no reports
            Tuple(TestBefore, "TestCase3", new List<TestReport>()),
            Tuple(TestAfter, "TestCase3", new List<TestReport>()),

            // reports a method signature failure
            Tuple(TestBefore, "TestCase4", new List<TestReport>()),
            Tuple(TestAfter, "TestCase4", new List<TestReport>
                {
                    new(Failure, 56, """
                                     Invalid method signature found at: TestCase4.
                                      You must return a <Task> for an asynchronously specified method.
                                     """)
                }
            ),

            // succeeds with no reports
            Tuple(TestBefore, "TestCase5", new List<TestReport>()),
            Tuple(TestAfter, "TestCase5", new List<TestReport>()),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Tests is all parameterized tests case executed.")]
    public async Task ExecuteParameterizedTest()
    {
        var events = await ExecuteTestSuite<TestSuiteParameterizedTests>();

        var suiteName = "TestSuiteParameterizedTests";
        var expectedEvents = new List<ITuple> { Tuple(SuiteBefore, suiteName, "Before", 9) };
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedBoolValue", new List<object[]>
        {
            new object[] { 0, false },
            new object[] { 1, true }
        }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValues", new List<object[]>
        {
            new object[] { 1, 2, 3, 6 },
            new object[] { 3, 4, 5, 12 },
            new object[] { 6, 7, 8, 21 }
        }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValuesFail", new List<object[]>
        {
            new object[] { 1, 2, 3, 6 },
            new object[] { 3, 4, 5, 11 },
            new object[] { 6, 7, 8, 22 }
        }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedSingleTest", new List<object[]> { new object[] { true } }));
        expectedEvents.Add(Tuple(SuiteAfter, suiteName, "After", 0));
        AssertTestCaseNames(events).ContainsExactly(expectedEvents);

        AssertEventStates(events).Contains(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            //Tuple(TESTCASE_BEFORE, "ParameterizedBoolValue", true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedBoolValue", new TestCaseAttribute(0, false)), true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedBoolValue", new TestCaseAttribute(1, true)), true, false, false, false),
            //Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue", new TestCaseAttribute()), true, false, false, false),
            //Tuple(TESTCASE_BEFORE, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute()), true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(1, 2, 3, 6)), true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(3, 4, 5, 12)), true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(6, 7, 8, 21)), true, false, false, false),
            //Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute()), true, false, false, false),
            // a test with failing test cases
            //Tuple(TESTCASE_BEFORE, "ParameterizedIntValuesFail", true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(1, 2, 3, 6)), true, false, false, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(3, 4, 5, 11)), false, false, true, false),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(6, 7, 8, 22)), false, false, true, false),
            //Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute()), false, false, true, false),
            // the single parameterized test
            //Tuple(TESTCASE_BEFORE, "ParameterizedSingleTest", true, false, false, false),
            Tuple(TestBefore, "ParameterizedSingleTest (True)", true, false, false, false),
            Tuple(TestAfter, "ParameterizedSingleTest (True)", true, false, false, false),
            //Tuple(TESTCASE_AFTER, "ParameterizedSingleTest", true, false, false, false),
            // test suite is failing
            Tuple(SuiteAfter, "After", false, false, true, false)
        );

        AssertReports(events).Contains(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedBoolValue", new TestCaseAttribute(0, false)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedBoolValue", new TestCaseAttribute(1, true)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(1, 2, 3, 6)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(3, 4, 5, 12)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValues", new TestCaseAttribute(6, 7, 8, 21)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(1, 2, 3, 6)), new List<TestReport>()),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(3, 4, 5, 11)), new List<TestReport>
            {
                new(Failure, 25, """
                                 Expecting be equal:
                                     '11' but is '12'
                                 """)
            }),
            Tuple(TestAfter, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new TestCaseAttribute(6, 7, 8, 22)), new List<TestReport>
            {
                new(Failure, 25, """
                                 Expecting be equal:
                                     '22' but is '21'
                                 """)
            }),
            Tuple(SuiteAfter, "After", new List<TestReport>())
        );
    }

    [TestCase(Description = "Verifies the exceptions are catches the right message as failure.")]
    public async Task ExecuteTestWithExceptions()
    {
        var events = await ExecuteTestSuite<TestSuiteAllTestsFailWithExceptions>();

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAllTestsFailWithExceptions", "ExceptionIsThrownOnSceneInvoke", "ExceptionAtAsyncMethod", "ExceptionAtSyncMethod"));

        // we expect all tests are failing and commits failures
        AssertEventCounters(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", 0, 0, 0),
            Tuple(TestBefore, "ExceptionIsThrownOnSceneInvoke", 0, 0, 0),
            Tuple(TestAfter, "ExceptionIsThrownOnSceneInvoke", 0, 1, 0),
            Tuple(TestBefore, "ExceptionAtAsyncMethod", 0, 0, 0),
            Tuple(TestAfter, "ExceptionAtAsyncMethod", 0, 1, 0),
            Tuple(TestBefore, "ExceptionAtSyncMethod", 0, 0, 0),
            Tuple(TestAfter, "ExceptionAtSyncMethod", 0, 1, 0),
            Tuple(SuiteAfter, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(SuiteBefore, "Before", true, false, false, false),
            Tuple(TestBefore, "ExceptionIsThrownOnSceneInvoke", true, false, false, false),
            Tuple(TestAfter, "ExceptionIsThrownOnSceneInvoke", false, false, true, false),
            Tuple(TestBefore, "ExceptionAtAsyncMethod", true, false, false, false),
            Tuple(TestAfter, "ExceptionAtAsyncMethod", false, false, true, false),
            Tuple(TestBefore, "ExceptionAtSyncMethod", true, false, false, false),
            Tuple(TestAfter, "ExceptionAtSyncMethod", false, false, true, false),
            // report suite is not success, is failed
            Tuple(SuiteAfter, "After", false, false, true, false)
        );
        // check for failure reports
        AssertReports(events).Contains(
            Tuple(SuiteBefore, "Before", new List<TestReport>()),
            Tuple(TestAfter, "ExceptionIsThrownOnSceneInvoke", new List<TestReport>
            {
                new(Failure, 14, """
                                 Test Exception
                                 """)
            }),
            Tuple(TestAfter, "ExceptionAtAsyncMethod", new List<TestReport>
            {
                new(Failure, 23, """
                                 outer exception
                                 """)
            }),
            Tuple(TestAfter, "ExceptionAtSyncMethod", new List<TestReport>
            {
                new(Failure, 27, """
                                 outer exception
                                 """)
            }),
            Tuple(SuiteAfter, "After", new List<TestReport>()));
    }
}
