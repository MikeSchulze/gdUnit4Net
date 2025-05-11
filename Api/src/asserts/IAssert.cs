// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

/// <summary>
///     Main interface of all GdUnit4 asserts.
/// </summary>
public interface IAssert
{
    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message.</param>
    /// <returns>IAssert.</returns>
    public IAssert OverrideFailureMessage(string message);
}

/// <summary>
///     Base interface of all GdUnit asserts.
/// </summary>
public interface IAssertBase<in TValue> : IAssert
{
    /// <summary>
    ///     Verifies that the current value is null.
    /// </summary>
    /// <returns>IAssertBase.</returns>
    public IAssertBase<TValue> IsNull();

    /// <summary>
    ///     Verifies that the current value is not null.
    /// </summary>
    /// <returns>IAssertBase.</returns>
    public IAssertBase<TValue> IsNotNull();

    /// <summary>
    ///     Verifies that the current value is equal to expected one.
    /// </summary>
    /// <param name="expected">The value to be equal.</param>
    /// <returns>IAssertBase.</returns>
    public IAssertBase<TValue> IsEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is not equal to expected one.
    /// </summary>
    /// <param name="expected">The value to be NOT equal.</param>
    /// <returns>IAssertBase.</returns>
    public IAssertBase<TValue> IsNotEqual(TValue expected);
}
