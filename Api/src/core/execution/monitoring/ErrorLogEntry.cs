// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution.Monitoring;

using Godot;

internal sealed class ErrorLogEntry
{
    private static readonly string GodotErrorPattern = IsGodot4X4 ? "ERROR:" : "USER ERROR:";

    private static readonly Dictionary<ErrorType, Tuple<string, string>> LoggerPatterns = new()
    {
        { ErrorType.Exception, new Tuple<string, string>("ERROR:", IsGodot4X4 ? "ERROR:" : "USER ERROR:") },
        { ErrorType.PushError, new Tuple<string, string>(GodotErrorPattern, GodotErrorPattern) },
        { ErrorType.PushWarning, new Tuple<string, string>("USER WARNING:", "USER WARNING:") }
    };

    private ErrorLogEntry(ErrorType entryType, string message, string details, Type? exceptionType = null)
    {
        EntryType = entryType;
        Message = message;
        Details = details;
        ExceptionType = exceptionType;
    }

    internal enum ErrorType
    {
        Exception,
        PushError,
        PushWarning
    }

    internal string Details { get; }

    internal string Message { get; }

    internal ErrorType EntryType { get; }

    internal Type? ExceptionType { get; }

    private static bool IsDebuggerActive { get; } = DebuggerUtils.IsDebuggerActive();

#pragma warning disable IDE0060

    private static bool IsGodot4X4 => (int)Engine.GetVersionInfo()["hex"] >= 0x40400;

#pragma warning restore IDE0060

    public static ErrorLogEntry? ExtractPushWarning(string[] records, int index) =>
        Extract(records, index, ErrorType.PushWarning);

    public static ErrorLogEntry? ExtractPushError(string[] records, int index) =>
        Extract(records, index, ErrorType.PushError);

    public static ErrorLogEntry? ExtractException(string[] records, int index) =>
        Extract(records, index, ErrorType.Exception);

    public override string ToString() =>
        EntryType == ErrorType.Exception
            ? $"{EntryType}: [{ExceptionType?.Name ?? "Unknown Exception"}] {Message}\nDetails: {Details}"
            : $"{EntryType}: {Message}\nDetails: {Details}";

    private static Type? TryGetExceptionType(string typeName)
    {
        try
        {
            var type = Type.GetType(typeName, false, true);
            if (type != null && typeof(Exception).IsAssignableFrom(type))
                return type;

            return null;
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
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
        var content = record.Replace(pattern, string.Empty, StringComparison.Ordinal)
            .Trim()

            // On Godot version before 4.5 the exception is covered by single quotes, we need to remove it to match!
            .TrimStart('\'')
            .TrimEnd('\'');

        // Get the details from the next line if available
        var details = index + 1 < records.Length ? records[index + 1].Trim() : string.Empty;

        // Handle exception type parsing
        if (type == ErrorType.Exception)
        {
            var match = IsDebuggerActive ? GodotExceptionPattern.DebugMode(content) : GodotExceptionPattern.ReleaseMode(content);
            if (!match.Success)
                return null;
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
}
