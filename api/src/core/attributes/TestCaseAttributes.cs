// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class TestCaseAttribute : TestStageAttribute
{
    public TestCaseAttribute(params object?[] args) : base("", -1)
        => Arguments = args;

    /// <summary>
    ///     Sets the starting point of random values by given seed.
    /// </summary>
    public double Seed { get; set; } = 1;

    /// <summary>
    ///     Sets the number of test iterations for a parameterized test
    /// </summary>
    public int Iterations { get; set; } = 1;

    /// <summary>
    ///     Holds the test case argument when is specified
    /// </summary>
    internal object?[] Arguments { get; private set; }

    /// <summary>
    ///     Optional test case name to override the original test case name
    /// </summary>
    public string? TestName { get; set; }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BeforeTestAttribute : TestStageAttribute
{
    public BeforeTestAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AfterTestAttribute : TestStageAttribute
{
    public AfterTestAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class IgnoreUntilAttribute : TestStageAttribute
{
    public IgnoreUntilAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}
