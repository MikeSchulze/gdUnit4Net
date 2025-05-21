// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

/// <summary>
///     An Assertion Tool to verify string values.
/// </summary>
public interface IStringAssert : IAssertBase<string>
{
    /// <summary>
    ///     Verifies that the current String is equal to the given one, ignoring case considerations.
    /// </summary>
    /// <param name="expected">The value to be equal.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert IsEqualIgnoringCase(string expected);

    /// <summary>
    ///     Verifies that the current String is not equal to the given one, ignoring case considerations.
    /// </summary>
    /// <param name="expected">The value to be NOT equal.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert IsNotEqualIgnoringCase(string expected);

    /// <summary>
    ///     Verifies that the current String is empty; it has a length of 0.
    /// </summary>
    /// <returns>IStringAssert.</returns>
    IStringAssert IsEmpty();

    /// <summary>
    ///     Verifies that the current String is not empty; it has a length of minimum 1.
    /// </summary>
    /// <returns>IStringAssert.</returns>
    IStringAssert IsNotEmpty();

    /// <summary>
    ///     Verifies that the current String contains the given String.
    /// </summary>
    /// <param name="expected">The value to be contained.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert Contains(string expected);

    /// <summary>
    ///     Verifies that the current String does not contain the given String.
    /// </summary>
    /// <param name="expected">The value to be NOT contained.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert NotContains(string expected);

    /// <summary>
    ///     Verifies that the current String does not contain the given String, ignoring case considerations.
    /// </summary>
    /// <param name="expected">The value to be contained.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert ContainsIgnoringCase(string expected);

    /// <summary>
    ///     Verifies that the current String does not contain the given String, ignoring case considerations.
    /// </summary>
    /// <param name="expected">The value to be NOT contained.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert NotContainsIgnoringCase(string expected);

    /// <summary>
    ///     Verifies that the current String starts with the given prefix.
    /// </summary>
    /// <param name="expected">The value to be starts with.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert StartsWith(string expected);

    /// <summary>
    ///     Verifies that the current String ends with the given suffix.
    /// </summary>
    /// <param name="expected">The value to be ends with.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert EndsWith(string expected);

    /// <summary>
    ///     Verifies that the current String has the expected length by used comparator.
    /// </summary>
    /// <param name="length">The lengths to be.</param>
    /// <param name="comparator">They used comparator mode.</param>
    /// <returns>IStringAssert.</returns>
    IStringAssert HasLength(int length, Compare comparator = Compare.EQUAL);

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message.</param>
    /// <returns>IStringAssert.</returns>
    new IStringAssert OverrideFailureMessage(string message);

#pragma warning disable CA1707

    // ReSharper disable InconsistentNaming
#pragma warning disable CS1591, SA1600, SA1400, SA1201
    enum Compare
#pragma warning restore SA1201, SA1400, SA1600
    {
        EQUAL,
        LESS_THAN,
        LESS_EQUAL,
        GREATER_THAN,
        GREATER_EQUAL
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    // ReSharper enable InconsistentNaming
#pragma warning restore CA1707
}
