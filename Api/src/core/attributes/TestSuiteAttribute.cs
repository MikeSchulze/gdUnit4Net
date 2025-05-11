// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Class)]
public sealed class TestSuiteAttribute : TestStageAttribute
{
    public TestSuiteAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class BeforeAttribute : TestStageAttribute
{
    public BeforeAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class AfterAttribute : TestStageAttribute
{
    public AfterAttribute([CallerLineNumber] int line = 0, [CallerMemberName] string name = "")
        : base(name, line)
    {
    }
}
