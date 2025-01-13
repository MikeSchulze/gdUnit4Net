namespace GdUnit4.Tests.Core.Events;

using System;
using System.Collections.Generic;

using GdUnit4.Core.Events;
using GdUnit4.Core.Reporting;

using Newtonsoft.Json;

using static Assertions;

[TestSuite]
public class TestEventTest
{
    [TestCase]
    public void SerializeDeserializeBefore()
    {
        var testEvent = TestEvent.Before("foo/bar/TestSuiteXXX.cs", "TestSuiteXXX", 100);
        var json = JsonConvert.SerializeObject(testEvent);

        var current = JsonConvert.DeserializeObject<TestEvent>(json);
        AssertThat(current).IsEqual(testEvent);
        AssertThat(current!.SuiteName).IsEqual("TestSuiteXXX");
        AssertThat(current!.TestName).IsEqual("Before");
    }

    [TestCase]
    public void SerializeDeserializeBeforeTest()
    {
        var testEvent = TestEvent.BeforeTest(Guid.Empty, "foo/bar/TestSuiteXXX.cs", "TestSuiteXXX", "TestCaseA");
        var json = JsonConvert.SerializeObject(testEvent);

        var current = JsonConvert.DeserializeObject<TestEvent>(json);
        AssertThat(current).IsEqual(testEvent);
    }

    [TestCase]
    public void SerializeDeserializeAfter()
    {
        Dictionary<TestEvent.STATISTIC_KEY, object> statistics = new()
        {
            { TestEvent.STATISTIC_KEY.ELAPSED_TIME, 124 },
            { TestEvent.STATISTIC_KEY.ERROR_COUNT, 2 },
            { TestEvent.STATISTIC_KEY.FAILED_COUNT, 3 },
            { TestEvent.STATISTIC_KEY.SKIPPED_COUNT, 4 },
            { TestEvent.STATISTIC_KEY.ORPHAN_NODES, 0 },
            { TestEvent.STATISTIC_KEY.FAILED, true },
            { TestEvent.STATISTIC_KEY.ERRORS, false },
            { TestEvent.STATISTIC_KEY.WARNINGS, false },
            { TestEvent.STATISTIC_KEY.SKIPPED, true }
        };

        List<TestReport> reports = new() { new TestReport(TestReport.ReportType.FAILURE, 42, "test failed") };

        var testEvent = TestEvent.After("foo/bar/TestSuiteXXX.cs", "TestSuiteXXX", statistics, reports);
        var json = JsonConvert.SerializeObject(testEvent);

        var current = JsonConvert.DeserializeObject<TestEvent>(json);
        AssertThat(current).IsNotNull().IsEqual(testEvent);
        AssertThat(current!.Reports).Contains(new TestReport(TestReport.ReportType.FAILURE, 42, "test failed"));
        AssertThat(current.SuiteName).IsEqual("TestSuiteXXX");
        AssertThat(current.TestName).IsEqual("After");
        AssertThat(current.ElapsedInMs).IsEqual(TimeSpan.FromMilliseconds(124));
        AssertThat(current.ErrorCount).IsEqual(2);
        AssertThat(current.FailedCount).IsEqual(3);
        AssertThat(current.SkippedCount).IsEqual(4);
        AssertThat(current.OrphanCount).IsEqual(0);
        AssertThat(current.IsFailed).IsEqual(true);
        AssertThat(current.IsError).IsEqual(false);
        AssertThat(current.IsWarning).IsEqual(false);
        AssertThat(current.IsSkipped).IsEqual(true);
    }

    [TestCase]
    public void SerializeDeserializeAfterTest()
    {
        Dictionary<TestEvent.STATISTIC_KEY, object> statistics = new()
        {
            { TestEvent.STATISTIC_KEY.ELAPSED_TIME, 124 },
            { TestEvent.STATISTIC_KEY.ERROR_COUNT, 2 },
            { TestEvent.STATISTIC_KEY.FAILED_COUNT, 3 },
            { TestEvent.STATISTIC_KEY.SKIPPED_COUNT, 4 },
            { TestEvent.STATISTIC_KEY.ORPHAN_NODES, 0 },
            { TestEvent.STATISTIC_KEY.FAILED, true },
            { TestEvent.STATISTIC_KEY.ERRORS, false },
            { TestEvent.STATISTIC_KEY.WARNINGS, false },
            { TestEvent.STATISTIC_KEY.SKIPPED, true }
        };

        List<TestReport> reports = new() { new TestReport(TestReport.ReportType.FAILURE, 42, "test failed") };

        var testEvent = TestEvent.AfterTest(Guid.Empty, "foo/bar/TestSuiteXXX.cs", "TestSuiteXXX", "TestCaseA", statistics, reports);
        var json = JsonConvert.SerializeObject(testEvent);

        var current = JsonConvert.DeserializeObject<TestEvent>(json);
        AssertThat(current).IsNotNull().IsEqual(testEvent);
        AssertThat(current!.Reports).Contains(new TestReport(TestReport.ReportType.FAILURE, 42, "test failed"));
        AssertThat(current.Id).IsEqual(testEvent.Id);
        AssertThat(current.ElapsedInMs).IsEqual(TimeSpan.FromMilliseconds(124));
        AssertThat(current.ErrorCount).IsEqual(2);
        AssertThat(current.FailedCount).IsEqual(3);
        AssertThat(current.SkippedCount).IsEqual(4);
        AssertThat(current.OrphanCount).IsEqual(0);
        AssertThat(current.IsFailed).IsEqual(true);
        AssertThat(current.IsError).IsEqual(false);
        AssertThat(current.IsWarning).IsEqual(false);
        AssertThat(current.IsSkipped).IsEqual(true);
    }
}
