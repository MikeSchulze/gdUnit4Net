namespace GdUnit4.Asserts;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using static GdUnit4.Assertions;


internal sealed class EnumerableAssert : AssertBase<IEnumerable>, IEnumerableAssert
{
    public EnumerableAssert(IEnumerable? current) : base(current) => Current = current?.Cast<object?>();

    private new IEnumerable<object?>? Current { get; set; }

    public IEnumerableAssert IsEqualIgnoringCase(IEnumerable expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.IsEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert IsNotEqualIgnoringCase(IEnumerable expected)
    {
        var result = Comparable.IsEqual(Current, expected, GodotObjectExtensions.MODE.CaseInsensitive);
        if (result.Valid)
            ThrowTestFailureReport(AssertFailures.IsNotEqualIgnoringCase(Current, expected), Current, expected);
        return this;
    }

    public IEnumerableAssert IsEmpty()
    {
        var count = Current?.Count() ?? -1;
        if (count != 0)
            ThrowTestFailureReport(AssertFailures.IsEmpty(count, Current == null), Current, count);
        return this;
    }

    public IEnumerableAssert IsNotEmpty()
    {
        var count = Current?.Count() ?? -1;
        if (count == 0)
            ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
        return this;
    }

    public IEnumerableAssert HasSize(int expected)
    {
        var count = Current?.Count();
        if (count != expected)
            ThrowTestFailureReport(AssertFailures.HasSize(count, expected), Current, null);
        return this;
    }

    public IEnumerableAssert Contains(params object?[] expected)
    {
        // we test for contains nothing
        if (expected.Length == 0)
            return this;
        var notFound = ArrayContainsAll(Current, expected);
        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Current, expected, notFound), Current, expected);
        return this;
    }

    public IEnumerableAssert Contains(IEnumerable expected)
    {
        var expectedArray = expected.Cast<object>().ToArray();
        // we test for contains nothing
        if (expectedArray.Length == 0)
            return this;

        var notFound = ArrayContainsAll(Current, expectedArray);
        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Current, expectedArray, notFound), Current, expected);
        return this;
    }

    public IEnumerableAssert ContainsExactly(params object?[] expected)
    {
        // we test for contains nothing
        if (expected.Length == 0)
            return this;
        // is equal than it contains same elements in same order
        if (Comparable.IsEqual(Current, expected).Valid)
            return this;
        var diff = DiffArray(Current, expected);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        if (notFound.Count > 0 || notExpected.Count > 0 || (notFound.Count == 0 && notExpected.Count == 0))
            ThrowTestFailureReport(AssertFailures.ContainsExactly(Current, expected, notFound, notExpected), Current, expected);
        return this;
    }

    public IEnumerableAssert ContainsExactly(IEnumerable expected)
    {
        var expectedArray = expected is string ? new object?[] { expected } : expected.Cast<object?>().ToArray();
        // we test for contains nothing
        if (expectedArray.Length == 0)
            return this;
        // is equal than it contains same elements in same order
        if (Comparable.IsEqual(Current, expectedArray).Valid)
            return this;

        var diff = DiffArray(Current, expectedArray);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        if (notFound.Count > 0 || notExpected.Count > 0 || (notFound.Count == 0 && notExpected.Count == 0))
            ThrowTestFailureReport(AssertFailures.ContainsExactly(Current, expectedArray, notFound, notExpected), Current, expected);
        return this;
    }

    public IEnumerableAssert ContainsExactlyInAnyOrder(params object?[] expected)
    {
        // we test for contains nothing
        if (expected.Length == 0)
            return this;
        var diff = DiffArray(Current, expected);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;

        // no difference and additions found
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactlyInAnyOrder(Current, expected, notFound, notExpected), Current, expected);
        return this;
    }

    public IEnumerableAssert ContainsExactlyInAnyOrder(IEnumerable expected)
    {
        var expectedArray = expected.Cast<object?>().ToArray();
        // we test for contains nothing
        if (expectedArray.Length == 0)
            return this;

        var diff = DiffArray(Current, expectedArray);
        var notExpected = diff.NotExpected;
        var notFound = diff.NotFound;
        // no difference and additions found
        if (notExpected.Count != 0 || notFound.Count != 0)
            ThrowTestFailureReport(AssertFailures.ContainsExactlyInAnyOrder(Current, expectedArray, notFound, notExpected), Current, expected);
        return this;
    }

    public IEnumerableAssert Extract(string funcName, params object[] args) => ExtractV(new ValueExtractor(funcName, args));

    public IEnumerableAssert ExtractV(params IValueExtractor[] extractors)
    {
        Current = Current?.Select(v =>
        {
            var values = extractors.Select(e => e.ExtractValue(v)).ToArray();
            return values.Length == 1 ? values.First() : Tuple(values);
        }).ToList();
        return this;
    }

    public new IEnumerableAssert OverrideFailureMessage(string message)
        => (IEnumerableAssert)base.OverrideFailureMessage(message);

    private List<object?> ArrayContainsAll(IEnumerable<object?>? left, IEnumerable<object?>? right)
    {
        var notFound = right?.ToList() ?? new List<object?>();

        if (left != null)
        {
            var leftList = left.ToList();
            foreach (var c in leftList)
            {
                var found = right?.FirstOrDefault(e => Comparable.IsEqual(c, e).Valid);
                if (found != null)
                    notFound.Remove(found);
            }
        }
        return notFound;
    }

    private class ArrayDiff
    {
        public List<object?> NotExpected { get; set; } = new List<object?>();
        public List<object?> NotFound { get; set; } = new List<object?>();
    }

    private ArrayDiff DiffArray(IEnumerable<object?>? left, IEnumerable<object?>? right)
    {
        var ll = left?.ToList() ?? new List<object?>();
        var rr = right?.ToList() ?? new List<object?>();

        var notExpected = left?.ToList() ?? new List<object?>();
        var notFound = right?.ToList() ?? new List<object?>();

        foreach (var c in ll)
        {
            foreach (var e in rr)
            {
                if (Comparable.IsEqual(c, e).Valid)
                {
                    notExpected.Remove(c);
                    notFound.Remove(e);
                    break;
                }
            }
        }
        return new ArrayDiff() { NotExpected = notExpected, NotFound = notFound };
    }
}
