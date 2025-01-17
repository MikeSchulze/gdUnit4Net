namespace GdUnit4.Core.Discovery;

/// <summary>
///     Value type representing source code navigation information for a test method.
/// </summary>
public readonly struct CodeNavigation
{
    /// <summary>
    ///     The method this navigation data refers to.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    ///     The line number in the source file where the method is defined.
    /// </summary>
    public required int LineNumber { get; init; }

    /// <summary>
    ///     The source code file path containing the method.
    /// </summary>
    public required string? CodeFilePath { get; init; }

    /// <summary>
    ///     Indicates if this navigation data contains valid source information.
    /// </summary>
    public readonly bool IsValid => CodeFilePath != null;

    public override string ToString()
        => $"""
            CodeNavigation:
              Name: '{MethodName}'
              Line: {LineNumber}
              CodeFilePath: '{CodeFilePath}';
            """;
}
