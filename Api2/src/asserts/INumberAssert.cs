namespace GdUnit4.Asserts;

using System;
using System.Numerics;

/// <summary>
///     Base interface for number assertions.
/// </summary>
public interface INumberAssert<in TValue> : IAssertBase<TValue>
    where TValue : notnull, IComparable, IComparable<TValue>, IEquatable<TValue>,
    IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
{
    /// <summary>
    ///     Verifies that the current value is less than the given one.
    /// </summary>
    /// <param name="expected">The value to be less</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsLess(TValue expected);

    /// <summary>
    ///     Verifies that the current value is less than or equal the given one.
    /// </summary>
    /// <param name="expected">The value to be less or equal</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsLessEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is greater than the given one.
    /// </summary>
    /// <param name="expected">The value to be greater</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsGreater(TValue expected);

    /// <summary>
    ///     Verifies that the current value is greater than or equal the given one.
    /// </summary>
    /// <param name="expected">The value to be greater or equal</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsGreaterEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current and expected value are approximately equal.
    /// </summary>
    /// <param name="expected">The value to be equal</param>
    /// <param name="approx">The approximate</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsEqualApprox(TValue expected, TValue approx);

    /// <summary>
    ///     Verifies that the current value is even.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsEven();

    /// <summary>
    ///     Verifies that the current value is odd.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsOdd();

    /// <summary>
    ///     Verifies that the current value is negative.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsNegative();

    /// <summary>
    ///     Verifies that the current value is not negative.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsNotNegative();

    /// <summary>
    ///     Verifies that the current value is equal to zero.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsZero();

    /// <summary>
    ///     Verifies that the current value is not equal to zero.
    /// </summary>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsNotZero();

    /// <summary>
    ///     Verifies that the current value is in the given set of values.
    /// </summary>
    /// <param name="expected">A possible set of values to be equal</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsIn(params TValue[] expected);

    /// <summary>
    ///     Verifies that the current value is not in the given set of values.
    /// </summary>
    /// <param name="expected">A set of values to be NOT equal</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsNotIn(params TValue[] expected);

    /// <summary>
    ///     Verifies that the current value is between the given boundaries (inclusive).
    /// </summary>
    /// <param name="min">The minimum range value</param>
    /// <param name="max">The maximum range value</param>
    /// <returns>INumberAssert</returns>
    public INumberAssert<TValue> IsBetween(TValue min, TValue max);

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message</param>
    /// <returns>INumberAssert</returns>
    public new INumberAssert<TValue> OverrideFailureMessage(string message);
}
