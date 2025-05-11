// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System;

/// <summary>
///     An Assertion tool to verify Godot.Vector values.
/// </summary>
public interface IVectorAssert<in TValue> : IAssertBase<TValue>
    where TValue : IEquatable<TValue>
{
    /// <summary>
    ///     Verifies that the current value is equal to expected one.
    /// </summary>
    /// <param name="expected">The value to be equal.</param>
    /// <returns>IVectorAssert.</returns>
    public new IVectorAssert<TValue> IsEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is not equal to expected one.
    /// </summary>
    /// <param name="expected">The value to be not equal.</param>
    /// <returns>IVectorAssert.</returns>
    public new IVectorAssert<TValue> IsNotEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current and expected value are approximately equal.
    /// </summary>
    /// <param name="expected">The value to be equal.</param>
    /// <param name="approx">The approximate value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsEqualApprox(TValue expected, TValue approx);

    /// <summary>
    ///     Verifies that the current value is less than the given one.
    /// </summary>
    /// <param name="expected">The less value. </param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsLess(TValue expected);

    /// <summary>
    ///     Verifies that the current value is less than or equal the given one.
    /// </summary>
    /// <param name="expected">The less value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsLessEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is greater than the given one.
    /// </summary>
    /// <param name="expected">The greater value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsGreater(TValue expected);

    /// <summary>
    ///     Verifies that the current value is greater than or equal the given one.
    /// </summary>
    /// <param name="expected">The greater value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsGreaterEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is between the given boundaries (inclusive).
    /// </summary>
    /// <param name="min">The minimal value.</param>
    /// <param name="max">The maximal value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsBetween(TValue min, TValue max);

    /// <summary>
    ///     Verifies that the current value is not between the given boundaries (inclusive).
    /// </summary>
    /// <param name="min">The minimal value.</param>
    /// <param name="max">The maximal value.</param>
    /// <returns>IVectorAssert.</returns>
    public IVectorAssert<TValue> IsNotBetween(TValue min, TValue max);

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">The message to replace the default message.</param>
    /// <returns>IVectorAssert.</returns>
    public new IVectorAssert<TValue> OverrideFailureMessage(string message);
}
