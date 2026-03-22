// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

using System.Collections.ObjectModel;
using System.Reflection;

/// <summary>
/// Provides context information to test extensions during their lifecycle callbacks.
/// </summary>
public interface IExtensionContext
{
    /// <summary>Gets the test suite class being executed.</summary>
    /// <returns>The test suite class being executed.</returns>
    Type GetTestSuiteType();

    /// <summary>Gets the live instance of the test suite.</summary>
    /// <returns>The instance of the test suite bing executed.</returns>
    TestSuiteNode GetTestSuiteInstance();

    /// <summary>Gets the test method currently executing (null during suite-level callbacks).</summary>
    /// <returns>The test method current executing, or null during suite-level callbacks.</returns>
    MethodInfo? GetTestMethod();

    /// <summary>Gets display name of the current test case, or null at suite level.</summary>
    /// <returns>Gets the display name of the current est case, or null during suite-level callbacks.</returns>
    string? GetTestCaseName();

    /// <summary>
    /// Gets the raw arguments passed to [TestCase(...)].
    /// Extensions read these in BeforeEach to configure themselves per test.
    /// Arguments not consumed by parameter resolvers are available here.
    /// </summary>
    /// <returns>Arguments to the test case method that have not been consumed by parameter resolvers.</returns>
    ReadOnlyCollection<object?> GetTestCaseArguments();

    /// <summary>
    /// Get the parameter store for this extension context, which allows extensions to store and retrieve arbitrary
    /// values within the context of test execution. This is useful for sharing state between different lifecycle
    /// callbacks of the same extension, or for storing values that should be accessible to parameter resolvers.
    /// </summary>
    /// <returns>The <see cref="IParameterStore"/> containing data for this and any parent contexts.</returns>
    IParameterStore GetStore();
}
