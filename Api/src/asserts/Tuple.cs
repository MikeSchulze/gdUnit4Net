// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using Core.Extensions;

// A tuple implementation to hold two or many values
internal sealed class Tuple : ITuple
{
    private readonly IEnumerable<object?> values;

    public Tuple(params object?[] args) => values = [.. args];

    public IEnumerable<object?> Values
    {
        get => values;
        set => throw new NotImplementedException();
    }

    public static bool operator ==(Tuple? tuple1, Tuple? tuple2)
    {
        if (ReferenceEquals(tuple1, tuple2))
            return true;
        if (tuple1 is null || tuple2 is null)
            return false;
        return tuple1.Values.VariantEquals(tuple2.Values);
    }

    public static bool operator !=(Tuple? tuple1, Tuple? tuple2) => !(tuple1 == tuple2);

    public override bool Equals(object? obj) => obj is Tuple tuple && Values.VariantEquals(tuple.Values);

    public override int GetHashCode() => HashCode.Combine(values);

    public override string ToString() => $"tuple({string.Join(", ", Values.Select(GdUnitExtensions.Formatted)).Indentation(0)})";
}
