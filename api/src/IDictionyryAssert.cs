namespace GdUnit4.Asserts;

using System.Collections;
using System.Collections.Generic;


/// <summary> An Assertion Tool to verify dictionary values </summary>
public interface IDictionaryAssert<TKey, TValue> : IAssertBase<IDictionary<TKey, TValue>> where TKey : notnull
{
    /// <summary> Verifies that the current dictionary is empty, it has a size of 0.</summary>
    public IDictionaryAssert<TKey, TValue> IsEmpty();

    /// <summary> Verifies that the current dictionary is not empty, it has a size of minimum 1.</summary>
    public IDictionaryAssert<TKey, TValue> IsNotEmpty();

    /// <summary> Verifies that the current dictionary has a size of given value.</summary>
    public IDictionaryAssert<TKey, TValue> HasSize(int expected);


    /// <summary> Verifies that the current dictionary contains the given key(s).</summary>
    public IDictionaryAssert<TKey, TValue> ContainsKeys(params TKey[] expected);
    public IDictionaryAssert<TKey, TValue> ContainsKeys(IEnumerable expected);

    /// <summary> Verifies that the current dictionary not contains the given key(s).</summary>
    public IDictionaryAssert<TKey, TValue> NotContainsKeys(params TKey[] expected);
    public IDictionaryAssert<TKey, TValue> NotContainsKeys(IEnumerable expected);

    /// <summary> Verifies that the current dictionary contains the given key and value.</summary>
    public IDictionaryAssert<TKey, TValue> ContainsKeyValue(TKey key, TValue value);

    public new IDictionaryAssert<TKey, TValue> OverrideFailureMessage(string message);
}
