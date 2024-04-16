namespace GdUnit4.Asserts;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public sealed class EnumerableAssert<TValue> : AssertBase<IEnumerable<TValue?>>, IEnumerableAssert<TValue?>
{

    public EnumerableAssert(IEnumerable? current) : base(current?.Cast<TValue?>())
    { }

    public EnumerableAssert(IEnumerable<TValue?>? current) : base(current)
    { }

    public IEnumerableAssert<TValue?> IsEqualIgnoringCase(IEnumerable<TValue?> expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.IsEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert<TValue?> IsNotEqualIgnoringCase(IEnumerable<TValue?> expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
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
    public IEnumerableAssert<TValue?> Contains(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContains(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSame(params TValue?[] expected)
          => CheckContains(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSame(IEnumerable<TValue?> expected)
        => CheckContains(expected, true);

    public IEnumerableAssert<TValue?> ContainsSame(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContains(expected_!, true);
    }

    public IEnumerableAssert<TValue?> ContainsExactly(params TValue?[] expected)
        => CheckContainsExactly(expected.ToList(), false);

    public IEnumerableAssert<TValue?> ContainsExactly(IEnumerable<TValue?> expected)
        => CheckContainsExactly(expected, false);

    public IEnumerableAssert<TValue?> ContainsExactly(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactly(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSameExactly(params TValue?[] expected)
        => CheckContainsExactly(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSameExactly(IEnumerable<TValue?> expected)
        => CheckContainsExactly(expected, true);

    public IEnumerableAssert<TValue?> ContainsSameExactly(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactly(expected_!, true);
    }

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(params TValue?[] expected)
        => CheckContainsExactlyInAnyOrder(expected.ToList(), false);

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(IEnumerable<TValue?> expected)
        => CheckContainsExactlyInAnyOrder(expected, false);

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactlyInAnyOrder(expected_!, false);
    }

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(params TValue?[] expected)
        => CheckContainsExactlyInAnyOrder(expected.ToList(), true);

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(IEnumerable<TValue?> expected)
        => CheckContainsExactlyInAnyOrder(expected, true);

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckContainsExactlyInAnyOrder(expected_!, true);
    }

    public IEnumerableAssert<TValue?> NotContains(params TValue?[] expected)
        => CheckNotContains(expected.ToList(), false);

    public IEnumerableAssert<TValue?> NotContains(IEnumerable<TValue?> expected)
        => CheckNotContains(expected, false);

    public IEnumerableAssert<TValue?> NotContains(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckNotContains(expected_!, false);
    }

    public IEnumerableAssert<TValue?> NotContainsSame(params TValue?[] expected)
        => CheckNotContains(expected.ToList(), true);

    public IEnumerableAssert<TValue?> NotContainsSame(IEnumerable<TValue?> expected)
         => CheckNotContains(expected, true);

    public IEnumerableAssert<TValue?> NotContainsSame(Godot.Collections.Array expected)
    {
        var expected_ = expected as IEnumerable<TValue?>;
        return CheckNotContains(expected_!, true);
    }

    public IEnumerableAssert<object?> Extract(string funcName, params object[] args)
        => ExtractV(new ValueExtractor(funcName, args));

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
        // we test for contains nothing
        if (!expected.Any())
            return this;

        var notFound = expected
                    .ToList()
                    .FindAll(left => !Current?.Any(e => (referenceEquals ? IsSame(left) : IsEquals(left)).Invoke(e)) ?? true);
        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Current, expected, notFound), Current, expected);
        return this;
    }

    private EnumerableAssert<TValue?> CheckContainsExactly(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        // we test for contains nothing
        if (!expected.Any())
            return this;

        var diff = DiffArrayExactly(Current, expected, referenceEquals);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactly(Current, expected, notFound, notExpected), Current, expected);
        return this;
    }

    private EnumerableAssert<TValue?> CheckContainsExactlyInAnyOrder(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        // we test for contains nothing
        if (!expected.Any())
            return this;

        var diff = DiffArrayAnyOrder(Current, expected, referenceEquals);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        // no difference and additions found
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactlyInAnyOrder(Current, expected, notFound, notExpected), Current, expected);
        return this;
    }

    private EnumerableAssert<TValue?> CheckNotContains(IEnumerable<TValue?> expected, bool referenceEquals)
    {
        if (!expected.Any())
            return this;

        var found = Current?
                .ToList()
                .FindAll(left => expected?.Any(e => (referenceEquals ? IsSame(left) : IsEquals(left)).Invoke(e)) ?? false)
            ?? new List<TValue?>();
        if (found.Count != 0)
            ThrowTestFailureReport(AssertFailures.NotContains(Current, expected, found), Current, expected);
        return this;
    }

    private class ArrayDiff
    {
        public List<TValue?> NotExpected { get; set; } = new List<TValue?>();
        public List<TValue?> NotFound { get; set; } = new List<TValue?>();
    }

    private ArrayDiff DiffArrayAnyOrder(
        IEnumerable<TValue?>? current,
        IEnumerable<TValue?>? expected,
        bool referenceEquals = false)
    {
        var ll = current?.ToArray() ?? Array.Empty<TValue?>();
        var rr = expected?.ToArray() ?? Array.Empty<TValue?>();

        var notExpected = new List<TValue?>();
        var notFound = new List<TValue?>();

        for (var i = 0; i < ll.Length; i++)
        {
            var left = ll[i];
            if (!rr.Any(e => (referenceEquals ? IsSame(left) : IsEquals(left)).Invoke(e)))
                notExpected.Add(left);
        }
        for (var i = 0; i < rr.Length; i++)
        {
            var right = rr[i];
            if (!ll.Any(e => (referenceEquals ? IsSame(right) : IsEquals(right)).Invoke(e)))
                notFound.Add(right);
        }

        return new ArrayDiff()
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
        var ll = current?.ToArray() ?? Array.Empty<TValue?>();
        var rr = expected?.ToArray() ?? Array.Empty<TValue?>();

        var notExpected = new List<TValue?>();
        var notFound = new List<TValue?>();

        for (var i = 0; i < ll.Length; i++)
        {
            var left = ll[i];
            if (i >= rr.Length || !(referenceEquals ? IsSame(left) : IsEquals(left)).Invoke(rr[i]))
                notExpected.Add(left);
        }
        for (var i = 0; i < rr.Length; i++)
        {
            var right = rr[i];
            if (i >= ll.Length || !(referenceEquals ? IsSame(right) : IsEquals(right)).Invoke(ll[i]))
                notFound.Add(right);
        }
        return new ArrayDiff()
        {
            NotExpected = notExpected,
            NotFound = notFound,
        };
    }

    private static Func<TValue?, bool> IsEquals(TValue? c) => e => c.UnboxVariant() == e.UnboxVariant();

    private static Func<TValue?, bool> IsSame(TValue? c) => e => AssertBase<TValue>.IsSame(c, e);

}
