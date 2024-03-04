/// A tuple implementation to hold two or many values
namespace GdUnit4.Asserts;

using System;
using System.Collections.Generic;
using System.Linq;

internal sealed class Tuple : ITuple
{
    public Tuple(params object?[] args)
        => Values = args.ToList() ?? new List<object?>();

    public IEnumerable<object?> Values
    { get; set; }

    public override bool Equals(object? obj) => obj is Tuple tuple && Values.VariantEquals(tuple.Values);

    public override int GetHashCode() => HashCode.Combine(Values);

    public override string ToString() => $"tuple({Values.Formatted()})";
}
