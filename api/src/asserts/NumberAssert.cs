namespace GdUnit4.Asserts;
using System;


internal class NumberAssert<TValue> : AssertBase<TValue>, INumberAssert<TValue> where TValue : notnull, IComparable, IComparable<TValue>
{
    public NumberAssert(TValue current) : base(current)
    { }

    public INumberAssert<TValue> IsBetween(TValue from, TValue to)
    {
        if (Current?.CompareTo(from) < 0 || Current?.CompareTo(to) > 0)
            ThrowTestFailureReport(AssertFailures.IsBetween(Current, from, to), Current, new TValue[] { from, to });
        return this;
    }

    public INumberAssert<TValue> IsEven()
    {
        if (Convert.ToInt64(Current) % 2 != 0)
            ThrowTestFailureReport(AssertFailures.IsEven(Current), Current, null);
        return this;
    }

    public INumberAssert<TValue> IsGreater(TValue expected)
    {
        if (Current?.CompareTo(expected) <= 0)
            ThrowTestFailureReport(AssertFailures.IsGreater(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsGreaterEqual(TValue expected)
    {
        if (Current?.CompareTo(expected) < 0)
            ThrowTestFailureReport(AssertFailures.IsGreaterEqual(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsIn(params TValue[] expected)
    {
        if (Array.IndexOf(expected, Current) == -1)
            ThrowTestFailureReport(AssertFailures.IsIn(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsLess(TValue expected)
    {
        if (Current?.CompareTo(expected) >= 0)
            ThrowTestFailureReport(AssertFailures.IsLess(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsLessEqual(TValue expected)
    {
        if (Current?.CompareTo(expected) > 0)
            ThrowTestFailureReport(AssertFailures.IsLessEqual(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsNegative()
    {
        if (Current?.CompareTo(0) >= 0)
            ThrowTestFailureReport(AssertFailures.IsNegative(Current), Current, null);
        return this;
    }

    public INumberAssert<TValue> IsNotIn(params TValue[] expected)
    {
        if (Array.IndexOf(expected, Current) != -1)
            ThrowTestFailureReport(AssertFailures.IsNotIn(Current, expected), Current, expected);
        return this;
    }

    public INumberAssert<TValue> IsNotNegative()
    {
        if (Current?.CompareTo(0) < 0)
            ThrowTestFailureReport(AssertFailures.IsNotNegative(Current), Current, null);
        return this;
    }

    public INumberAssert<TValue> IsNotZero()
    {
        if (Convert.ToInt64(Current) == 0)
            ThrowTestFailureReport(AssertFailures.IsNotZero(), Current, null);
        return this;
    }

    public INumberAssert<TValue> IsOdd()
    {
        if (Convert.ToInt64(Current) % 2 == 0)
            ThrowTestFailureReport(AssertFailures.IsOdd(Current), Current, null);
        return this;
    }

    public INumberAssert<TValue> IsZero()
    {
        if (Convert.ToInt64(Current) != 0)
            ThrowTestFailureReport(AssertFailures.IsZero(Current), Current, null);
        return this;
    }

    public new INumberAssert<TValue> OverrideFailureMessage(string message)
    {
        base.OverrideFailureMessage(message);
        return this;
    }
}
