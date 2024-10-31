// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;

using Core.Execution.Exceptions;
using Core.Extensions;

/// <summary>
///     Specifies that a test method is expected to throw an exception to the given type.
///     The test will only pass if the expected exception is thrown and optionally matches
///     the specified exception message.
/// </summary>
/// <remarks>
///     This attribute can be used to verify that a test method correctly throws
///     expected exceptions under specific conditions.
///     Example usage:
///     <code>
/// [TestCase]
/// [ThrowsException(typeof(ArgumentNullException))]
/// public void TestExpectedNullException()
/// {
///     string? value = null;
///     value.Length; // This will throw ArgumentNullException
/// }
///
/// [TestCase]
/// [ThrowsException(typeof(ArgumentException), "Invalid argument")]
/// public void TestExpectedExceptionWithMessage()
/// {
///     throw new ArgumentException("Invalid argument");
/// }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class ThrowsExceptionAttribute : Attribute
{
    private readonly Type expectedExceptionType;
    private readonly string? expectedMessage;

    /// <summary>
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    internal ThrowsExceptionAttribute(Type expectedExceptionType) => this.expectedExceptionType = expectedExceptionType;

    /// <summary>
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type
    ///     and expected exception message.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    /// <param name="expectedMessage">The expected message of the thrown exception</param>
    internal ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage)
    {
        this.expectedExceptionType = expectedExceptionType;
        this.expectedMessage = expectedMessage;
    }

    /// <summary>
    ///     Verifies that the thrown exception matches the expected type and message (if specified).
    /// </summary>
    /// <param name="exception">The exception that was thrown during test execution</param>
    /// <returns>true if the verification passes</returns>
    /// <exception cref="TestFailedException">
    ///     Thrown when:
    ///     - The exception type does not match the expected type
    ///     - The exception message does not match the expected message (if a message was specified)
    /// </exception>
    public bool Verify(Exception exception)
    {
        if (exception.GetType() != expectedExceptionType)
            throw new TestFailedException($"Expecting exception type\n\t{expectedExceptionType}\n but is\n\t{exception.GetType()}.");
        var exceptionMessage = exception.Message.RichTextNormalize();
        if (expectedMessage != null && exceptionMessage != expectedMessage)
            throw new TestFailedException($"Expecting exception message\n\t\"{expectedMessage}\"\n but is\n\t\"{exceptionMessage}\"");

        return true;
    }
}
