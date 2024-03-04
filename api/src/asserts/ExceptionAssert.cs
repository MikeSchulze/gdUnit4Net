namespace GdUnit4.Asserts;
using System;


using Exceptions;

internal sealed class ExceptionAssert<T> : IExceptionAssert
{
    private Exception? Current { get; set; }

    private string? CustomFailureMessage { get; set; }

    public ExceptionAssert(Action action)
    {
        try
        { action.Invoke(); }
        catch (Exception e) { Current = e; }
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
