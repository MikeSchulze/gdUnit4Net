namespace GdUnit4.Asserts;

using System.Collections;
using System.Collections.Generic;
using System.Linq;


internal sealed class DictionaryAssert<TKey, TValue> : AssertBase<IDictionary<TKey, TValue>>, IDictionaryAssert<TKey, TValue> where TKey : notnull
{
    public DictionaryAssert(IDictionary<TKey, TValue>? current) : base(current)
    { }

    public IDictionaryAssert<TKey, TValue> IsEmpty()
    {
        if (Current == null || Current.Count != 0)
            ThrowTestFailureReport(AssertFailures.IsEmpty(Current?.Count ?? 0, Current == null), Current, null);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> IsNotEmpty()
    {
        IsNotNull();
        if (Current?.Count == 0)
            ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> HasSize(int expected)
    {
        IsNotNull();
        if (Current?.Count != expected)
            ThrowTestFailureReport(AssertFailures.HasSize(Current!, expected), Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> ContainsKeys(params TKey[] expected)
    {
        IsNotNull();
        IEnumerable<TKey> keys = Current?.Keys.Cast<TKey>().ToList() ?? new List<TKey>();
        var notFound = expected.Where(key => !keys.Contains(key)).ToList();

        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(keys, expected!, notFound), Current, expected);
        return this;
    }
    public IDictionaryAssert<TKey, TValue> ContainsKeys(IEnumerable expected) => ContainsKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> NotContainsKeys(params TKey[] expected)
    {
        IsNotNull();
        IEnumerable<TKey> keys = Current?.Keys.Cast<TKey>().ToList() ?? new List<TKey>();
        var found = expected.Where(key => keys.Contains(key)).ToList();
        if (found.Count > 0)
            ThrowTestFailureReport(AssertFailures.NotContains(keys, expected, found), Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> NotContainsKeys(IEnumerable expected) => NotContainsKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> ContainsKeyValue(TKey key, TValue value)
    {
        IsNotNull();
        var hasKey = Current!.ContainsKey(key);
        var expectedKeyValue = new Dictionary<TKey, TValue>() { { key, value } };
        if (!hasKey)
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue), Current, expectedKeyValue);

        var currentValue = Current[key];
        var result = Comparable.IsEqual(currentValue, value);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue, currentValue), Current, expectedKeyValue);
        return this;
    }

    public new IDictionaryAssert<TKey, TValue> OverrideFailureMessage(string message)
    {
        base.OverrideFailureMessage(message);
        return this;
    }
}
