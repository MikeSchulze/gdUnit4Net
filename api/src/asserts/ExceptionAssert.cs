namespace GdUnit4.Asserts;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;

using Exceptions;

internal sealed class ExceptionAssert<T> : IExceptionAssert
{
    private Exception? Current { get; set; }

    private string? CustomFailureMessage { get; set; }

    public ExceptionAssert(Action action)
    {
        try
        { action.Invoke(); }
        catch (Exception e)
        {
            var capturedException = ExceptionDispatchInfo.Capture(e.InnerException ?? e);
            Current = capturedException.SourceException;
        }
    }

    public ExceptionAssert(Exception e) => Current = e;

    public IExceptionAssert IsInstanceOf<TExpectedType>()
    {
        if (Current is not TExpectedType)
            ThrowTestFailureReport(AssertFailures.IsInstanceOf(Current?.GetType(), typeof(TExpectedType)));
        return this;
    }

    public IExceptionAssert HasMessage(string message)
    {
        message = message.UnixFormat();
        var current = Current?.Message.RichTextNormalize() ?? "";
        if (!current.Equals(message, StringComparison.Ordinal))
            ThrowTestFailureReport(AssertFailures.IsEqual(current, message));
        return this;
    }

    public IExceptionAssert HasFileLineNumber(int lineNumber)
    {
        var stackFrame = new StackTrace(Current!, true).GetFrame(0);
        var currentLine = stackFrame?.GetFileLineNumber();
        if (currentLine != lineNumber)
            ThrowTestFailureReport(AssertFailures.IsEqual(currentLine, lineNumber));
        return this;
    }

    public IExceptionAssert HasFileName(string fileName)
    {
        var stackFrame = new StackTrace(Current!, true).GetFrame(0);
        var currentFileName = stackFrame?.GetFileName() ?? "";
        var fullPath = Path.GetFullPath(fileName);
        if (!currentFileName.Equals(fullPath, StringComparison.Ordinal))
            ThrowTestFailureReport(AssertFailures.IsEqual(currentFileName ?? "", fullPath));
        return this;
    }

    public IExceptionAssert HasPropertyValue(string propertyName, object expected)
    {
        var value = Current?.GetType().GetProperty(propertyName)?.GetValue(Current);
        if (!Comparable.IsEqual(value, expected).Valid)
            ThrowTestFailureReport(AssertFailures.HasValue(propertyName, value, expected));
        return this;
    }

    public IAssert OverrideFailureMessage(string message)
    {
        CustomFailureMessage = message.UnixFormat();
        return this;
    }

    public IExceptionAssert StartsWithMessage(string message)
    {
        message = message.UnixFormat();
        var current = Current?.Message.RichTextNormalize() ?? "";
        if (!current.StartsWith(message, StringComparison.InvariantCulture))
            ThrowTestFailureReport(AssertFailures.IsEqual(current, message));
        return this;
    }

    private void ThrowTestFailureReport(string message)
    {
        var failureMessage = CustomFailureMessage ?? message;
        throw new TestFailedException(failureMessage);
    }
}
