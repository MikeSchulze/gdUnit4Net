namespace GdUnit4.Api;

/// <summary>
///     Represents a test case in the GdUnit4 testing framework.
///     Provides essential information about a test's location and identity.
/// </summary>
public record TestCaseNode : TestNode
{
    /// <summary>
    ///     Gets the name of the test method.
    /// </summary>
    public required string ManagedMethod { get; init; }

    public required int LineNumber { get; init; }
    public required int AttributeIndex { get; init; }

    public required bool RequireRunningGodotEngine { get; init; }
}
