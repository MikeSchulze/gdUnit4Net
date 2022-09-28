using System.Collections.Generic;
using System.Threading.Tasks;

namespace GdUnit3.Tests
{
    using GdUnit3.Asserts;
    using Executions;

    using static Assertions;
    using static TestEvent.TYPE;
    using static TestReport.TYPE;


    [TestSuite]
    public class ExecutorTest : ITestEventListener
    {
        private Executor _executor = null!;
        private List<TestEvent> _events = new List<TestEvent>();

        // enable to verbose debug event 
        private bool _verbose = false;

        [Before]
        public void Before()
        {
            _executor = new Executor();
            _executor.AddTestEventListener(this);
        }

        private static TestSuite LoadTestSuite(string clazzPath)
        {
            TestSuite testSuite = new TestSuite(clazzPath);

            // we disable default test filtering
            testSuite.FilterDisabled = true;
            return testSuite;
        }

        public void PublishEvent(TestEvent e)
        {
            if (_verbose)
            {
                Godot.GD.PrintS("-------------------------------");
                Godot.GD.PrintS(e.Type, e.SuiteName, e.TestName, new Godot.Collections.Dictionary(e.Statistics));
                Godot.GD.PrintS("ErrorCount:", e.ErrorCount, "FailedCount:", e.FailedCount, "OrphanCount:", e.OrphanCount);
                var reports = new List<TestReport>(e.Reports).ConvertAll(r => new TestReport(r.Type, r.LineNumber, NormalizedFailureMessage(r.Message)));
                if (_verbose)
                    reports.ForEach(r => Godot.GD.PrintS("Reports ->", r));
            }
            _events.Add(e);
        }

        private async Task<List<TestEvent>> ExecuteTestSuite(TestSuite testSuite, bool enableOrphanDetection = true)
        {
            var testSuiteName = testSuite.Name;
            _events.Clear();

            _executor.ReportOrphanNodesEnabled = enableOrphanDetection;
            if (_verbose)
                Godot.GD.PrintS($"Execute {testSuiteName}.");
            await _executor.ExecuteInternally(testSuite);
            if (_verbose)
                Godot.GD.PrintS($"Execution {testSuiteName} done.");
            return _events;
        }

        private List<ITuple> ExpectedEvents(List<TestEvent> events, string suiteName, params string[] testCaseNames)
        {
            var expectedEvents = new List<ITuple>();

            expectedEvents.Add(Tuple(TESTSUITE_BEFORE, suiteName, "Before", testCaseNames.Length));
            foreach (var testCase in testCaseNames)
            {
                expectedEvents.Add(Tuple(TESTCASE_BEFORE, suiteName, testCase, 0));
                expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testCase, 0));
            }
            expectedEvents.Add(Tuple(TESTSUITE_AFTER, suiteName, "After", 0));
            return expectedEvents;
        }

        private IEnumerableAssert AssertTestCaseNames(List<TestEvent> events) =>
            AssertArray(events).ExtractV(Extr("Type"), Extr("SuiteName"), Extr("TestName"), Extr("TotalCount"));

        private IEnumerableAssert AssertEventCounters(List<TestEvent> events) =>
            AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("ErrorCount"), Extr("FailedCount"), Extr("OrphanCount"));

        private IEnumerableAssert AssertEventStates(List<TestEvent> events) =>
             AssertArray(events).ExtractV(Extr("Type"), Extr("TestName"), Extr("IsSuccess"), Extr("IsWarning"), Extr("IsFailed"), Extr("IsError"));


        private IEnumerableAssert AssertReports(List<TestEvent> events)
        {
            var extractedEvents = events.ConvertAll(e =>
            {
                var reports = new List<TestReport>(e.Reports).ConvertAll(r => new TestReport(r.Type, r.LineNumber, NormalizedFailureMessage(r.Message)));
                return new { e.TestName, EventType = e.Type, Reports = reports };
            });
            return AssertArray(extractedEvents).ExtractV(Extr("EventType"), Extr("TestName"), Extr("Reports"));
        }

        private static string NormalizedFailureMessage(string input)
        {
            using (var rtl = new Godot.RichTextLabel())
            {
                rtl.BbcodeEnabled = true;
                rtl.ParseBbcode(input);
                var text = rtl.Text;
                rtl.Free();
                return text.Replace("\n", "").Replace("\r", "");
            }
        }

        private static List<ITuple> ExpectedTestCase(string suiteName, string testName, List<object[]> testCaseParams)
        {
            var expectedEvents = new List<ITuple>();
            expectedEvents.Add(Tuple(TESTCASE_BEFORE, suiteName, testName, 0));
            foreach (var testCaseParam in testCaseParams)
            {
                string testCaseName = TestCaseName(testName, testCaseParam);
                expectedEvents.Add(Tuple(TESTCASE_BEFORE, suiteName, testCaseName, 0));
                expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testCaseName, 0));
            }
            expectedEvents.Add(Tuple(TESTCASE_AFTER, suiteName, testName, 0));
            return expectedEvents;
        }

        private static string TestCaseName(string testName, object[] testCaseParam) => $"{testName} [{testCaseParam.Formated()}]";

        [TestCase(Description = "Verifies the complete test suite ends with success and no failures are reported.")]
        public async Task Execute_Success()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteAllStagesSuccess.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteAllStagesSuccess", "TestCase1", "TestCase2"));

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
        public async Task Execute_FailureOnStage_Before()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnStageBefore.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnStageBefore", "TestCase1", "TestCase2"));

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
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new TestReport(FAILURE, 13, "failed on Before()") }));
        }

        [TestCase(Description = "Verifies report a failure on stage 'After'.")]
        public async Task Execute_FailureOnStage_After()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnStageAfter.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnStageAfter", "TestCase1", "TestCase2"));

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
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new TestReport(FAILURE, 19, "failed on After()") }));
        }

        [TestCase(Description = "Verifies report a failure on stage 'BeforeTest'.")]
        public async Task Execute_FailureOnStage_BeforeTest()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnStageBeforeTest.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnStageBeforeTest", "TestCase1", "TestCase2"));

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
                Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new TestReport(FAILURE, 25, "failed on BeforeTest()") }),
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new TestReport(FAILURE, 25, "failed on BeforeTest()") }),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
        }

        [TestCase(Description = "Verifies report a failure on stage 'AfterTest'.")]
        public async Task Execute_FailureOnStage_AfterTest()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnStageAfterTest.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnStageAfterTest", "TestCase1", "TestCase2"));

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
                Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new TestReport(FAILURE, 31, "failed on AfterTest()") }),
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new TestReport(FAILURE, 31, "failed on AfterTest()") }),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
        }

        [TestCase(Description = "Verifies a failure is reportes for a single test case.")]
        public async Task Execute_FailureOn_TestCase1()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnTestCase1.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnTestCase1", "TestCase1", "TestCase2"));

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
                Tuple(TESTCASE_AFTER, "TestCase1", new List<TestReport>() { new TestReport(FAILURE, 36, "Expecting be equal:  'TestCase1' but is  'invalid'") }),
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>()),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
        }

        [TestCase(Description = "Verifies multiple failures are reportes for different stages.")]
        public async Task Execute_FailureOn_MultiStages()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailOnMultiStages.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailOnMultiStages", "TestCase1", "TestCase2"));

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
                new TestReport(FAILURE, 25, "failed on BeforeTest()"),
                new TestReport(FAILURE, 37, "Expecting be empty: but is  'TestCase1'")}),
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() { new TestReport(FAILURE, 25, "failed on BeforeTest()") }),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new TestReport(FAILURE, 19, "failed on After()") }));
        }

        [TestCase(Description = "GD-63: Execution must detect orphan nodes in the different test stages.")]
        public async Task Execute_Failure_OrphanNodesDetected()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailAndOrpahnsDetected.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailAndOrpahnsDetected", "TestCase1", "TestCase2"));

            // we expect orphans detected on multiple stages
            AssertEventCounters(events).ContainsExactly(
                Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),
                Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
                // TestCase1 ends with a warning and in summ 5 orphans detected
                // 2 from stage 'BeforeTest' + 3 from test itself
                Tuple(TESTCASE_AFTER, "TestCase1", 0, 0, 5),
                Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
                // TestCase2 ends with a one failure and in summ 6 orphans detected
                // 2 from stage 'befoBeforeTestre_test' + 4 from test itself
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
                new TestReport(WARN, 0, "WARNING: Detected <2> orphan nodes during test setup stage! Check SetupTest:28 and TearDownTest:36 for unfreed instances!"),
                new TestReport(WARN, 43, "WARNING: Detected <3> orphan nodes during test execution!")}),
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                // ends with failure and warnings 
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>() {
                new TestReport(WARN, 0, "WARNING: Detected <2> orphan nodes during test setup stage! Check SetupTest:28 and TearDownTest:36 for unfreed instances!"),
                new TestReport(WARN, 52, "WARNING: Detected <4> orphan nodes during test execution!"),
                new TestReport(FAILURE, 58, "Expecting be empty: but is  'TestCase2'") }),
                // and one orphan detected at stage 'After'
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>() { new TestReport(WARN, 0, "WARNING: Detected <1> orphan nodes during test suite setup stage! Check SetupSuite:15 and TearDownSuite:22 for unfreed instances!") }));
        }

        [TestCase(Description = "GD-62: Execution must ignore detect orphan nodes if is disabled.")]
        public async Task Execute_Failure_OrphanNodesDetection_Disabled()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteFailAndOrpahnsDetected.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2" });

            // simulate test suite execution with disabled orphan detection
            var events = await ExecuteTestSuite(testSuite, false);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteFailAndOrpahnsDetected", "TestCase1", "TestCase2"));

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
                new TestReport(FAILURE, 58, "Expecting be empty: but is  'TestCase2'") }),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
        }

        [TestCase(Description = "GD-66: The execution must be aborted by a test timeout.")]
        public async Task Execute_Abort_OnTimeOut()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteAbortOnTestTimeout.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] { "TestCase1", "TestCase2", "TestCase3", "TestCase4", "TestCase5" });

            var events = await ExecuteTestSuite(testSuite);

            AssertTestCaseNames(events)
                .ContainsExactly(ExpectedEvents(events, "TestSuiteAbortOnTestTimeout", "TestCase1", "TestCase2", "TestCase3", "TestCase4", "TestCase5"));


            // "ErrorCount", "FailedCount", "OrphanCount"
            AssertEventCounters(events).ContainsExactly(
                Tuple(TESTSUITE_BEFORE, "Before", 0, 0, 0),

                // expect test error by a timeout
                Tuple(TESTCASE_BEFORE, "TestCase1", 0, 0, 0),
                Tuple(TESTCASE_AFTER, "TestCase1", 1, 0, 0),

                // expect test failed by reported failure
                Tuple(TESTCASE_BEFORE, "TestCase2", 0, 0, 0),
                Tuple(TESTCASE_AFTER, "TestCase2", 0, 1, 0),

                // expect test succeded
                Tuple(TESTCASE_BEFORE, "TestCase3", 0, 0, 0),
                Tuple(TESTCASE_AFTER, "TestCase3", 0, 0, 0),

                // expect to fail, invalic method signature
                Tuple(TESTCASE_BEFORE, "TestCase4", 0, 0, 0),
                Tuple(TESTCASE_AFTER, "TestCase4", 0, 1, 0),

                // expect test succeded
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

                //  test case is succeded
                Tuple(TESTCASE_BEFORE, "TestCase3", true, false, false, false),
                Tuple(TESTCASE_AFTER, "TestCase3", true, false, false, false),

                //  test fails by invalid method signature
                Tuple(TESTCASE_BEFORE, "TestCase4", true, false, false, false),
                Tuple(TESTCASE_AFTER, "TestCase4", false, false, true, false),

                //  test case is succeded
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
                new TestReport(INTERUPTED, 33, "The execution has timed out after 1000ms.") }),

                // reports a test failure
                Tuple(TESTCASE_BEFORE, "TestCase2", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase2", new List<TestReport>(){
                new TestReport(FAILURE, 45, "Expecting be equal:  'False' but is 'True'") }),

                // succedes with no reports
                Tuple(TESTCASE_BEFORE, "TestCase3", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase3", new List<TestReport>()),

                // reports a method signature failure
                Tuple(TESTCASE_BEFORE, "TestCase4", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase4", new List<TestReport>(){
                new TestReport(FAILURE, 58, "Invalid method signature found at: TestCase4. You must return a <Task> for an asynchronously specified method.") }),

                // succedes with no reports
                Tuple(TESTCASE_BEFORE, "TestCase5", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, "TestCase5", new List<TestReport>()),

                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>()));
        }

        [TestCase(Description = "Tests is all parameterized tests case executed.")]
        public async Task Execute_ParameterizedTest()
        {
            TestSuite testSuite = LoadTestSuite("test/core/resources/testsuites/mono/TestSuiteParameterizedTests.cs");
            AssertArray(testSuite.TestCases).Extract("Name").ContainsExactly(new string[] {
                "ParameterizedBoolValue",
                "ParameterizedIntValues",
                "ParameterizedIntValuesFail" });

            var events = await ExecuteTestSuite(testSuite);

            var suiteName = "TestSuiteParameterizedTests";
            var expectedEvents = new List<ITuple>();
            expectedEvents.Add(Tuple(TESTSUITE_BEFORE, suiteName, "Before", 3));
            expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedBoolValue", new List<object[]> {
                new object[] { 0, false }, new object[] { 1, true } }));
            expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValues", new List<object[]> {
                new object[] { 1, 2, 3, 6 }, new object[] { 3, 4, 5, 12 }, new object[] { 6, 7, 8, 21 } }));
            expectedEvents.AddRange(ExpectedTestCase(suiteName, "ParameterizedIntValuesFail", new List<object[]> {
                new object[] { 1, 2, 3, 6 }, new object[] { 3, 4, 5, 11 }, new object[] { 6, 7, 8, 22 } }));
            expectedEvents.Add(Tuple(TESTSUITE_AFTER, suiteName, "After", 0));
            AssertTestCaseNames(events).ContainsExactly(expectedEvents);

            AssertEventStates(events).Contains(
                Tuple(TESTSUITE_BEFORE, "Before", true, false, false, false),
                Tuple(TESTCASE_BEFORE, "ParameterizedBoolValue", true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedBoolValue", new object[] { 0, false }), true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedBoolValue", new object[] { 1, true }), true, false, false, false),
                Tuple(TESTCASE_AFTER, "ParameterizedBoolValue", true, false, false, false),
                Tuple(TESTCASE_BEFORE, "ParameterizedIntValues", true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 1, 2, 3, 6 }), true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 3, 4, 5, 12 }), true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 6, 7, 8, 21 }), true, false, false, false),
                Tuple(TESTCASE_AFTER, "ParameterizedIntValues", true, false, false, false),
                // a test with failing test cases
                Tuple(TESTCASE_BEFORE, "ParameterizedIntValuesFail", true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 1, 2, 3, 6 }), true, false, false, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 3, 4, 5, 11 }), false, false, true, false),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 6, 7, 8, 22 }), false, false, true, false),
                Tuple(TESTCASE_AFTER, "ParameterizedIntValuesFail", false, false, true, false),
                // test suite is failing
                Tuple(TESTSUITE_AFTER, "After", false, false, true, false)
            );

            AssertReports(events).Contains(
                Tuple(TESTSUITE_BEFORE, "Before", new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedBoolValue", new object[] { 0, false }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedBoolValue", new object[] { 1, true }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 1, 2, 3, 6 }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 3, 4, 5, 12 }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValues", new object[] { 6, 7, 8, 21 }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 1, 2, 3, 6 }), new List<TestReport>()),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 3, 4, 5, 11 }), new List<TestReport>(){
                    new TestReport(FAILURE, 30, "Expecting be equal:  '11' but is '12'")
                }),
                Tuple(TESTCASE_AFTER, TestCaseName("ParameterizedIntValuesFail", new object[] { 6, 7, 8, 22 }), new List<TestReport>(){
                    new TestReport(FAILURE, 30, "Expecting be equal:  '22' but is '21'")
                }),
                Tuple(TESTSUITE_AFTER, "After", new List<TestReport>())
            );
        }
    }
}
