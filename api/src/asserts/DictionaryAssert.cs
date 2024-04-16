namespace GdUnit4.Asserts;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CommandLine;

public sealed class DictionaryAssert<TKey, TValue> : AssertBase<IEnumerable>, IDictionaryAssert<TKey, TValue> where TKey : notnull
{

    private bool IsGeneric => CurrentTyped != null;

    private new IDictionary? Current => base.Current as IDictionary;

    private IDictionary<TKey, TValue>? CurrentTyped => base.Current as IDictionary<TKey, TValue>;

    public DictionaryAssert(IDictionary<TKey, TValue>? current) : base(current)
    { }

#pragma warning disable CA1000 // Do not declare static members on generic types
    public static DictionaryAssert<TKey, TValue> From(IDictionary? current)
#pragma warning restore CA1000 // Do not declare static members on generic types
        => new(current);

    private DictionaryAssert(IDictionary? current) : base(current)
    { }

    private int ItemCount => IsGeneric ? CurrentTyped?.Count ?? 0 : Current?.Count ?? 0;

    private ICollection<TKey> Keys => (IsGeneric ? CurrentTyped?.Keys : Current?.Keys.Cast<TKey>().ToList()) ?? new List<TKey>();

    private TValue? TryGetValue(TKey key)
    {
        if (Current != null)
            return Current[key].Cast<TValue>();
        return CurrentTyped?.ContainsKey(key) == true ? CurrentTyped[key] : default;
    }

    public IDictionaryAssert<TKey, TValue> IsEmpty()
    {
        if (Current == null && CurrentTyped == null)
            ThrowTestFailureReport(AssertFailures.IsEmpty(ItemCount, true), ItemCount, null);
        if (ItemCount != 0)
            ThrowTestFailureReport(AssertFailures.IsEmpty(ItemCount, CurrentTyped == null && Current == null), ItemCount, null);
        return this;
    }

    public void CheckNotNull()
    {
        if (base.Current == null)
            ThrowTestFailureReport(AssertFailures.IsNotNull(), base.Current, null);
    }

    public IDictionaryAssert<TKey, TValue> IsNotEmpty()
    {
        CheckNotNull();
        if (ItemCount == 0)
            ThrowTestFailureReport(AssertFailures.IsNotEmpty(), base.Current, null);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> HasSize(int expected)
    {
        CheckNotNull();
        if (ItemCount != expected)
            ThrowTestFailureReport(AssertFailures.HasSize(base.Current, expected), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> ContainsKeys(params TKey[] expected)
    {
        CheckNotNull();
        var notFound = expected.Where(key => !Keys.Contains(key)).ToList();

        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Keys, expected!, notFound), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> ContainsKeys(IEnumerable expected)
        => ContainsKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> NotContainsKeys(params TKey[] expected)
    {
        CheckNotNull();
        var found = expected.Where(Keys.Contains).ToList();
        if (found.Count > 0)
            ThrowTestFailureReport(AssertFailures.NotContains(Keys, expected, found), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> NotContainsKeys(IEnumerable expected)
        => NotContainsKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> ContainsKeyValue(TKey key, TValue value)
    {
        CheckNotNull();
        var hasKey = Keys.Contains(key);
        var expectedKeyValue = new Dictionary<TKey, TValue>() { { key, value } };
        if (!hasKey)
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue), base.Current, expectedKeyValue);

        var currentValue = TryGetValue(key);
        var result = Comparable.IsEqual(currentValue, value);
        if (!result.Valid)
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue, currentValue), base.Current, expectedKeyValue);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> NotContainsSameKeys(params TKey[] expected)
    {
        CheckNotNull();
        var found = expected.Where(key => Enumerable.Any(Keys, (k) => IsSame(k, key))).ToList();
        if (found.Count > 0)
            ThrowTestFailureReport(AssertFailures.NotContains(Keys, expected, found), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> NotContainsSameKeys(IEnumerable expected)
        => NotContainsSameKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> ContainsSameKeys(params TKey[] expected)
    {
        CheckNotNull();
        var notFound = expected.Where(key => !Enumerable.Any(Keys, (k) => IsSame(k, key))).ToList();
        if (notFound.Count > 0)
            ThrowTestFailureReport(AssertFailures.Contains(Keys, expected!, notFound), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> ContainsSameKeys(IEnumerable expected)
        => ContainsSameKeys(expected.Cast<TKey>().ToArray());

    public IDictionaryAssert<TKey, TValue> ContainsSameKeyValue(TKey key, TValue value)
    {
        CheckNotNull();
        var hasKey = Enumerable.Any(Keys, (k) => IsSame(k, key));
        var expectedKeyValue = new Dictionary<TKey, TValue>() { { key, value } };
        if (!hasKey)
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue), base.Current, expectedKeyValue);

        var currentValue = TryGetValue(key);
        if (!IsSame(currentValue, value))
            ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue, currentValue), base.Current, expectedKeyValue);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> IsSame(IEnumerable expected)
    {
        if (!ReferenceEquals(base.Current, expected))
            ThrowTestFailureReport(AssertFailures.IsSame(base.Current, expected), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> IsSame(IDictionary<TKey, TValue> expected)
    {
        if (!ReferenceEquals(base.Current, expected))
            ThrowTestFailureReport(AssertFailures.IsSame(base.Current, expected), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> IsNotSame(IEnumerable expected)
    {
        if (ReferenceEquals(base.Current, expected))
            ThrowTestFailureReport(AssertFailures.IsSame(base.Current, expected), base.Current, expected);
        return this;
    }

    public IDictionaryAssert<TKey, TValue> IsNotSame(IDictionary<TKey, TValue> expected)
    {
        if (ReferenceEquals(base.Current, expected))
            ThrowTestFailureReport(AssertFailures.IsSame(base.Current, expected), base.Current, expected);
        return this;
    }


    public new IDictionaryAssert<TKey, TValue> OverrideFailureMessage(string message)
        => (IDictionaryAssert<TKey, TValue>)base.OverrideFailureMessage(message);
}
