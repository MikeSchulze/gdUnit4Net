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

    public override string ToString()
        => $"tuple({string.Join(", ", Values.Cast<object>().Select(GdUnitExtensions.Formatted)).Indentation(0)})";

    public static bool operator ==(Tuple? tuple1, Tuple? tuple2)
    {
        if (ReferenceEquals(tuple1, tuple2))
            return true;
        if (tuple1 is null || tuple2 is null)
            return false;
        return tuple1.Values.VariantEquals(tuple2.Values);
    }

    public static bool operator !=(Tuple? tuple1, Tuple? tuple2) => !(tuple1 == tuple2);
}
