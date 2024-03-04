namespace GdUnit4.Asserts;

using System;

/// <summary> Base interface for number assertions.</summary>
public interface INumberAssert<TValue> : IAssertBase<TValue> where TValue : IComparable, IComparable<TValue>
{
    /// <summary> Verifies that the current value is less than the given one.</summary>
    public INumberAssert<TValue> IsLess(TValue expected);

    /// <summary> Verifies that the current value is less than or equal the given one.</summary>
    public INumberAssert<TValue> IsLessEqual(TValue expected);

    /// <summary> Verifies that the current value is greater than the given one.</summary>
    public INumberAssert<TValue> IsGreater(TValue expected);

    /// <summary> Verifies that the current value is greater than or equal the given one.</summary>
    public INumberAssert<TValue> IsGreaterEqual(TValue expected);

    /// <summary> Verifies that the current value is even.</summary>
    public INumberAssert<TValue> IsEven();

    /// <summary> Verifies that the current value is odd.</summary>
    public INumberAssert<TValue> IsOdd();

    /// <summary> Verifies that the current value is negative.</summary>
    public INumberAssert<TValue> IsNegative();

    /// <summary> Verifies that the current value is not negative.</summary>
    public INumberAssert<TValue> IsNotNegative();

    /// <summary> Verifies that the current value is equal to zero.</summary>
    public INumberAssert<TValue> IsZero();

    /// <summary> Verifies that the current value is not equal to zero.</summary>
    public INumberAssert<TValue> IsNotZero();

    /// <summary> Verifies that the current value is in the given set of values.</summary>
    public INumberAssert<TValue> IsIn(params TValue[] expected);

    /// <summary> Verifies that the current value is not in the given set of values.</summary>
    public INumberAssert<TValue> IsNotIn(params TValue[] expected);

    /// <summary> Verifies that the current value is between the given boundaries (inclusive).</summary>
    public INumberAssert<TValue> IsBetween(TValue min, TValue max);

    public new INumberAssert<TValue> OverrideFailureMessage(string message);
}
