// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Parameter)]
public class FuzzerAttribute : Attribute, IValueProvider
{
    private int value;

    public FuzzerAttribute(int value) => this.value = value;

    public IEnumerable<object> GetValues()
    {
        value += 1;
        yield return value;
    }
}
