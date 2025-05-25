// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System;

using Core.Extensions;

internal sealed class StringAssert : AssertBase<string>, IStringAssert
{
    public StringAssert(string? current)
        : base(current)
    {
    }

    public IStringAssert Contains(string expected)
    {
        if (Current == null || !Current.Contains(expected, StringComparison.Ordinal))
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

    public IStringAssert HasLength(int length, IStringAssert.Compare comparator = IStringAssert.Compare.EQUAL)
    {
        if (Current == null)
            ThrowTestFailureReport(AssertFailures.HasLength(-1, length, comparator), Current, length);

        var currentLength = Current?.Length ?? -1;
        var failed = false;
        switch (comparator)
        {
            case IStringAssert.Compare.EQUAL:
                if (currentLength != length)
                    failed = true;
                break;
            case IStringAssert.Compare.GREATER_EQUAL:
                if (currentLength < length)
                    failed = true;
                break;
            case IStringAssert.Compare.GREATER_THAN:
                if (currentLength <= length)
                    failed = true;
                break;
            case IStringAssert.Compare.LESS_EQUAL:
                if (currentLength > length)
                    failed = true;
                break;
            case IStringAssert.Compare.LESS_THAN:
                if (currentLength >= length)
                    failed = true;
                break;

            // ReSharper disable once RedundantEmptySwitchSection
            default:
                break;
        }

        if (failed)
            ThrowTestFailureReport(AssertFailures.HasLength(currentLength, length, comparator), Current, length);
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
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.Mode.CASE_INSENSITIVE);
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
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.Mode.CASE_INSENSITIVE);
        if (result.Valid)
            ThrowTestFailureReport(AssertFailures.IsNotEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IStringAssert NotContains(string expected)
    {
        if (Current != null && Current.Contains(expected, StringComparison.Ordinal))
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
