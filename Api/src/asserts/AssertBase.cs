// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using CommandLine;

using Core.Execution.Exceptions;
using Core.Extensions;

internal abstract class AssertBase<TValue, TAssert> : IAssertBase<TValue>, IAssertMessage<TAssert>
    where TAssert : IAssert
{
    protected AssertBase(TValue? current) => Current = current;

    protected TValue? Current { get; }

    protected string? CustomFailureMessage { get; private set; }

    protected string CurrentFailureMessage { get; set; } = string.Empty;

    protected string AppendingFailureMessage { get; set; } = string.Empty;

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

    public TAssert OverrideFailureMessage(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        CustomFailureMessage = message;
        return this.Cast<TAssert>();
    }

    public TAssert AppendFailureMessage(string message)
    {
        ArgumentException.ThrowIfNullOrEmpty(message);
        AppendingFailureMessage = message;
        return this.Cast<TAssert>();
    }

    internal static bool IsSame<TLeft, TRight>(TLeft lKey, TRight rKey)
    {
        var left = lKey.UnboxVariant();
        var right = rKey.UnboxVariant();

        if ((left is string || left?.GetType().IsPrimitive) ?? false)
            return Equals(left, right);
        return ReferenceEquals(left, right);
    }

#pragma warning disable IDE0060 // Remove unused parameter
    protected void ThrowTestFailureReport(string message, object? current, object? expected)
#pragma warning restore IDE0060 // Remove unused parameter
    {
#pragma warning disable CA1062
        var failureMessage = (CustomFailureMessage ?? message).UnixFormat();
#pragma warning restore CA1062
        if (AppendingFailureMessage.Length != 0)
        {
            failureMessage = $"""
                              {failureMessage}

                              Additional info:
                              {AppendingFailureMessage}
                              """;
        }

        CurrentFailureMessage = failureMessage;
        throw new TestFailedException(failureMessage);
    }
}
#pragma warning restore CS1591, SA1600
