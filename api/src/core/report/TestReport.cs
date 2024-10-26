namespace GdUnit4;

using System;
using System.Collections.Generic;
using System.Linq;

using Core.Execution.Exceptions;
using Core.Extensions;

using Newtonsoft.Json;

internal sealed class TestReport : IEquatable<TestReport>
{
    [Flags]
    public enum ReportType
    {
        SUCCESS,
        WARN,
        FAILURE,
        ORPHAN,
        TERMINATED,
        INTERRUPTED,
        ABORT,
        SKIPPED,
        STDOUT
    }

    [JsonConstructor]
    public TestReport(ReportType type, int lineNumber, string message, string? stackTrace = null)
    {
        Type = type;
        LineNumber = lineNumber;
        Message = message.UnixFormat();
        StackTrace = stackTrace;
    }

    public TestReport(TestFailedException e)
    {
        Type = ReportType.FAILURE;
        LineNumber = e.LineNumber;
        Message = e.Message;
        StackTrace = e.StackTrace;
    }

    public ReportType Type { get; }

    public int LineNumber { get; set; } = -1;

    public string Message { get; }

    public string? StackTrace { get; set; }

    private static IEnumerable<ReportType> ErrorTypes => new[] { ReportType.TERMINATED, ReportType.INTERRUPTED, ReportType.ABORT };

    public bool IsError => ErrorTypes.Contains(Type);

    public bool IsFailure => Type == ReportType.FAILURE;

    public bool IsWarning => Type == ReportType.WARN;

    public bool Equals(TestReport? other)
        => other is not null
           && Type == other.Type
           && LineNumber == other.LineNumber
           && Message == other.Message
           && IsError == other.IsError
           && IsFailure == other.IsFailure
           && IsWarning == other.IsWarning;

    public override string ToString() => $"[color=green]line [/color][color=aqua]{LineNumber}:[/color]\n {Message}";

    public IDictionary<string, object> Serialize()
        => new Dictionary<string, object>
        {
            { "type", (int)Type },
            { "line_number", LineNumber },
            { "message", Message }
        };

    public TestReport Deserialize(IDictionary<string, object> serialized)
    {
        var type = (ReportType)Enum.Parse(typeof(ReportType), (string)serialized["type"]);
        var lineNumber = (int)serialized["line_number"];
        var message = (string)serialized["message"];
        return new TestReport(type, lineNumber, message);
    }

    public override bool Equals(object? obj) => obj is TestReport other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Type, LineNumber, Message, IsError, IsFailure, IsWarning);

    public static bool operator ==(TestReport lhs, TestReport rhs) => lhs.Equals(rhs);

    public static bool operator !=(TestReport lhs, TestReport rhs) => !(lhs == rhs);
}
