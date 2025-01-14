namespace GdUnit4.Core.Execution.Monitoring;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Godot;

internal sealed partial class ErrorLogEntry
{
    public enum ErrorType
    {
        Exception,
        PushError,
        PushWarning
    }

    private static readonly Regex ExceptionPatternDebugMode = ExceptionPatternDebugRegex();
    private static readonly Regex ExceptionPatternReleaseMode = ExceptionPatternReleaseRegex();

    private ErrorLogEntry(ErrorType entryType, string message, string details, Type? exceptionType = null)
    {
        EntryType = entryType;
        Message = message;
        Details = details;
        ExceptionType = exceptionType;
    }

    internal string Details { get; }
    internal string Message { get; }
    internal ErrorType EntryType { get; }
    internal Type? ExceptionType { get; }

    private static bool IsDebuggerActive { get; } = DebuggerUtils.IsDebuggerActive();

    public static ErrorLogEntry? ExtractPushWarning(string[] records, int index) =>
        Extract(records, index, ErrorType.PushWarning);

    public static ErrorLogEntry? ExtractPushError(string[] records, int index) =>
        Extract(records, index, ErrorType.PushError);

    public static ErrorLogEntry? ExtractException(string[] records, int index) =>
        Extract(records, index, ErrorType.Exception);

    private static Type? TryGetExceptionType(string typeName)
    {
        try
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null && typeof(Exception).IsAssignableFrom(type))
                return type;

            return null;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Failed to resolve exception type '{typeName}': {ex.Message}");
            return null;
        }
    }

    private static ErrorLogEntry? Extract(string[] records, int index, ErrorType type)
    {
        if (index >= records.Length)
            return null;

        var pattern = IsDebuggerActive ? LoggerPatterns[type].Item1 : LoggerPatterns[type].Item2;
        var record = records[index];
        if (!record.StartsWith(pattern))
            return null;

        // Remove the pattern and trim whitespace
        var content = record.Replace(pattern, "").Trim();
        // Get the details from the next line if available
        var details = index + 1 < records.Length ? records[index + 1].Trim() : string.Empty;

        // Handle exception type parsing
        if (type == ErrorType.Exception)
        {
            var match = IsDebuggerActive ? ExceptionPatternDebugMode.Match(content) : ExceptionPatternReleaseMode.Match(content);
            if (!match.Success) return null;
            var exceptionTypeName = match.Groups[1].Value;
            var exceptionMessage = match.Groups[2].Value;
            var exceptionType = TryGetExceptionType(exceptionTypeName);
            return new ErrorLogEntry(type, exceptionMessage, details, exceptionType);
        }

        // is PushError we need to scan the stacktrace
        if (type == ErrorType.PushError)
        {
        }

        return new ErrorLogEntry(type, content, details);
    }

    public override string ToString() =>
        EntryType == ErrorType.Exception
            ? $"{EntryType}: [{ExceptionType?.Name ?? "Unknown Exception"}] {Message}\nDetails: {Details}"
            : $"{EntryType}: {Message}\nDetails: {Details}";

    [GeneratedRegex(@"'([\w\.]+Exception):\s*(.*)'", RegexOptions.Compiled)]
    private static partial Regex ExceptionPatternDebugRegex();


    [GeneratedRegex(@"([\w\.]+Exception):\s*(.*?)(?:\r\n|\n|$)", RegexOptions.Compiled)]
    private static partial Regex ExceptionPatternReleaseRegex();

#pragma warning disable IDE0060
    private static bool IsGodot4X4 => (int)Engine.GetVersionInfo()["hex"] >= 0x40400;

    private static readonly string GodotErrorPattern = IsGodot4X4 ? "ERROR:" : "USER ERROR:";

    private static readonly Dictionary<ErrorType, Tuple<string, string>> LoggerPatterns = new()
    {
        { ErrorType.Exception, new Tuple<string, string>("ERROR:", IsGodot4X4 ? "ERROR:" : "USER ERROR:") },
        { ErrorType.PushError, new Tuple<string, string>(GodotErrorPattern, GodotErrorPattern) },
        { ErrorType.PushWarning, new Tuple<string, string>("USER WARNING:", "USER WARNING:") }
    };
#pragma warning restore IDE0060
}
