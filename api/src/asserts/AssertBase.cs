namespace GdUnit4.Asserts;

using Core.Execution.Exceptions;
using Core.Extensions;

public abstract class AssertBase<TValue> : IAssertBase<TValue>
{
    protected AssertBase(TValue? current) => Current = current;
    protected TValue? Current { get; }

    protected string? CustomFailureMessage { get; private set; }

    protected string CurrentFailureMessage { get; set; } = "";

    public IAssertBase<TValue> IsEqual(TValue expected)
    {
        var result = Comparable.IsEqual(Current, expected);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.IsEqual(Current, expected), Current, expected);
        return this;
    }

    public IAssertBase<TValue> IsNotEqual(TValue expected)
    {
        var result = Comparable.IsEqual(Current, expected);
        if (result.Valid)
            ThrowTestFailureReport(AssertFailures.IsNotEqual(Current, expected), Current, expected);
        return this;
    }

    public IAssertBase<TValue> IsNull()
    {
        if (Current != null)
            ThrowTestFailureReport(AssertFailures.IsNull(Current), Current, null);
        return this;
    }

    public IAssertBase<TValue> IsNotNull()
    {
        if (Current == null)
            ThrowTestFailureReport(AssertFailures.IsNotNull(), Current, null);
        return this;
    }

    public IAssert OverrideFailureMessage(string message)
    {
        CustomFailureMessage = message;
        return this;
    }

#pragma warning disable IDE0060 // Remove unused parameter
    protected void ThrowTestFailureReport(string message, object? current, object? expected)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        var failureMessage = (CustomFailureMessage ?? message).UnixFormat();
        CurrentFailureMessage = failureMessage;
        throw new TestFailedException(failureMessage);
    }

    internal static bool IsSame<TLeft, TRight>(TLeft lKey, TRight rKey)
    {
        var left = lKey.UnboxVariant();
        var right = rKey.UnboxVariant();

        if ((left is string || left?.GetType().IsPrimitive) ?? false)
            return Equals(left, right);
        return ReferenceEquals(left, right);
    }
}
