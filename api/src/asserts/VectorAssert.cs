namespace GdUnit4.Asserts;

using System;
using System.Globalization;

using Godot;

public class VectorAssert<TValue> : AssertBase<TValue>, IVectorAssert<TValue> where TValue : IEquatable<TValue>
{
    public VectorAssert(TValue current) : base(current)
    {
    }

    public IVectorAssert<TValue> IsBetween(TValue min, TValue max)
    {
        if (CompareTo(Current, min) < 0 || CompareTo(Current, max) > 0)
            ThrowTestFailureReport(AssertFailures.IsBetween(Current, min, max), Current, min);
        return this;
    }

    public new IVectorAssert<TValue> IsEqual(TValue expected) => (IVectorAssert<TValue>)base.IsEqual(expected);

    public IVectorAssert<TValue> IsEqualApprox(TValue expected, TValue approx)
    {
        var minMax = MinMax(expected, approx);
        return IsBetween(minMax.Item1, minMax.Item2);
    }

    public IVectorAssert<TValue> IsGreater(TValue expected)
    {
        if (CompareTo(Current, expected) <= 0)
            ThrowTestFailureReport(AssertFailures.IsGreater(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<TValue> IsGreaterEqual(TValue expected)
    {
        if (CompareTo(Current, expected) < 0)
            ThrowTestFailureReport(AssertFailures.IsGreaterEqual(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<TValue> IsLess(TValue expected)
    {
        if (CompareTo(Current, expected) >= 0)
            ThrowTestFailureReport(AssertFailures.IsLess(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<TValue> IsLessEqual(TValue expected)
    {
        if (CompareTo(Current, expected) > 0)
            ThrowTestFailureReport(AssertFailures.IsLessEqual(Current!, expected), Current, expected);
        return this;
    }

    public IVectorAssert<TValue> IsNotBetween(TValue min, TValue max)
    {
        if (CompareTo(Current, min) >= 0 && CompareTo(Current, max) <= 0)
            ThrowTestFailureReport(AssertFailures.IsNotBetween(Current, min, max), Current, min);
        return this;
    }

    public new IVectorAssert<TValue> IsNotEqual(TValue expected) => (IVectorAssert<TValue>)base.IsNotEqual(expected);

    public new IVectorAssert<TValue> OverrideFailureMessage(string message) => (IVectorAssert<TValue>)base.OverrideFailureMessage(message);

    private static int CompareTo(TValue? left, TValue right)
    {
        if (left == null)
            return -1;
        return (left, right) switch
        {
            (Vector2 l, Vector2 r) => l == r ? 0 : l > r ? 1 : -1,
            (Vector2I l, Vector2I r) => l == r ? 0 : l > r ? 1 : -1,
            (Vector3 l, Vector3 r) => l == r ? 0 : l > r ? 1 : -1,
            (Vector3I l, Vector3I r) => l == r ? 0 : l > r ? 1 : -1,
            (Vector4 l, Vector4 r) => l == r ? 0 : l > r ? 1 : -1,
            (Vector4I l, Vector4I r) => l == r ? 0 : l > r ? 1 : -1,
            _ => 0
        };
    }

#pragma warning disable CS8619
    private static (TValue, TValue) MinMax(TValue left, TValue right) => (left, right) switch
    {
        (Vector2 l, Vector2 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (Vector2I l, Vector2I r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (Vector3 l, Vector3 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (Vector3I l, Vector3I r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (Vector4 l, Vector4 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (Vector4I l, Vector4I r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        _ => (default, default)
    };
#pragma warning restore CS8619
}
