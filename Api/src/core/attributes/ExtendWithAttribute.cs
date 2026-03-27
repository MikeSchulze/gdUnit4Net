// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
// Need to be placed in the root namespace to be accessible by the test runner.
namespace GdUnit4;

using Api;

/// <summary>
///     Extends the test suite with the specified test extension, hooking it into the test lifecycle
///     and enabling its callbacks and parameter injection for the annotated class.
///     Note that extensions registered using this attribute <i>must</i> have a no-argument constructor or the test
///     framework will be unable to instantiate them.
///     Usage:
///     <code>
/// [TestSuite]
/// [ExtendWith&lt;SomeExtension&gt;]
/// public class SomeTestSuite
/// {
///   [TestCase]
///   public void SomeTestCase()
///   {
///   }
/// }
/// </code>
/// </summary>
/// <typeparam name="T">The type of the extension to register.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ExtendWithAttribute<T> : ExtendWithBaseAttribute
    where T : ITestExtension, new()
{
    /// <inheritdoc />
    public override ITestExtension CreateExtension() => new T();
}
