// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class GodotTestCaseAttribute : TestCaseAttribute
{
    public GodotTestCaseAttribute(params object?[] args) : base("", -1)
        => Arguments = args;
}
