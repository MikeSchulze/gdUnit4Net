// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Discovery;

/// <summary>
///     Value type representing source code navigation information for a test method.
/// </summary>
public readonly struct CodeNavigation
{
    /// <summary>
    ///     Gets the method this navigation data refers to.
    /// </summary>
    public required string MethodName { get; init; }

    /// <summary>
    ///     Gets the line number in the source file where the method is defined.
    /// </summary>
    public required int LineNumber { get; init; }

    /// <summary>
    ///     Gets the source code file path containing the method.
    /// </summary>
    public required string? CodeFilePath { get; init; }

    /// <summary>
    ///     Gets a value indicating whether indicates if this navigation data contains valid source information.
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
