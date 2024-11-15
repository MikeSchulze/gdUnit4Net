// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;
using System.Diagnostics;

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
    private readonly string? expectedFileName;
    private readonly int? expectedLineNumber;
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
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type
    ///     and expected exception message.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    /// <param name="expectedMessage">The expected message of the thrown exception</param>
    /// <param name="expectedLineNumber">The expected line of exception is thrown</param>
    internal ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage, int expectedLineNumber)
    {
        this.expectedExceptionType = expectedExceptionType;
        this.expectedMessage = expectedMessage;
        this.expectedLineNumber = expectedLineNumber;
    }

    /// <summary>
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type
    ///     and expected exception message.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    /// <param name="expectedMessage">The expected message of the thrown exception</param>
    /// <param name="expectedFileName">The expected file of exception is thrown</param>
    /// <param name="expectedLineNumber">The expected line of exception is thrown</param>
    internal ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage, string expectedFileName, int expectedLineNumber)
    {
        this.expectedExceptionType = expectedExceptionType;
        this.expectedMessage = expectedMessage;
        this.expectedFileName = expectedFileName;
        this.expectedLineNumber = expectedLineNumber;
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

        // Early return if we don't need to check file/line info
        if (expectedLineNumber == null && expectedFileName == null)
            return true;

        var frameInfo = ExtractFileLineInfo(exception);
        if (expectedFileName != null)
        {
            var normalizedExpectedPath = expectedFileName.Replace('\\', '/');
            var normalizedActualPath = frameInfo.fileName?.Replace('\\', '/') ?? string.Empty;

            // Check if actual path contains expected path (to handle relative paths)
            if (!normalizedActualPath.Contains(normalizedExpectedPath, StringComparison.OrdinalIgnoreCase))
                throw new TestFailedException($"Expecting exception at file\n\t{expectedFileName}\n but was at\n\t{frameInfo.fileName}");
        }

        if (expectedLineNumber != null && frameInfo.lineNumber != expectedLineNumber)
            throw new TestFailedException($"Expecting exception at line\n\t{expectedLineNumber}\n but was at\n\t{frameInfo.lineNumber}");
        return true;
    }

    private (string? fileName, int lineNumber) ExtractFileLineInfo(Exception exception)
    {
        if (exception is TestFailedException testFailedException) return (testFailedException.FileName, testFailedException.LineNumber);

        var stackTrace = new StackTrace(exception, true);
        foreach (var frame in stackTrace.GetFrames())
        {
            var fileName = frame.GetFileName();
            if (string.IsNullOrEmpty(fileName))
                continue;

            // Skip framework methods
            var nameSpace = frame.GetMethod()?.DeclaringType?.Namespace;
            if (nameSpace?.StartsWith("System") == true || nameSpace?.StartsWith("Microsoft") == true)
                continue;

            return (fileName, frame.GetFileLineNumber());
        }

        return (null, -1);
    }

    public void ThrowExpectingExceptionExpected()
    {
        var message = $"Expecting exception is thrown:\n\t{expectedExceptionType}\n\tmessage: '{expectedMessage}'\n\tin";
        if (expectedFileName != null)
            message += $" {expectedFileName}";
        if (expectedLineNumber != null)
            message += $":line {expectedLineNumber}";

        throw new TestFailedException(message);
    }
}
