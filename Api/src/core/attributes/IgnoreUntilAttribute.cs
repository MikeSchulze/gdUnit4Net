// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
// Need to be placed in the root namespace to be accessible by the test runner.
namespace GdUnit4;

using System;
using System.Runtime.CompilerServices;

/// <summary>
///     Attribute used to temporarily ignore tests until a certain condition is met or until further development.
///     Tests marked with this attribute will be skipped during test execution.
/// </summary>
/// <remarks>
///     This attribute is useful for tests that are temporarily failing due to known issues
///     or tests that depend on features which haven't been implemented yet.
/// </remarks>
/// <example>
///     <code>
/// [Test]
/// [IgnoreUntil]
/// public void TestFeatureNotImplementedYet()
/// {
///     // Test code that will be skipped
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class IgnoreUntilAttribute : TestStageAttribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="IgnoreUntilAttribute" /> class.
    /// </summary>
    /// <param name="line">The line number where the attribute is applied (automatically provided).</param>
    /// <param name="name">The name of the method where the attribute is applied (automatically provided).</param>
#pragma warning disable CA1019
    public IgnoreUntilAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
#pragma warning restore CA1019
        : base(name, line)
    {
    }
}
