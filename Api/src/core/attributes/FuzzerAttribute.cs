// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FuzzerAttribute : Attribute, IValueProvider
{
    private int value;

    public FuzzerAttribute(int value) => this.value = value;

    public IEnumerable<object> GetValues()
    {
        value += 1;
        yield return value;
    }
}
