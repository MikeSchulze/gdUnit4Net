// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
// Need to be placed in the root namespace to be accessible by the test runner.
namespace GdUnit4;

using Api;

/// <summary>
/// Extends the test suite or test method with the specified test extension, hooking it into the test lifecycle
/// and enabling its callbacks and parameter injection for the annotated class or method.
///
/// Note that extensions registered using this attribute <i>must</i> have a no-argument constructor or the test
/// framework will be unable to instantiate them.
///
/// Usage:
///
/// <code>
/// [TestSuite]
/// [ExtendWith&lt;SomeExtension&gt;]
/// public class SomeTestSuite
/// {
///   [TestCase]
///   [ExtendWith&lt;SomeOtherExtension&gt;]
///   public void SomeTestCase()
///   {
///   }
/// }
/// </code>
///
/// As demonstrated, this attribute can be applied to either test cases or suites.
/// </summary>
/// <typeparam name="T">The type of the extension to register.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class ExtendWithAttribute<T> : Attribute
    where T : ITestExtension, new();
