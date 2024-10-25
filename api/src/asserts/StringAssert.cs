namespace GdUnit4.Asserts;

using System;

internal sealed class StringAssert : AssertBase<string>, IStringAssert
{
    public StringAssert(string? current) : base(current)
    {
    }

    public IStringAssert Contains(string expected)
    {
        if (Current == null || !Current.Contains(expected))
            ThrowTestFailureReport(AssertFailures.Contains(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert ContainsIgnoringCase(string expected)
    {
        if (Current == null || !Current.ToLower().Contains(expected.ToLower(), StringComparison.OrdinalIgnoreCase))
            ThrowTestFailureReport(AssertFailures.ContainsIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert EndsWith(string expected)
    {
        if (Current == null || !Current.EndsWith(expected))
            ThrowTestFailureReport(AssertFailures.EndsWith(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert HasLength(int expectedLength, IStringAssert.Compare comparator = IStringAssert.Compare.EQUAL)
    {
        if (Current == null)
            ThrowTestFailureReport(AssertFailures.HasLength(-1, expectedLength, comparator), Current, expectedLength);

        var currentLength = Current?.Length ?? -1;
        var failed = false;
        switch (comparator)
        {
            case IStringAssert.Compare.EQUAL:
                if (currentLength != expectedLength)
                    failed = true;
                break;
            case IStringAssert.Compare.GREATER_EQUAL:
                if (currentLength < expectedLength)
                    failed = true;
                break;
            case IStringAssert.Compare.GREATER_THAN:
                if (currentLength <= expectedLength)
                    failed = true;
                break;
            case IStringAssert.Compare.LESS_EQUAL:
                if (currentLength > expectedLength)
                    failed = true;
                break;
            case IStringAssert.Compare.LESS_THAN:
                if (currentLength >= expectedLength)
                    failed = true;
                break;
        }

        if (failed)
            ThrowTestFailureReport(AssertFailures.HasLength(currentLength, expectedLength, comparator), Current, expectedLength);
        return this;
    }

    public IStringAssert IsEmpty()
    {
        if (Current == null || Current.Length > 0)
            ThrowTestFailureReport(AssertFailures.IsEmpty(Current), Current, null);
        return this;
    }

    public IStringAssert IsEqualIgnoringCase(string expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.IsEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert IsNotEmpty()
    {
        if (Current?.Length == 0)
            ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
        return this;
    }

    public IStringAssert IsNotEqualIgnoringCase(string expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
        if (result.Valid)
            ThrowTestFailureReport(AssertFailures.IsNotEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert NotContains(string expected)
    {
        if (Current != null && Current.Contains(expected))
            ThrowTestFailureReport(AssertFailures.NotContains(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert NotContainsIgnoringCase(string expected)
    {
        if (Current != null && Current.ToLower().Contains(expected.ToLower(), StringComparison.OrdinalIgnoreCase))
            ThrowTestFailureReport(AssertFailures.NotContainsIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert StartsWith(string expected)
    {
        if (Current == null || !Current.StartsWith(expected))
            ThrowTestFailureReport(AssertFailures.StartsWith(Current, expected), Current, expected);
        return this;
    }

    public new IStringAssert OverrideFailureMessage(string message)
    {
        base.OverrideFailureMessage(message);
        return this;
    }
}
