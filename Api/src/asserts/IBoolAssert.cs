// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

/// <summary>
///     An Assertion Tool to verify boolean values.
/// </summary>
public interface IBoolAssert : IAssertBase<bool>
{
    /// <summary>
    ///     Verifies that the current value is true.
    /// </summary>
    /// <returns>IBoolAssert.</returns>
    IBoolAssert IsTrue();

    /// <summary>
    ///     Verifies that the current value is false.
    /// </summary>
    /// <returns>IBoolAssert.</returns>
    IBoolAssert IsFalse();

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message.</param>
    /// <returns>IBoolAssert.</returns>
    new IBoolAssert OverrideFailureMessage(string message);
}
