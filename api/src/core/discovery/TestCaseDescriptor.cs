namespace GdUnit4.Core.Discovery;

using System;

public sealed class TestCaseDescriptor : ITestCase
{
    public required string SimpleName { get; init; }
    public required string FullyQualifiedName { get; init; }
    public required string AssemblyPath { get; init; }
    public required string ManagedType { get; init; }
    public required string ManagedMethod { get; init; }
    public required Guid Id { get; init; }
}
