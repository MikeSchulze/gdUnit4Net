// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class)]
public class TestSuiteAttribute : TestStageAttribute
{
    public TestSuiteAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class BeforeAttribute : TestStageAttribute
{
    public BeforeAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AfterAttribute : TestStageAttribute
{
    public AfterAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "") : base(name, line)
    {
    }
}
