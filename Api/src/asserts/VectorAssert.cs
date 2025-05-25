// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System;
using System.Globalization;

using Core.Extensions;

using Godot;

using SystemVector2 = System.Numerics.Vector2;
using SystemVector3 = System.Numerics.Vector3;
using SystemVector4 = System.Numerics.Vector4;

public class VectorAssert<TValue> : AssertBase<TValue>, IVectorAssert<TValue>
    where TValue : IEquatable<TValue>
{
    public VectorAssert(TValue current)
        : base(current)
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
        var isEqualApproximate = (Current, expected, approx) switch
        {
            (SystemVector2 v, SystemVector2 e, SystemVector2 a) => v.IsEqualApprox(e, a),
            (SystemVector3 v, SystemVector3 e, SystemVector3 a) => v.IsEqualApprox(e, a),
            (SystemVector4 v, SystemVector4 e, SystemVector4 a) => v.IsEqualApprox(e, a),
            (Vector2 v, Vector2 e, Vector2 a) => v.IsEqualApprox(e, a),
            (Vector2I v, Vector2I e, Vector2I a) => v.IsEqualApprox(e, a),
            (Vector3 v, Vector3 e, Vector3 a) => v.IsEqualApprox(e, a),
            (Vector3I v, Vector3I e, Vector3I a) => v.IsEqualApprox(e, a),
            (Vector4 v, Vector4 e, Vector4 a) => v.IsEqualApprox(e, a),
            (Vector4I v, Vector4I e, Vector4I a) => v.IsEqualApprox(e, a),
            _ => false
        };
        if (!isEqualApproximate)
            ThrowTestFailureReport(AssertFailures.IsBetween(Current, minMax.Item1, minMax.Item2), Current, minMax.Item1);

        return this;
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
        (SystemVector2 l, SystemVector2 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (SystemVector3 l, SystemVector3 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
        (SystemVector4 l, SystemVector4 r) => ((TValue)Convert.ChangeType(l - r, typeof(TValue), CultureInfo.InvariantCulture),
            (TValue)Convert.ChangeType(l + r, typeof(TValue), CultureInfo.InvariantCulture)),
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
