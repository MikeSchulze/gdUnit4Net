namespace GdUnit4.Api;

using System.Collections.Generic;

public record TestSuiteNode : TestNode
{
    /// <summary>
    ///     Gets the fully qualified name of the test class type.
    /// </summary>
    public required string ManagedType { get; init; }


    public required List<TestCaseNode> Tests { get; init; } = new();
    public required string AssemblyPath { get; set; }
}
