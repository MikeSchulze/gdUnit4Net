namespace GdUnit4.Asserts;
using System;
using System.Globalization;

internal class VectorAssert<T> : AssertBase<T>, IVectorAssert<T> where T : notnull, IEquatable<T>
{

    public VectorAssert(T current) : base(current)
    {
    }

    public IVectorAssert<T> IsBetween(T min, T max)
    {
        if (CompareTo(Current, min) < 0 || CompareTo(Current, max) > 0)
            ThrowTestFailureReport(AssertFailures.IsBetween(Current, min, max), Current, min);
        return this;
    }

    public new IVectorAssert<T> IsEqual(T expected) => (IVectorAssert<T>)base.IsEqual(expected);

    public IVectorAssert<T> IsEqualApprox(T expected, T approx)
    {
        var minMax = MinMax(expected, approx);
        return IsBetween(minMax.Item1, minMax.Item2);
    }

    public IVectorAssert<T> IsGreater(T expected)
    {
        if (CompareTo(Current, expected) <= 0)
            ThrowTestFailureReport(AssertFailures.IsGreater(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<T> IsGreaterEqual(T expected)
    {
        if (CompareTo(Current, expected) < 0)
            ThrowTestFailureReport(AssertFailures.IsGreaterEqual(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<T> IsLess(T expected)
    {
        if (CompareTo(Current, expected) >= 0)
            ThrowTestFailureReport(AssertFailures.IsLess(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<T> IsLessEqual(T expected)
    {
        if (CompareTo(Current, expected) > 0)
            ThrowTestFailureReport(AssertFailures.IsLessEqual(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<T> IsNotBetween(T from, T to)
    {
        if (CompareTo(Current, from) >= 0 && CompareTo(Current, to) <= 0)
            ThrowTestFailureReport(AssertFailures.IsNotBetween(Current, from, to), Current, from);
        return this;
    }

    public new IVectorAssert<T> IsNotEqual(T expected) => (IVectorAssert<T>)base.IsNotEqual(expected);

    public new IVectorAssert<T> OverrideFailureMessage(string message) => (IVectorAssert<T>)base.OverrideFailureMessage(message);

    private static int CompareTo(T? left, T right)
    {
        if (left == null)
            return -1;
        return (left, right) switch
        {
            (Godot.Vector2 l, Godot.Vector2 r) => l == r ? 0 : l > r ? 1 : -1,
            (Godot.Vector2I l, Godot.Vector2I r) => l == r ? 0 : l > r ? 1 : -1,
            (Godot.Vector3 l, Godot.Vector3 r) => l == r ? 0 : l > r ? 1 : -1,
            (Godot.Vector3I l, Godot.Vector3I r) => l == r ? 0 : l > r ? 1 : -1,
            (Godot.Vector4 l, Godot.Vector4 r) => l == r ? 0 : l > r ? 1 : -1,
            (Godot.Vector4I l, Godot.Vector4I r) => l == r ? 0 : l > r ? 1 : -1,
            _ => 0
        };
    }

#pragma warning disable CS8619
    private static (T, T) MinMax(T left, T right) => (left, right) switch
    {
        (Godot.Vector2 l, Godot.Vector2 r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        (Godot.Vector2I l, Godot.Vector2I r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        (Godot.Vector3 l, Godot.Vector3 r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        (Godot.Vector3I l, Godot.Vector3I r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        (Godot.Vector4 l, Godot.Vector4 r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        (Godot.Vector4I l, Godot.Vector4I r) => ((T)Convert.ChangeType(l - r, typeof(T), CultureInfo.InvariantCulture), (T)Convert.ChangeType(l + r, typeof(T), CultureInfo.InvariantCulture)),
        _ => (default(T), default(T))
    };
#pragma warning restore CS8619

}
