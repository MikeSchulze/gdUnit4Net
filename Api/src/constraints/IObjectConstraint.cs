// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Constraints;

using Asserts;

/// <summary>
///     A set of constrains to verify object values.
/// </summary>
public interface IObjectConstraint : IAssertBase<object, IObjectConstraint>
{
    /// <summary>
    ///     Verifies that the current value is the same as the given one.
    /// </summary>
    /// <param name="expected">The object to be the same.</param>
    /// <returns>IObjectAssert.</returns>
    IObjectConstraint IsSame(object expected);

    /// <summary>
    ///     Verifies that the current value is not the same as the given one.
    /// </summary>
    /// <param name="expected">The object to be NOT the same.</param>
    /// <returns>IObjectAssert.</returns>
    IObjectConstraint IsNotSame(object expected);

    /// <summary>
    ///     Verifies that the current value is an instance of the given type.
    /// </summary>
    /// <typeparam name="TExpectedType">The type of instance to be expected.</typeparam>
    /// <returns>IObjectAssert.</returns>
    IObjectConstraint IsInstanceOf<TExpectedType>();

    /// <summary>
    ///     Verifies that the current value is not an instance of the given type.
    /// </summary>
    /// <typeparam name="TExpectedType">The type of instance to be NOT expected.</typeparam>
    /// <returns>IObjectAssert.</returns>
    IObjectConstraint IsNotInstanceOf<TExpectedType>();
}
