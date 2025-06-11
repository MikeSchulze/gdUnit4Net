// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.Execution.Exceptions;

using System.Text.RegularExpressions;

/// <summary>
///     Provides pattern matching for Godot push_error stack trace format.
/// </summary>
/// <remarks>
///     This class contains regex patterns to parse Godot's error output format,
///     specifically the stack trace information produced by push_error() calls.
///     The expected format is: "at: MethodName() in file.cs:lineNumber".
/// </remarks>
internal static partial class GodotPushErrorPattern
{
    /// <summary>
    ///     Matches a Godot push_error stack trace line against the expected format.
    /// </summary>
    /// <param name="value">The stack trace line to match.</param>
    /// <returns>
    ///     A <see cref="Match" /> object containing:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Groups[1]: Method name and signature</description>
    ///         </item>
    ///         <item>
    ///             <description>Groups[2]: File path</description>
    ///         </item>
    ///         <item>
    ///             <description>Groups[3]: Line number</description>
    ///         </item>
    ///     </list>
    /// </returns>
    public static Match Match(string value) => PushErrorFileInfoRegex().Match(value);

    /// <summary>
    ///     Generated regex pattern to match Godot push_error stack trace format.
    ///     Pattern: "at: (method_info) in (file_path):(line_number)"
    /// </summary>
    /// <returns>A compiled regex for parsing Godot error stack traces.</returns>
    [GeneratedRegex(@"at: (.*) \((.*\.cs):(\d+)\)$", RegexOptions.Compiled)]
    private static partial Regex PushErrorFileInfoRegex();
}
