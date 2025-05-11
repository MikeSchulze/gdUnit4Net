// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Api;

using Newtonsoft.Json;

using Reporting;

internal class TestEvent : ITestEvent, IEquatable<TestEvent>
{
    // constructor needs to serialize/deserialize by JsonConvert
    [JsonConstructor]

    // ReSharper disable once NotNullOrRequiredMemberIsNotInitialized
    private TestEvent()
    {
    }

    private TestEvent(ITestEvent.EventType eventType, string resourcePath, string suiteName, string testName, int totalCount = 0,
        IDictionary<STATISTIC_KEY, object>? statistics = null,
        IEnumerable<ITestReport>? reports = null)
    {
        Type = eventType;
        ResourcePath = resourcePath;
        SuiteName = suiteName;
        TestName = testName;
        Statistics = statistics ?? new Dictionary<STATISTIC_KEY, object>();
        Statistics[STATISTIC_KEY.TOTAL_COUNT] = totalCount;
        Reports = reports?.ToList() ?? new List<ITestReport>();
        FullyQualifiedName = string.Empty;
    }

    private TestEvent(ITestEvent.EventType eventType, Guid id, string resourcePath, string suiteName, string testName)
    {
        Type = eventType;
        Id = id;
        ResourcePath = resourcePath;
        SuiteName = suiteName;
        TestName = testName;
        Statistics = new Dictionary<STATISTIC_KEY, object>
        {
            [STATISTIC_KEY.TOTAL_COUNT] = 0
        };
        Reports = new List<ITestReport>();
        FullyQualifiedName = string.Empty;
    }

    public IDictionary<STATISTIC_KEY, object> Statistics { get; private init; } = new Dictionary<STATISTIC_KEY, object>();

    public int TotalCount => GetByKeyOrDefault(STATISTIC_KEY.TOTAL_COUNT, 0);

    public int ErrorCount => GetByKeyOrDefault(STATISTIC_KEY.ERROR_COUNT, 0);

    public int FailedCount => GetByKeyOrDefault(STATISTIC_KEY.FAILED_COUNT, 0);

    public int OrphanCount => GetByKeyOrDefault(STATISTIC_KEY.ORPHAN_NODES, 0);

    public int SkippedCount => GetByKeyOrDefault(STATISTIC_KEY.SKIPPED_COUNT, 0);

    public bool Equals(TestEvent? other) =>
        other is not null &&
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

    [JsonProperty]
    [JsonConverter(typeof(TestReportListConverter))]
    public ICollection<ITestReport> Reports { get; private init; } = new List<ITestReport>();

    public bool IsWarning => GetByKeyOrDefault(STATISTIC_KEY.WARNINGS, false);

    public bool IsSkipped => GetByKeyOrDefault(STATISTIC_KEY.SKIPPED, false);

    public bool IsFailed => GetByKeyOrDefault(STATISTIC_KEY.FAILED, false);

    public bool IsError => GetByKeyOrDefault(STATISTIC_KEY.ERRORS, false);

    public bool IsSuccess => !IsWarning && !IsFailed && !IsError && !IsSkipped;

    public TimeSpan ElapsedInMs => TimeSpan.FromMilliseconds(GetByKeyOrDefault(STATISTIC_KEY.ELAPSED_TIME, 0));

    public string? DisplayName { get; set; }

    public static TestEvent Before(string resourcePath, string suiteName, int totalCount, IDictionary<STATISTIC_KEY, object> statistics, IEnumerable<ITestReport> reports) =>
        new(ITestEvent.EventType.SuiteBefore, resourcePath, suiteName, "Before", totalCount, statistics, reports);

    public static TestEvent After(string resourcePath, string suiteName, IDictionary<STATISTIC_KEY, object> statistics, IEnumerable<ITestReport> reports) =>
        new(ITestEvent.EventType.SuiteAfter, resourcePath, suiteName, "After", 0, statistics, reports);

    public static TestEvent BeforeTest(Guid id, string resourcePath, string suiteName, string testName) =>
        new(ITestEvent.EventType.TestBefore, id, resourcePath, suiteName, testName);

    public static TestEvent AfterTest(Guid id, string resourcePath, string suiteName, string testName, IDictionary<STATISTIC_KEY, object>? statistics = null,
        List<ITestReport>? reports = null) =>
        new(ITestEvent.EventType.TestAfter, id, resourcePath, suiteName, testName)
        {
            Statistics = statistics ?? new Dictionary<STATISTIC_KEY, object>(),
            Reports = reports ?? new List<ITestReport>()
        };

    public override bool Equals(object? obj)
    {
        if (obj is TestEvent other)
            return Equals(other);
        return false;
    }

    internal TestEvent WithStatistic(STATISTIC_KEY key, object value)
    {
        Statistics[key] = value;
        return this;
    }

    internal TestEvent WithFullyQualifiedName(string name)
    {
        FullyQualifiedName = name;
        return this;
    }

    internal TestEvent WithDisplayName(string? name)
    {
        DisplayName = name;
        return this;
    }

    internal TestEvent WithReport(ITestReport report)
    {
        Reports.Add(report);
        return this;
    }

    public static IDictionary<STATISTIC_KEY, object> BuildStatistics(
        int orphanCount,
        bool isError, int errorCount,
        bool isFailure, int failureCount,
        bool isWarning,
        bool isSkipped, int skippedCount,
        long elapsedSinceMs) => new Dictionary<STATISTIC_KEY, object>
    {
        { STATISTIC_KEY.ORPHAN_NODES, orphanCount },
        { STATISTIC_KEY.ELAPSED_TIME, elapsedSinceMs },
        { STATISTIC_KEY.WARNINGS, isWarning },
        { STATISTIC_KEY.ERRORS, isError },
        { STATISTIC_KEY.ERROR_COUNT, errorCount },
        { STATISTIC_KEY.FAILED, isFailure },
        { STATISTIC_KEY.FAILED_COUNT, failureCount },
        { STATISTIC_KEY.SKIPPED, isSkipped },
        { STATISTIC_KEY.SKIPPED_COUNT, skippedCount }
    };

#pragma warning disable CA1854
    private T GetByKeyOrDefault<T>(STATISTIC_KEY key, T defaultValue) =>
        Statistics.ContainsKey(key) ? (T)Convert.ChangeType(Statistics[key], typeof(T), CultureInfo.InvariantCulture) : defaultValue;

#pragma warning restore CA1854
    public override string ToString() => $"Event: {Type} {SuiteName}:{TestName}, IsSuccess:{IsSuccess} ";

    public static bool operator ==(TestEvent? lhs, TestEvent? rhs) => lhs?.Equals(rhs) ?? rhs is null;

    public static bool operator !=(TestEvent? lhs, TestEvent? rhs) => !(lhs == rhs);

    public override int GetHashCode() =>

        // ReSharper disable all NonReadonlyMemberInGetHashCode
        HashCode.Combine(
            Type,
            ResourcePath,
            SuiteName,
            TestName,
            TotalCount,
            ErrorCount,
            FailedCount,
            OrphanCount);

    // ReSharper enable all NonReadonlyMemberInGetHashCode
#pragma warning disable CA1707
    // ReSharper disable all InconsistentNaming

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

    // ReSharper enable all InconsistentNaming
#pragma warning restore CA1707

#nullable disable
    public ITestEvent.EventType Type { get; set; }

    public Guid Id { get; set; }

    public string SuiteName { get; set; }

    public string TestName { get; set; }

    public string FullyQualifiedName { get; set; }

    public string ResourcePath { get; set; }
}

public class TestReportListConverter : JsonConverter<List<ITestReport>>
{
    public override List<ITestReport> ReadJson(JsonReader reader, Type objectType, List<ITestReport> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var reports = serializer.Deserialize<List<TestReport>>(reader);
        return reports?.Cast<ITestReport>().ToList() ?? new List<ITestReport>();
    }

    public override void WriteJson(JsonWriter writer, List<ITestReport> value, JsonSerializer serializer) => serializer.Serialize(writer, value?.Cast<TestReport>().ToList());
}
