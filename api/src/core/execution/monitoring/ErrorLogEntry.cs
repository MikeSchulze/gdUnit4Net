namespace GdUnit4.Core.Execution.Monitoring;

using System;
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

    private const string PATTERN_SCRIPT_ERROR = "USER SCRIPT ERROR:";
    private const string PATTERN_PUSH_ERROR = "USER ERROR:";
    private const string PATTERN_PUSH_WARNING = "USER WARNING:";

    // With Godot 4.4 the pattern has changed
    private const string PATTERN_4_X4_EXCEPTION = "ERROR:";
    private const string PATTERN_4_X4_PUSH_ERROR = "USER ERROR:";
    private const string PATTERN_4_X4_PUSH_WARNING = "USER WARNING:";
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

    // Cache the Godot version check result
    private static bool IsGodot4X4 { get; } = (int)Engine.GetVersionInfo()["hex"] >= 0x40300;

    private static bool IsDebuggerActive { get; } = DebuggerUtils.IsDebuggerActive();

    public static ErrorLogEntry? ExtractPushWarning(string[] records, int index) =>
        Extract(records, index, ErrorType.PushWarning, IsGodot4X4 ? PATTERN_4_X4_PUSH_WARNING : PATTERN_PUSH_WARNING);

    public static ErrorLogEntry? ExtractPushError(string[] records, int index) =>
        Extract(records, index, ErrorType.PushError, IsGodot4X4 ? PATTERN_4_X4_PUSH_ERROR : PATTERN_PUSH_ERROR);

    public static ErrorLogEntry? ExtractException(string[] records, int index)
    {
        var pattern = IsDebuggerActive ? PATTERN_4_X4_EXCEPTION : PATTERN_4_X4_PUSH_ERROR;

        return Extract(records, index, ErrorType.Exception, pattern);
    }

    private static Type? TryGetExceptionType(string typeName)
    {
        try
        {
            // First try to get the type directly
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

    private static ErrorLogEntry? Extract(string[] records, int index, ErrorType type, string pattern)
    {
        if (index >= records.Length)
            return null;

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
            if (match.Success)
            {
                var exceptionTypeName = match.Groups[1].Value;
                var exceptionMessage = match.Groups[2].Value;
                var exceptionType = TryGetExceptionType(exceptionTypeName);
                return new ErrorLogEntry(type, exceptionMessage, details, exceptionType);
            }
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
}
