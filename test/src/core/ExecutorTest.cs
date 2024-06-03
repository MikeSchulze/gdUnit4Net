namespace GdUnit4.Tests.Core;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GdUnit4.Asserts;
using GdUnit4.Core;
using GdUnit4.Executions;

using static Assertions;
using static TestEvent.TYPE;
using static TestReport.ReportType;

[TestSuite]
#pragma warning disable CA1001 // Types that own disposable fields should be disposable
public class ExecutorTest : ITestEventListener
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
{
    private Executor executor = null!;
    private readonly List<TestEvent> events = new();

#pragma warning disable CS0649
    // enable to verbose debug event
    private readonly bool verbose;
#pragma warning restore CS0649

    public bool IsFailed { get; set; }

    [Before]
    public void Before()
    {
        executor = new Executor();
        executor.AddTestEventListener(this);
    }

    [BeforeTest]
    public void InitTest()
        => Godot.ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, true);

    [AfterTest]
    public void TeardownTest()
        => Godot.ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, true);


    private static TestSuite LoadTestSuite(string clazzPath)
    {
        var testSuite = new TestSuite(clazzPath, null, false)
        {
            // we disable default test filtering
            FilterDisabled = true
        };
        return testSuite;
    }
    void ITestEventListener.PublishEvent(TestEvent e)
    {
        if (verbose)
        {
            Console.WriteLine("-------------------------------");
            Console.WriteLine($"Event Type: {e.Type}, SuiteName: {e.SuiteName}, TestName: {e.TestName}, Statistics: {e.Statistics}");
            Console.WriteLine($"ErrorCount: {e.ErrorCount}, FailedCount: {e.FailedCount}, OrphanCount: {e.OrphanCount}");
            var reports = new List<TestReport>(e.Reports).ConvertAll(r => new TestReport(r.Type, r.LineNumber, r.Message.RichTextNormalize()));
            if (verbose)
                reports.ForEach(r => Console.WriteLine($"Reports -> {r}"));
        }
        events.Add(e);
    }

    private async Task<List<TestEvent>> ExecuteTestSuite(TestSuite testSuite)
    {
        var testSuiteName = testSuite.Name;
        events.Clear();

        if (verbose)
            Console.WriteLine($"Execute {testSuiteName}.");
        await executor.ExecuteInternally(testSuite);
        if (verbose)
            Console.WriteLine($"Execution {testSuiteName} done.");
        return events;
    }

    private List<ITuple> ExpectedEvents(string suiteName, params string[] testCaseNames)
    {
        var expectedEvents = new List<ITuple>
        {
            Tuple(TESTSUITE_BEFORE, suiteName, "Before", testCaseNames.Length)
        };
        foreach (var testCase in testCaseNames)
        {
            expectedEvents.Add(Tuple(TESTCASE_BEFORE, suiteName, testCase, 0));
            expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testCase, 0));
        }
        expectedEvents.Add(Tuple(TESTSUITE_AFTER, suiteName, "After", 0));
        return expectedEvents;
    }

    private IEnumerableAssert<object?> AssertTestCaseNames(List<TestEvent> events) =>
        AssertArray(events).ExtractV(Extr("Type"), Extr("SuiteName"), Extr("TestName"), Extr("TotalCount"));

    private IEnumerableAssert<object?> AssertEventCounters(List<TestEvent> events) =>
        AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("ErrorCount"), Extr("FailedCount"), Extr("OrphanCount"));

    private IEnumerableAssert<object?> AssertEventStates(List<TestEvent> events) =>
         AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("IsSuccess"), Extr("IsWarning"), Extr("IsFailed"), Extr("IsError"));

    private IEnumerableAssert<object?> AssertReports(List<TestEvent> events)
    {
        var extractedEvents = events.ConvertAll(e =>
        {
            var reports = new List<TestReport>(e.Reports).ConvertAll(r => new TestReport(r.Type, r.LineNumber, r.Message.RichTextNormalize()));
            return new { e.TestName, EventType = e.Type, Reports = reports };
        });
        return AssertArray(extractedEvents).ExtractV(Extr("EventType"), Extr("TestName"), Extr("Reports"));
    }

    private static List<ITuple> ExpectedTestCase(string suiteName, string testName, List<object[]> testCaseParams)
    {
        var expectedEvents = new List<ITuple>
        {
            Tuple(TESTCASE_BEFORE, suiteName, testName, 0)
        };
        foreach (var testCaseParam in testCaseParams)
        {
            var testCaseName = TestCase.BuildDisplayName(testName, testCaseParam);
            expectedEvents.Add(Tuple(TESTCASE_BEFORE, suiteName, testCaseName, 0));
            expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testCaseName, 0));
        }
        expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testName, 0));
        return expectedEvents;
    }

    [TestCase(Description = "Verifies the complete test suite ends with success and no failures are reported.")]
    public async Task ExecuteSuccess()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteAllStagesSuccess.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAllStagesSuccess", "TestCase1", "TestCase2"));

        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 0, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", true, false, false, false),
            Tuple(TESTSUITE_AFTER, "After", true, false, false, false)
        );

        // all success no reports expected
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>()),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies report a failure on stage 'Before'.")]
    public async Task ExecuteFailureOnStageBefore()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnStageBefore.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageBefore", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'Before()' and commits one failure
        // where is reported finally at TESTSUITE_AFTER event
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 0, 0),
            // report failure failed_count = 1
            Tuple(TESTSUITE_AFTER, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // one failure at Before()
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>()),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new(FAILURE, 12, "failed on Before()") }));
    }

    [TestCase(Description = "Verifies report a failure on stage 'After'.")]
    public async Task ExecuteFailureOnStageAfter()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnStageAfter.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageAfter", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'After()' and commits one failure
        // where is reported finally at TESTSUITE_AFTER event
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 0, 0),
            // report failure failed_count = 1
            Tuple(TESTSUITE_AFTER, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // one failure at After()
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>()),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new(FAILURE, 16, "failed on After()") }));
    }

    [TestCase(Description = "Verifies report a failure on stage 'BeforeTest'.")]
    public async Task ExecuteFailureOnStageBeforeTest()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnStageBeforeTest.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageBeforeTest", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'BeforeTest()' and commits one failure on each test case
        // because is in scope of test execution
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 1, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // BeforeTest() failure report is append to each test
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new(FAILURE, 20, "failed on BeforeTest()") }),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new(FAILURE, 20, "failed on BeforeTest()") }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies report a failure on stage 'AfterTest'.")]
    public async Task ExecuteFailureOnStageAfterTest()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnStageAfterTest.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnStageAfterTest", "TestCase1", "TestCase2"));

        // we expect the testsuite is failing on stage 'AfterTest()' and commits one failure on each test case
        // because is in scope of test execution
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 1, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // AfterTest() failure report is append to each test
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new(FAILURE, 24, "failed on AfterTest()") }),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new(FAILURE, 24, "failed on AfterTest()") }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies a failure is reports for a single test case.")]
    public async Task ExecuteFailureOnTestCase1()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnTestCase1.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnTestCase1", "TestCase1", "TestCase2"));

        // we expect the test case 'TestCase1' is failing  and commits one failure
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 1, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 0, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", true, false, false, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // only 'TestCase1' reports a failure
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new(FAILURE, 27, """
                Expecting be equal:
                    "TestCase1"
                 but is
                    "invalid"
                """) }),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>()),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Verifies multiple failures are report's for different stages.")]
    public async Task ExecuteFailureOnMultiStages()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailOnMultiStages.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailOnMultiStages", "TestCase1", "TestCase2"));

        // we expect failing on multiple stages
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            // TestCase1 has a failure plus one from 'BeforeTest'
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 2, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            // the second test has no failures but one from 'BeforeTest'
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),
            // and one failure is on stage 'After' found
            Tuple(TESTSUITE_AFTER, "After", 0, 1, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // only 'TestCase1' reports a 'real' failure plus test setup stage failures
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() {
            new(FAILURE, 20, "failed on BeforeTest()"),
            new(FAILURE, 28, """
                Expecting be empty:
                 but is
                    "TestCase1"
                """)}),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new(FAILURE, 20, "failed on BeforeTest()") }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new(FAILURE, 16, "failed on After()") }));
    }

    [TestCase(Description = "GD-63: Execution must detect orphan nodes in the different test stages.")]
    public async Task ExecuteFailureOrphanNodesDetected()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailAndOrphansDetected.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        var events = await ExecuteTestSuite(testSuite);
        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailAndOrphansDetected", "TestCase1", "TestCase2"));

        // we expect orphans detected on multiple stages
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            // TestCase1 ends with a warning and in sum 5 orphans detected
            // 2 from stage 'BeforeTest' + 3 from test itself
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 5),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            // TestCase2 ends with a one failure and in sum 6 orphans detected
            // 2 from stage 'BeforeTest' + 4 from test itself
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 6),
            // and one orphan detected from stage 'Before'
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 1)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            // test case has only warnings
            Tuple(TESTCASE_AFTER, "TestCase1", false, true, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            //  test case has a failure and warnings
            Tuple(TESTCASE_AFTER, "TestCase2", false, true, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, true, true, false)
        );
        // only 'TestCase2' reports a 'real' failure plus test setup stage failures
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            // ends with warnings
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() {
                new(WARN, 0, """
                    WARNING:
                        Detected <2> orphan nodes during test setup stage!
                        Check SetupTest:26 and TearDownTest:34 for unfreed instances!
                    """),
                new(WARN, 39, """
                    WARNING:
                        Detected <3> orphan nodes during test execution!
                    """)
                }
            ),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            // ends with failure and warnings
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() {
                new(WARN, 0, """
                    WARNING:
                        Detected <2> orphan nodes during test setup stage!
                        Check SetupTest:26 and TearDownTest:34 for unfreed instances!
                    """),
                new(WARN, 48, """
                    WARNING:
                        Detected <4> orphan nodes during test execution!
                    """),
                new(FAILURE, 54, """
                    Expecting be empty:
                     but is
                        "TestCase2"
                    """)
                }
            ),
            // and one orphan detected at stage 'After'
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() {
                new(WARN, 0, """
                    WARNING:
                        Detected <1> orphan nodes during test suite setup stage!
                        Check SetupSuite:15 and TearDownSuite:22 for unfreed instances!
                    """)
                })
            );
    }

    [TestCase(Description = "GD-62: Execution must ignore detect orphan nodes if is disabled.")]
    public async Task ExecuteFailureOrphanNodesDetectionDisabled()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteFailAndOrphansDetected.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

        // simulate test suite execution with disabled orphan detection
        Godot.ProjectSettings.SetSetting(GdUnit4Settings.REPORT_ORPHANS, false);
        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteFailAndOrphansDetected", "TestCase1", "TestCase2"));

        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            //  test case has a failure
            Tuple(TESTCASE_AFTER, "TestCase2", false, false, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // only 'TestCase2' reports a failure, orphans are not reported
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            // ends with failure
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() {
                new(FAILURE, 54, """
                    Expecting be empty:
                     but is
                        "TestCase2"
                    """) }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "GD-66: The execution must be aborted by a test timeout.")]
    public async Task ExecuteAbortOnTimeOut()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteAbortOnTestTimeout.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2", "TestCase3", "TestCase4", "TestCase5" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAbortOnTestTimeout", "TestCase1", "TestCase2", "TestCase3", "TestCase4", "TestCase5"));

        // "ErrorCount", "FailedCount", "OrphanCount"
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),

            // expect test error by a timeout
            Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase1", 1, 0, 0),

            // expect test failed by reported failure
            Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),

            // expect test succeeded
            Tuple(TESTCASE_BEFORE, "TestCase3", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase3", 0, 0, 0),

            // expect to fail, invalid method signature
            Tuple(TESTCASE_BEFORE, "TestCase4", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase4", 0, 1, 0),

            // expect test succeeded
            Tuple(TESTCASE_BEFORE, "TestCase5", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "TestCase5", 0, 0, 0),

            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );

        // IsSuccess", "IsWarning", "IsFailed", "IsError"
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            //  test case is marked as error because of timeout
            Tuple(TESTCASE_BEFORE, "TestCase1", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase1", false, false, false, true),

            //  test case is marked as failure
            Tuple(TESTCASE_BEFORE, "TestCase2", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase2", false, false, true, false),

            //  test case is succeeded
            Tuple(TESTCASE_BEFORE, "TestCase3", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase3", true, false, false, false),

            //  test fails by invalid method signature
            Tuple(TESTCASE_BEFORE, "TestCase4", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase4", false, false, true, false),

            //  test case is succeeded
            Tuple(TESTCASE_BEFORE, "TestCase5", true, false, false, false),
            Tuple(TESTCASE_AFTER, "TestCase5", true, false, false, false),

            // report suite is not success, is failed and has a error
            Tuple(TESTSUITE_AFTER, "After", false, false, true, true)
        );
        AssertReports(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            // reports a test interruption due to a timeout
            Tuple(TESTCASE_BEFORE, "TestCase1", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>(){
                new(INTERRUPTED, 31, "The execution has timed out after 1s.") }
            ),

            // reports a test failure
            Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>(){
                new(FAILURE, 43, """
                    Expecting be equal:
                        'False' but is 'True'
                    """) }
            ),

            // succeeds with no reports
            Tuple(TESTCASE_BEFORE, "TestCase3", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase3", new List<TestReport>()),

            // reports a method signature failure
            Tuple(TESTCASE_BEFORE, "TestCase4", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase4", new List<TestReport>(){
                new(FAILURE, 55, """
                    Invalid method signature found at: TestCase4.
                     You must return a <Task> for an asynchronously specified method.
                    """) }
            ),

            // succeeds with no reports
            Tuple(TESTCASE_BEFORE, "TestCase5", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "TestCase5", new List<TestReport>()),

            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

    [TestCase(Description = "Tests is all parameterized tests case executed.")]
    public async Task ExecuteParameterizedTest()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteParameterizedTests.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] {
            "ParameterizedBoolValue",
            "ParameterizedIntValues",
            "ParameterizedIntValuesFail",
            "ParameterizedSingleTest" });

        var events = await ExecuteTestSuite(testSuite);

        var suiteName = "TestSuiteParameterizedTests";
        var expectedEvents = new List<ITuple>
        {
            Tuple(TESTSUITE_BEFORE, suiteName, "Before", 4)
        };
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedBoolValue", new List<object[]> {
            new object[] { 0, false }, new object[] { 1, true } }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValues", new List<object[]> {
            new object[] { 1, 2, 3, 6 }, new object[] { 3, 4, 5, 12 }, new object[] { 6, 7, 8, 21 } }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValuesFail", new List<object[]> {
            new object[] { 1, 2, 3, 6 }, new object[] { 3, 4, 5, 11 }, new object[] { 6, 7, 8, 22 } }));
        expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedSingleTest", new List<object[]> {
            new object[] { true } }));
        expectedEvents.Add(Tuple(TESTSUITE_AFTER, suiteName, "After", 0));
        AssertTestCaseNames(events).ContainsExactly(expectedEvents);

        AssertEventStates(events).Contains(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "ParameterizedBoolValue", true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue", new object[] { 0, false }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue", new object[] { 1, true }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue"), true, false, false, false),
            Tuple(TESTCASE_BEFORE, TestCase.BuildDisplayName("ParameterizedIntValues"), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 1, 2, 3, 6 }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 3, 4, 5, 12 }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 6, 7, 8, 21 }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues"), true, false, false, false),
            // a test with failing test cases
            Tuple(TESTCASE_BEFORE, "ParameterizedIntValuesFail", true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 1, 2, 3, 6 }), true, false, false, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 3, 4, 5, 11 }), false, false, true, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 6, 7, 8, 22 }), false, false, true, false),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail"), false, false, true, false),
            // the single parameterized test
            Tuple(TESTCASE_BEFORE, "ParameterizedSingleTest", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "ParameterizedSingleTest(True)", true, false, false, false),
            Tuple(TESTCASE_AFTER, "ParameterizedSingleTest(True)", true, false, false, false),
            Tuple(TESTCASE_AFTER, "ParameterizedSingleTest", true, false, false, false),
            // test suite is failing
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );

        AssertReports(events).Contains(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue", new object[] { 0, false }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedBoolValue", new object[] { 1, true }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 1, 2, 3, 6 }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 3, 4, 5, 12 }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValues", new object[] { 6, 7, 8, 21 }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 1, 2, 3, 6 }), new List<TestReport>()),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 3, 4, 5, 11 }), new List<TestReport>(){
                new(FAILURE, 25, """
                    Expecting be equal:
                        '11' but is '12'
                    """)
            }),
            Tuple(TESTCASE_AFTER, TestCase.BuildDisplayName("ParameterizedIntValuesFail", new object[] { 6, 7, 8, 22 }), new List<TestReport>(){
                new(FAILURE, 25, """
                    Expecting be equal:
                        '22' but is '21'
                    """)
            }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>())
        );
    }

    [TestCase(Description = "Verifies the exceptions are catches the right message as failure.")]
    public async Task ExecuteTestWithExceptions()
    {
        var testSuite = LoadTestSuite("src/core/resources/testsuites/mono/TestSuiteAllTestsFailWithExceptions.cs");
        AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "ExceptionIsThrownOnSceneInvoke", "ExceptionAtAsyncMethod", "ExceptionAtSyncMethod" });

        var events = await ExecuteTestSuite(testSuite);

        AssertTestCaseNames(events)
            .ContainsExactly(ExpectedEvents("TestSuiteAllTestsFailWithExceptions", "ExceptionIsThrownOnSceneInvoke", "ExceptionAtAsyncMethod", "ExceptionAtSyncMethod"));

        // we expect all tests are failing and commits failures
        AssertEventCounters(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
            Tuple(TESTCASE_BEFORE, "ExceptionIsThrownOnSceneInvoke", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "ExceptionIsThrownOnSceneInvoke", 0, 1, 0),
            Tuple(TESTCASE_BEFORE, "ExceptionAtAsyncMethod", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "ExceptionAtAsyncMethod", 0, 1, 0),
            Tuple(TESTCASE_BEFORE, "ExceptionAtSyncMethod", 0, 0, 0),
            Tuple(TESTCASE_AFTER, "ExceptionAtSyncMethod", 0, 1, 0),
            Tuple(TESTSUITE_AFTER, "After", 0, 0, 0)
        );
        AssertEventStates(events).ContainsExactly(
            Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
            Tuple(TESTCASE_BEFORE, "ExceptionIsThrownOnSceneInvoke", true, false, false, false),
            Tuple(TESTCASE_AFTER, "ExceptionIsThrownOnSceneInvoke", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "ExceptionAtAsyncMethod", true, false, false, false),
            Tuple(TESTCASE_AFTER, "ExceptionAtAsyncMethod", false, false, true, false),
            Tuple(TESTCASE_BEFORE, "ExceptionAtSyncMethod", true, false, false, false),
            Tuple(TESTCASE_AFTER, "ExceptionAtSyncMethod", false, false, true, false),
            // report suite is not success, is failed
            Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
        );
        // check for failure reports
        AssertReports(events).Contains(
            Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
            Tuple(TESTCASE_AFTER, "ExceptionIsThrownOnSceneInvoke", new List<TestReport>() { new(FAILURE, 12, """
                Test Exception
                """) }),
            Tuple(TESTCASE_AFTER, "ExceptionAtAsyncMethod", new List<TestReport>() { new(FAILURE, 24, """
                outer exception
                """) }),
            Tuple(TESTCASE_AFTER, "ExceptionAtSyncMethod", new List<TestReport>() { new(FAILURE, 28, """
                outer exception
                """) }),
            Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
    }

}
