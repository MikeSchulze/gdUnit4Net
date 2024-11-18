﻿// ReSharper disable once CheckNamespace

namespace GdUnit4;

using System;
using System.Diagnostics;

using Core.Execution.Exceptions;
using Core.Extensions;

/// <summary>
///     Specifies that a test method is expected to throw an exception of the given type. The test will pass only if:
///     <list type="bullet">
///         <li>The expected exception type is thrown</li>
///         <li>The exception message matches (if specified)</li>
///         <li>The exception occurs at the expected file and line (if specified)</li>
///     </list>
/// </summary>
/// <remarks>
///     This attribute can be used in several ways:
///     <list type="bullet">
///         <li>Basic exception type verification</li>
///         <li>Exception type and message verification</li>
///         <li>Full exception verification including source location</li>
///     </list>
///     Multiple attributes can be applied to handle different possible exceptions:
///     <code>
/// [TestCase]
/// [ThrowsException(typeof(ArgumentNullException))]
/// [ThrowsException(typeof(InvalidOperationException))]
/// public void TestMethodCanThrowMultipleExceptions()
/// {
///     // Test logic that might throw either exception
/// }
/// </code>
///     The attribute supports Godot-specific exceptions when used with <see cref="GodotExceptionMonitorAttribute" />:
///     <code>
/// [TestCase]
/// [GodotExceptionMonitor]
/// [ThrowsException(typeof(InvalidOperationException), "Node not found", "/scenes/test_scene.cs", 25)]
/// public void TestGodotException()
/// {
///     // Test Godot code that throws during scene processing
/// }
/// </code>
///     Basic usage example:
///     <code>
/// [TestCase]
/// [ThrowsException(typeof(ArgumentNullException))]
/// public void TestNullArgument()
/// {
///     string? text = null;
///     text.Length; // Will throw ArgumentNullException
/// }
/// </code>
///     Message verification example:
///     <code>
/// [TestCase]
/// [ThrowsException(typeof(ArgumentException), "Value cannot be zero")]
/// public void TestSpecificError()
/// {
///     throw new ArgumentException("Value cannot be zero");
/// }
/// </code>
///     Location verification example:
///     <code>
/// [TestCase]
/// [ThrowsException(typeof(InvalidOperationException), "Operation failed", "TestClass.cs", 42)]
/// public void TestExceptionLocation()
/// {
///     throw new InvalidOperationException("Operation failed");
/// }
/// </code>
/// </remarks>
/// <seealso cref="GodotExceptionMonitorAttribute" />
/// <seealso cref="TestCaseAttribute" />
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public class ThrowsExceptionAttribute : GodotExceptionMonitorAttribute
{
    private readonly Type expectedExceptionType;
    private readonly string? expectedFileName;
    private readonly int? expectedLineNumber;
    private readonly string? expectedMessage;

    /// <summary>
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    public ThrowsExceptionAttribute(Type expectedExceptionType) => this.expectedExceptionType = expectedExceptionType;


    /// <summary>
    ///     Initializes a new instance of the ThrowsExceptionAttribute with the specified exception type
    ///     and expected exception message.
    /// </summary>
    /// <param name="expectedExceptionType">The Type of exception that is expected to be thrown</param>
    /// <param name="expectedMessage">The expected message of the thrown exception</param>
    public ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage)
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
    public ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage, int expectedLineNumber)
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
    public ThrowsExceptionAttribute(Type expectedExceptionType, string expectedMessage, string expectedFileName, int expectedLineNumber)
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
    internal bool Verify(Exception exception)
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

    internal void ThrowExpectingExceptionExpected()
    {
        var message = $"Expecting exception is thrown:\n\t{expectedExceptionType}\n\tmessage: '{expectedMessage}'\n\tin";
        if (expectedFileName != null)
            message += $" {expectedFileName}";
        if (expectedLineNumber != null)
            message += $":line {expectedLineNumber}";

        throw new TestFailedException(message);
    }
}
