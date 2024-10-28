namespace GdUnit4.Asserts;

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.ExceptionServices;

using Core.Execution.Exceptions;
using Core.Extensions;

public sealed class ExceptionAssert<TException> : IExceptionAssert where TException : Exception
{
    public ExceptionAssert(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            var capturedException = ExceptionDispatchInfo.Capture(e.InnerException ?? e);
            Current = (TException)capturedException.SourceException;
        }
    }

    public ExceptionAssert(TException e) => Current = e;
    private TException? Current { get; }

    private string? CustomFailureMessage { get; set; }

    public IExceptionAssert IsInstanceOf<TExpectedType>()
    {
        if (Current is not TExpectedType)
            ThrowTestFailureReport(AssertFailures.IsInstanceOf(Current?.GetType(), typeof(TExpectedType)));
        return this;
    }

    public IExceptionAssert HasMessage(string expected)
    {
        expected = expected.UnixFormat();
        var current = Current?.Message.RichTextNormalize() ?? "";
        if (!current.Equals(expected, StringComparison.Ordinal))
            ThrowTestFailureReport(AssertFailures.IsEqual(current, expected));
        return this;
    }

    public IExceptionAssert HasFileLineNumber(int lineNumber)
    {
        int currentLine;
        if (Current is TestFailedException e)
            currentLine = e.LineNumber;
        else
        {
            var stackFrame = new StackTrace(Current!, true).GetFrame(0);
            currentLine = stackFrame?.GetFileLineNumber() ?? -1;
        }

        if (currentLine != lineNumber)
            ThrowTestFailureReport(AssertFailures.IsEqual(currentLine, lineNumber));
        return this;
    }

    public IExceptionAssert HasFileName(string fileName)
    {
        var fullPath = Path.GetFullPath(fileName);
        string currentFileName;
        if (Current is TestFailedException e)
            currentFileName = e.FileName ?? "";
        else
        {
            var stackFrame = new StackTrace(Current!, true).GetFrame(0);
            currentFileName = stackFrame?.GetFileName() ?? "";
        }

        if (!currentFileName.Equals(fullPath, StringComparison.Ordinal))
            ThrowTestFailureReport(AssertFailures.IsEqual(currentFileName, fullPath));
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

    public IExceptionAssert StartsWithMessage(string value)
    {
        value = value.UnixFormat();
        var current = Current?.Message.RichTextNormalize() ?? "";
        if (!current.StartsWith(value, StringComparison.InvariantCulture))
            ThrowTestFailureReport(AssertFailures.IsEqual(current, value));
        return this;
    }

    private void ThrowTestFailureReport(string message)
    {
        var failureMessage = CustomFailureMessage ?? message;
        throw new TestFailedException(failureMessage);
    }

    internal string? GetExceptionStackTrace()
    {
        if (Current is TestFailedException tfe)
            return tfe.StackTrace;
        return Current?.StackTrace ?? null;
    }
}
