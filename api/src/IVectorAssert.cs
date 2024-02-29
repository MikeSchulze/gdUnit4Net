namespace GdUnit4.Asserts;
using System;

/// <summary> An Assertion tool to verify Godot.Vector values </summary>
public interface IVectorAssert<T> : IAssertBase<T> where T : IEquatable<T>
{
    /// <summary>
    /// Verifies that the current value is equal to expected one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public new IVectorAssert<T> IsEqual(T expected);

    /// <summary>
    /// Verifies that the current value is not equal to expected one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public new IVectorAssert<T> IsNotEqual(T expected);

    /// <summary>
    /// Verifies that the current and expected value are approximately equal.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <param name="approx">The approximal value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsEqualApprox(T expected, T approx);

    /// <summary>
    /// Verifies that the current value is less than the given one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsLess(T expected);

    /// <summary>
    /// Verifies that the current value is less than or equal the given one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsLessEqual(T expected);

    /// <summary>
    /// Verifies that the current value is greater than the given one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsGreater(T expected);

    /// <summary>
    /// Verifies that the current value is greater than or equal the given one.
    /// </summary>
    /// <param name="expected">The expected value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsGreaterEqual(T expected);

    /// <summary>
    /// Verifies that the current value is between the given boundaries (inclusive).
    /// </summary>
    /// <param name="min">The minimal value</param>
    /// <param name="max">The maximal value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsBetween(T min, T max);

    /// <summary>
    /// Verifies that the current value is not between the given boundaries (inclusive).
    /// </summary>
    /// <param name="min">The minimal value</param>
    /// <param name="max">The maximal value</param>
    /// <returns>IVectorAssert</returns>
    public IVectorAssert<T> IsNotBetween(T min, T max);

    /// <summary>
    /// Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">The message to replace the default message</param>
    /// <returns>IVectorAssert</returns>
    new IVectorAssert<T> OverrideFailureMessage(string message);
}
