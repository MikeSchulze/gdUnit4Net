namespace GdUnit4;

using System;
using System.Collections.Generic;
using System.Linq;


public sealed class TestReport : IEquatable<TestReport>
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
        ABORT
    }

    public TestReport(ReportType type, int lineNumber, string message)
    {
        Type = type;
        LineNumber = lineNumber;
        Message = message.UnixFormat();
    }

    public ReportType Type { get; private set; }

    public int LineNumber { get; set; }

    public string Message { get; private set; }

    private static IEnumerable<ReportType> ErrorTypes => new[] { ReportType.TERMINATED, ReportType.INTERRUPTED, ReportType.ABORT };

    public bool IsError => ErrorTypes.Contains(Type);

    public bool IsFailure => Type == ReportType.FAILURE;

    public bool IsWarning => Type == ReportType.WARN;

    public override string ToString() => $"[color=green]line [/color][color=aqua]{LineNumber}:[/color]\n {Message}";

    public IDictionary<string, object> Serialize()
        => new Dictionary<string, object>(){
             {"type"        ,(int)Type},
             {"line_number" ,LineNumber},
             {"message"     ,Message}
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

    public bool Equals(TestReport? other)
        => other is not null
        && Type == other.Type
        && LineNumber == other.LineNumber
        && Message == other.Message
        && IsError == other.IsError
        && IsFailure == other.IsFailure
        && IsWarning == other.IsWarning;
}
