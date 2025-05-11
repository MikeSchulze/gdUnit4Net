// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TestCaseAttribute : TestStageAttribute
{
    public TestCaseAttribute(params object?[] args)
        : base(string.Empty, -1)
        => Arguments = args;

    /// <summary>
    ///     Gets or sets the starting point of random values by given seed.
    /// </summary>
    public double Seed { get; set; } = 1;

    /// <summary>
    ///     Gets or sets the number of test iterations for a parameterized test.
    /// </summary>
    public int Iterations { get; set; } = 1;

    /// <summary>
    ///     Gets holds the test case argument when is specified.
    /// </summary>
    internal object?[] Arguments { get; init; }

    /// <summary>
    ///     Gets or sets optional test case name to override the original test case name.
    /// </summary>
    public string? TestName { get; set; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BeforeTestAttribute : TestStageAttribute
{
    public BeforeTestAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class AfterTestAttribute : TestStageAttribute
{
    public AfterTestAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class IgnoreUntilAttribute : TestStageAttribute
{
    public IgnoreUntilAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}
