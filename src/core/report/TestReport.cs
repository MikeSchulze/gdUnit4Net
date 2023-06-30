using System;
using System.Collections.Generic;
using System.Linq;

namespace GdUnit4
{
    public sealed class TestReport : IEquatable<object?>
    {
        [Flags]
        public enum TYPE
        {
            SUCCESS,
            WARN,
            FAILURE,
            ORPHAN,
            TERMINATED,
            INTERUPTED,
            ABORT
        }

        public TestReport(TYPE type, int line_number, string message)
        {
            Type = type;
            LineNumber = line_number;
            Message = message.UnixFormat();
        }

        public TYPE Type { get; private set; }

        public int LineNumber { get; private set; }

        public string Message { get; private set; }

        private static IEnumerable<TYPE> ErrorTypes => new[] { TYPE.TERMINATED, TYPE.INTERUPTED, TYPE.ABORT };

        public bool IsError => ErrorTypes.Contains(Type);

        public bool IsFailure => Type == TYPE.FAILURE;

        public bool IsWarning => Type == TYPE.WARN;

        public override string ToString() => $"[color=green]line [/color][color=aqua]{LineNumber}:[/color]\n {Message}";

        public IDictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>(){
             {"type"        ,((int)Type)},
             {"line_number" ,LineNumber},
             {"message"     ,Message}
            };
        }

        public TestReport Deserialize(IDictionary<string, object> serialized)
        {
            TYPE type = (TYPE)Enum.Parse(typeof(TYPE), (string)serialized["type"]);
            int lineNumber = (int)serialized["line_number"];
            string message = (string)serialized["message"];
            return new TestReport(type, lineNumber, message);
        }

        public override bool Equals(object? other) => other is TestReport report
            && Type == report.Type
            && LineNumber == report.LineNumber
            && Message == report.Message
            && IsError == report.IsError
            && IsFailure == report.IsFailure
            && IsWarning == report.IsWarning;


        public override int GetHashCode() =>
            HashCode.Combine(Type, LineNumber, Message, IsError, IsFailure, IsWarning);
    }
}
