// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Core.Extensions;

using Extractors;

using Array = Godot.Collections.Array;

internal sealed class EnumerableAssert<TValue> : AssertBase<IEnumerable<TValue?>>, IEnumerableAssert<TValue?>
{
    public EnumerableAssert(IEnumerable? current)
        : base(current?.Cast<TValue?>())
    {
    }

    public EnumerableAssert(IEnumerable<TValue?>? current)
        : base(current)
    {
    }

    public IEnumerableAssert<TValue?> IsEqualIgnoringCase(IEnumerable<TValue?> expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.Mode.CASE_INSENSITIVE);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.IsEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert<TValue?> IsNotEqualIgnoringCase(IEnumerable<TValue?> expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.Mode.CASE_INSENSITIVE);
        if (result.Valid)
            ThrowTestFailureReport(AssertFailures.IsNotEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert<TValue?> IsEmpty()
    {
        var count = Current?.Count() ?? -1;
        if (count != 0)
            ThrowTestFailureReport(AssertFailures.IsEmpty(count, Current == null), Current, count);
        return this;
    }

    public IEnumerableAssert<TValue?> IsNotEmpty()
    {
        var count = Current?.Count() ?? -1;
        if (count == 0)
            ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
        return this;
    }

    public IEnumerableAssert<TValue?> IsSame(IEnumerable<TValue?> expected)
    {
        if (!ReferenceEquals(Current, expected))
            ThrowTestFailureReport(AssertFailures.IsSame(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert<TValue?> IsNotSame(IEnumerable<TValue?> expected)
    {
        if (ReferenceEquals(Current, expected))
            ThrowTestFailureReport(AssertFailures.IsNotSame(expected), Current, expected);
        return this;
    }

    public IEnumerableAssert<TValue?> HasSize(int expected)
    {
        var count = Current?.Count();
        if (count != expected)
            ThrowTestFailureReport(AssertFailures.HasSize(count, expected), Current, null);
        return this;
    }

    public IEnumerableAssert<TValue?> Contains(params TValue?[] expected)
        => CheckContains(expected.ToList(), false);

    public IEnumerableAssert<TValue?> Contains(IEnumerable<TValue?> expected)
        => CheckContains(expected, false);

    public IEnumerableAssert<TValue?> Contains(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContains(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSame(params TValue?[] expected)
        => CheckContains(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSame(IEnumerable<TValue?> expected)
        => CheckContains(expected, true);

    public IEnumerableAssert<TValue?> ContainsSame(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContains(expected_!, true);
    }

    public IEnumerableAssert<TValue?> ContainsExactly(params TValue?[] expected)
        => CheckContainsExactly(expected.ToList(), false);

    public IEnumerableAssert<TValue?> ContainsExactly(IEnumerable<TValue?> expected)
        => CheckContainsExactly(expected, false);

    public IEnumerableAssert<TValue?> ContainsExactly(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactly(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSameExactly(params TValue?[] expected)
        => CheckContainsExactly(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSameExactly(IEnumerable<TValue?> expected)
        => CheckContainsExactly(expected, true);

    public IEnumerableAssert<TValue?> ContainsSameExactly(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactly(expected_!, true);
    }

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(params TValue?[] expected)
        => CheckContainsExactlyInAnyOrder(expected.ToList(), false);

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(IEnumerable<TValue?> expected)
        => CheckContainsExactlyInAnyOrder(expected, false);

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactlyInAnyOrder(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(params TValue?[] expected)
        => CheckContainsExactlyInAnyOrder(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(IEnumerable<TValue?> expected)
        => CheckContainsExactlyInAnyOrder(expected, true);

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactlyInAnyOrder(expected_!, true);
    }

    public IEnumerableAssert<TValue?> NotContains(params TValue?[] expected)
        => CheckNotContains(expected.ToList(), false);

    public IEnumerableAssert<TValue?> NotContains(IEnumerable<TValue?> expected)
        => CheckNotContains(expected, false);

    public IEnumerableAssert<TValue?> NotContains(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckNotContains(expected_!, false);
    }

    public IEnumerableAssert<TValue?> NotContainsSame(params TValue?[] expected)
        => CheckNotContains(expected.ToList(), true);

    public IEnumerableAssert<TValue?> NotContainsSame(IEnumerable<TValue?> expected)
        => CheckNotContains(expected, true);

    public IEnumerableAssert<TValue?> NotContainsSame(Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckNotContains(expected_!, true);
    }

    public IEnumerableAssert<object?> Extract(string methodName, params object[] args)
        => ExtractV(new ValueExtractor(methodName, args));

    public IEnumerableAssert<object?> ExtractV(params IValueExtractor[] extractors)
        => new EnumerableAssert<object?>(Current?.Select(v =>
            {
                var values = extractors.Select(e => e.ExtractValue(v)).ToArray();
                return values.Length == 1 ? values.First() : new Tuple(values);
            })
            .ToList());

    public new IEnumerableAssert<TValue?> OverrideFailureMessage(string message)
        => (IEnumerableAssert<TValue?>)base.OverrideFailureMessage(message);

    private EnumerableAssert<TValue?> CheckContains(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        var expectedList = expected.ToList();

        // we test for contains nothing
        if (expectedList.Count == 0)
            return this!;

        // Create list once to avoid multiple enumerations
        var notFound = expectedList.Where(expectedItem =>
                Current?.Any(currentItem =>
                    referenceEquals ? expectedItem.IsSame(currentItem) : expectedItem.IsEquals(currentItem)) != true) // This pattern maintains the original null handling
            .ToList();

        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Current, expectedList, notFound), Current, expected);
        return this!;
    }

    private EnumerableAssert<TValue?> CheckContainsExactly(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        var expectedList = expected.ToList();

        // we test for contains nothing
        if (expectedList.Count == 0)
            return this!;

        var diff = DiffArrayExactly(Current, expectedList, referenceEquals);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactly(Current, expectedList, notFound, notExpected), Current, expected);
        return this!;
    }

    private EnumerableAssert<TValue?> CheckContainsExactlyInAnyOrder(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        var expectedList = expected.ToList();

        // we test for contains nothing
        if (expectedList.Count == 0)
            return this!;

        var diff = DiffArrayAnyOrder(Current, expectedList, referenceEquals);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;

        // no difference and additions found
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactlyInAnyOrder(Current, expectedList, notFound, notExpected), Current, expected);
        return this!;
    }

    private EnumerableAssert<TValue?> CheckNotContains(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        var expectedList = expected.ToList();

        // we test for contains nothing
        if (expectedList.Count == 0)
            return this!;

        var found = Current?
            .Where(currentItem =>
                expectedList.Any(expectedItem =>
                    referenceEquals ? currentItem.IsSame(expectedItem) : currentItem.IsEquals(expectedItem)))
            .ToList() ?? new List<TValue?>();

        if (found.Count != 0)
            ThrowTestFailureReport(AssertFailures.NotContains(Current, expectedList, found), Current, expected);
        return this!;
    }

    private ArrayDiff DiffArrayAnyOrder(
        IEnumerable<TValue?>? current,
        IEnumerable<TValue?>? expected,
        bool referenceEquals = false)
    {
        var ll = current?.ToArray() ?? System.Array.Empty<TValue?>();
        var rr = expected?.ToArray() ?? System.Array.Empty<TValue?>();

        var notExpected = ll.Where(left =>
            !rr.Any(e => referenceEquals ? left.IsSame(e) : left.IsEquals(e))).ToList();
        var notFound = rr.Where(right =>
            !ll.Any(e => referenceEquals ? right.IsSame(e) : right.IsEquals(e))).ToList();

        return new ArrayDiff
        {
            NotExpected = notExpected,
            NotFound = notFound
        };
    }

    private ArrayDiff DiffArrayExactly(
        IEnumerable<TValue?>? current,
        IEnumerable<TValue?>? expected,
        bool referenceEquals = false)
    {
        var ll = current?.ToArray() ?? System.Array.Empty<TValue?>();
        var rr = expected?.ToArray() ?? System.Array.Empty<TValue?>();

        var notExpected = new List<TValue?>();
        var notFound = new List<TValue?>();

        for (var i = 0; i < ll.Length; i++)
        {
            var left = ll[i];
            if (i >= rr.Length)
            {
                notExpected.Add(left);
                continue;
            }

            var right = rr[i];
            if (!(referenceEquals ? left.IsSame(right) : left.IsEquals(right)))
                notExpected.Add(left);
        }

        for (var i = 0; i < rr.Length; i++)
        {
            var right = rr[i];
            if (i >= ll.Length)
            {
                notFound.Add(right);
                continue;
            }

            var left = ll[i];
            if (!(referenceEquals ? right.IsSame(left) : right.IsEquals(left)))
                notFound.Add(right);
        }

        return new ArrayDiff
        {
            NotExpected = notExpected,
            NotFound = notFound
        };
    }

    private class ArrayDiff
    {
        public List<TValue?> NotExpected { get; init; } = new();

        public List<TValue?> NotFound { get; init; } = new();
    }
}

internal static class CompareExtensions
{
    internal static bool IsEquals<T>(this T? c, T? e) => Comparable.IsEqual(c, e).Valid;

    internal static bool IsSame<T>(this T? c, T? e) => AssertBase<T>.IsSame(c, e);
}
