// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

/// <summary>
///     An Assertion Tool to verify exceptions.
/// </summary>
public interface IExceptionAssert : IAssert
{
    /// <summary>
    ///     Verifies the exception message is equal to expected one.
    /// </summary>
    /// <param name="expected">The expected exception message.</param>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert HasMessage(string expected);

    /// <summary>
    ///     Verifies that the exception message starts with the given value.
    /// </summary>
    /// <param name="value">A string with which the exception message must begin.</param>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert StartsWithMessage(string value);

    /// <summary>
    ///     Verifies that the exception message starts with the given value.
    /// </summary>
    /// <typeparam name="TExpectedType">The exception Type.</typeparam>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert IsInstanceOf<TExpectedType>();

    /// <summary>
    ///     Verifies the exception is thrown at expected file line number.
    /// </summary>
    /// <param name="lineNumber">The line number the exception is thrown.</param>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert HasFileLineNumber(int lineNumber);

    /// <summary>
    ///     Verifies the exception is thrown in expected file name.
    /// </summary>
    /// <param name="fileName">The file name where the exception is thrown.</param>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert HasFileName(string fileName);

    /// <summary>
    ///     Verifies that the exception has the expected property value.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="expected">The expected value of the property.</param>
    /// <returns>IExceptionAssert.</returns>
    public IExceptionAssert HasPropertyValue(string propertyName, object expected);
}
