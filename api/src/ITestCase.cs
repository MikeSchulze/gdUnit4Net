namespace GdUnit4;

using System;

/// <summary>
///     Represents a test case in the GdUnit4 testing framework.
///     Provides essential information about a test's location and identity.
/// </summary>
public interface ITestCase
{
    /// <summary>
    ///     Gets the file path to the assembly containing this test case.
    /// </summary>
    public string AssemblyPath { get; init; }

    /// <summary>
    ///     Gets the fully qualified name of the test class type.
    /// </summary>
    public string ManagedType { get; init; }

    /// <summary>
    ///     Gets the name of the test method.
    /// </summary>
    public string ManagedMethod { get; init; }

    /// <summary>
    ///     Gets the unique identifier for this test case.
    /// </summary>
    Guid Id { get; init; }
}
