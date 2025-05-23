// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

/// <summary>
///     The main interface of all GdUnit4 assertions.
///     Provides the core functionality for assertion-based testing in the GdUnit4 framework.
/// </summary>
/// <remarks>
///     This interface defines the base contract that all assertion types must implement,
///     enabling consistent behavior across different types of assertions.
/// </remarks>
public interface IAssert
{
    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message.</param>
    /// <returns>IAssert.</returns>
    IAssert OverrideFailureMessage(string message);
}

/// <summary>
///     The base interface of all GdUnit asserts.
///     Provides common assertion methods that apply to most value types.
/// </summary>
/// <typeparam name="TValue">The type of value being tested by this assertion.</typeparam>
/// <remarks>
///     This generic interface extends the core IAssert interface with type-specific
///     assertion methods for comparing values and checking nullability.
/// </remarks>
public interface IAssertBase<in TValue> : IAssert
{
    /// <summary>
    ///     Verifies that the current value is null.
    /// </summary>
    /// <returns>IAssertBase.</returns>
    IAssertBase<TValue> IsNull();

    /// <summary>
    ///     Verifies that the current value is not null.
    /// </summary>
    /// <returns>IAssertBase.</returns>
    IAssertBase<TValue> IsNotNull();

    /// <summary>
    ///     Verifies that the current value is equal to the expected one.
    /// </summary>
    /// <param name="expected">The value to be equal.</param>
    /// <returns>IAssertBase.</returns>
    IAssertBase<TValue> IsEqual(TValue expected);

    /// <summary>
    ///     Verifies that the current value is not equal to the expected one.
    /// </summary>
    /// <param name="expected">The value to be NOT equal.</param>
    /// <returns>IAssertBase.</returns>
    IAssertBase<TValue> IsNotEqual(TValue expected);
}
