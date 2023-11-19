using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace GdUnit4;

public class TestEvent : IEquatable<TestEvent>
{
    public enum TYPE
    {
        INIT,
        STOP,
        TESTSUITE_BEFORE,
        TESTSUITE_AFTER,
        TESTCASE_BEFORE,
        TESTCASE_AFTER,
    }

    public enum STATISTIC_KEY
    {
        WARNINGS,
        FAILED,
        ERRORS,
        SKIPPED,
        ELAPSED_TIME,
        ORPHAN_NODES,
        TOTAL_COUNT,
        ERROR_COUNT,
        FAILED_COUNT,
        SKIPPED_COUNT
    }

#nullable enable

    // constructor needs to serialize/deserialize by JsonConvert
    TestEvent() { }

    private TestEvent(TYPE type, string resourcePath, string suiteName, string testName, int totalCount = 0, IDictionary<STATISTIC_KEY, object>? statistics = null, IEnumerable<TestReport>? reports = null)
    {
        Type = type;
        ResourcePath = resourcePath;
        SuiteName = suiteName;
        TestName = testName;
        Statistics = statistics ?? new Dictionary<STATISTIC_KEY, object>();
        Statistics[STATISTIC_KEY.TOTAL_COUNT] = totalCount;
        Reports = reports?.ToList() ?? new List<TestReport>();
    }

    public static TestEvent Before(string resourcePath, string suiteName, int totalCount) =>
        new(TYPE.TESTSUITE_BEFORE, resourcePath, suiteName, "Before", totalCount);

    public static TestEvent After(string resourcePath, string suiteName, IDictionary<STATISTIC_KEY, object> statistics, IEnumerable<TestReport> reports) =>
        new(TYPE.TESTSUITE_AFTER, resourcePath, suiteName, "After", 0, statistics, reports);

    public static TestEvent BeforeTest(string resourcePath, string suiteName, string testName) =>
        new(TYPE.TESTCASE_BEFORE, resourcePath, suiteName, testName);

    public static TestEvent AfterTest(string resourcePath, string suiteName, string testName, IDictionary<STATISTIC_KEY, object>? statistics = null, IEnumerable<TestReport>? reports = null) =>
        new(TYPE.TESTCASE_AFTER, resourcePath, suiteName, testName, 0, statistics, reports);

#nullable disable

    public static IDictionary<STATISTIC_KEY, object> BuildStatistics(int orphan_count,
        bool isError, int error_count,
        bool isFailure, int failure_count,
        bool is_warning,
        bool is_skipped, int skippedCount,
        long elapsed_since_ms)
    {
        return new Dictionary<STATISTIC_KEY, object>() {
            { STATISTIC_KEY.ORPHAN_NODES, orphan_count},
            { STATISTIC_KEY.ELAPSED_TIME, elapsed_since_ms},
            { STATISTIC_KEY.WARNINGS, is_warning},
            { STATISTIC_KEY.ERRORS, isError},
            { STATISTIC_KEY.ERROR_COUNT, error_count},
            { STATISTIC_KEY.FAILED, isFailure},
            { STATISTIC_KEY.FAILED_COUNT, failure_count},
            { STATISTIC_KEY.SKIPPED, is_skipped},
            { STATISTIC_KEY.SKIPPED_COUNT, skippedCount}};
    }

    public TYPE Type { get; set; }
    public string SuiteName { get; set; }
    public string TestName { get; set; }
    public string ResourcePath { get; set; }
    public IDictionary<STATISTIC_KEY, object> Statistics { get; set; } = new Dictionary<STATISTIC_KEY, object>();
    public IEnumerable<TestReport> Reports { get; set; } = new List<TestReport>();

    public int TotalCount => GetByKeyOrDefault(STATISTIC_KEY.TOTAL_COUNT, 0);
    public int ErrorCount => GetByKeyOrDefault(STATISTIC_KEY.ERROR_COUNT, 0);
    public int FailedCount => GetByKeyOrDefault(STATISTIC_KEY.FAILED_COUNT, 0);
    public int OrphanCount => GetByKeyOrDefault(STATISTIC_KEY.ORPHAN_NODES, 0);
    public int SkippedCount => GetByKeyOrDefault(STATISTIC_KEY.SKIPPED_COUNT, 0);
    public bool IsWarning => GetByKeyOrDefault(STATISTIC_KEY.WARNINGS, false);
    public bool IsFailed => GetByKeyOrDefault(STATISTIC_KEY.FAILED, false);
    public bool IsError => GetByKeyOrDefault(STATISTIC_KEY.ERRORS, false);
    public bool IsSkipped => GetByKeyOrDefault(STATISTIC_KEY.SKIPPED, false);
    public bool IsSuccess => !IsWarning && !IsFailed && !IsError && !IsSkipped;
    public TimeSpan ElapsedInMs => TimeSpan.FromMilliseconds(GetByKeyOrDefault(STATISTIC_KEY.ELAPSED_TIME, 0));

    private T GetByKeyOrDefault<T>(STATISTIC_KEY key, T default_value) =>
        Statistics.ContainsKey(key) ? (T)Convert.ChangeType(Statistics[key], typeof(T), CultureInfo.InvariantCulture) : default_value;
    public override string ToString()
    {
        return $"Event: {Type} {SuiteName}:{TestName}, {""} ";
    }

#nullable enable
    public override bool Equals(object? obj) => obj is TestEvent other && this.Equals(other);
#nullable disable

    public static bool operator ==(TestEvent lhs, TestEvent rhs) => lhs.Equals(rhs);

    public static bool operator !=(TestEvent lhs, TestEvent rhs) => !(lhs == rhs);

    public bool Equals(TestEvent other) =>
        (Type,
        ResourcePath,
        SuiteName,
        TestName,
        TotalCount,
        ErrorCount,
        FailedCount,
        OrphanCount)
        .Equals((
            other.Type,
            other.ResourcePath,
            other.SuiteName,
            other.TestName,
            other.TotalCount,
            other.ErrorCount,
            other.FailedCount,
            other.OrphanCount));

    public override int GetHashCode() =>
        HashCode.Combine(Type,
            ResourcePath,
            SuiteName,
            TestName,
            TotalCount,
            ErrorCount,
            FailedCount,
            OrphanCount);
}
