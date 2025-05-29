// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.Execution.Monitoring;

using System.Text.RegularExpressions;

internal static partial class GodotExceptionPattern
{
    public static Match DebugMode(string value)
        => ExceptionPatternDebug().Match(value);

    public static Match ReleaseMode(string value)
        => ExceptionPatternRelease().Match(value);

    [GeneratedRegex(@"'([\w\.]+Exception):\s*(.*)'", RegexOptions.Compiled)]
    private static partial Regex ExceptionPatternDebug();

    [GeneratedRegex(@"([\w\.]+Exception):\s*(.*?)(?:\r\n|\n|$)", RegexOptions.Compiled)]
    private static partial Regex ExceptionPatternRelease();
}
