// ReSharper disable InvalidXmlDocComment

namespace GdUnit4.Asserts;

using System.Collections;
using System.Collections.Generic;

/// <summary>
///     An Assertion Tool to verify dictionary values
/// </summary>
public interface IDictionaryAssert<TKey, TValue> : IAssertBase<IEnumerable> where TKey : notnull
{
    /// <summary>
    ///     Verifies that the current dictionary is empty, it has a size of 0.
    /// </summary>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> IsEmpty();

    /// <summary>
    ///     Verifies that the current dictionary is not empty, it has a size of minimum 1.
    /// </summary>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> IsNotEmpty();

    /// <summary>
    ///     Verifies that the current dictionary has a size of given value.
    /// </summary>
    /// <param name="expected">The expected size</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> HasSize(int expected);

    /// <summary>
    ///     Verifies that the current dictionary contains the given key(s).
    /// </summary>
    /// <param name="expected">The keys to be contains</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> ContainsKeys(params TKey[] expected);

    public IDictionaryAssert<TKey, TValue> ContainsKeys(IEnumerable expected);

    /// <summary>
    ///     Verifies that the current dictionary not contains the given key(s).
    /// </summary>
    /// <param name="expected">Keys to be NOT contains</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> NotContainsKeys(params TKey[] expected);

    public IDictionaryAssert<TKey, TValue> NotContainsKeys(IEnumerable expected);

    /// <summary>
    ///     Verifies that the current dictionary not contains the given key(s) by object reference
    /// </summary>
    /// <remarks>
    ///     The key and value are compared by object reference, for deep parameter comparison use <see cref="NotContainsKeys" />
    /// </remarks>
    /// <param name="expected">Keys to be NOT contains</param>
    /// <returns></returns>
    public IDictionaryAssert<TKey, TValue> NotContainsSameKeys(params TKey[] expected);

    public IDictionaryAssert<TKey, TValue> NotContainsSameKeys(IEnumerable expected);

    /// <summary>
    ///     Verifies that the current dictionary contains the given key and value.
    /// </summary>
    /// <param name="key">The key to contains</param>
    /// <param name="value">The value to contains</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> ContainsKeyValue(TKey key, TValue value);

    /// <summary>
    ///     Verifies that the current dictionary contains the given key(s) by object reference
    /// </summary>
    /// <remarks>
    ///     The key and value are compared by object reference, for deep parameter comparison use <see cref="ContainsKeys" />
    /// </remarks>
    /// <param name="expected">The keys to be contains</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> ContainsSameKeys(params TKey[] expected);

    public IDictionaryAssert<TKey, TValue> ContainsSameKeys(IEnumerable expected);

    /// <summary>
    ///     Verifies that the current dictionary contains the given key and value by object reference.
    /// </summary>
    /// <param name="key">The key to contains</param>
    /// <param name="value">The value to contains</param>
    /// <remarks>
    ///     The key and value are compared by object reference, for deep parameter comparison use <see cref="ContainsKeyValue" />
    /// </remarks>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> ContainsSameKeyValue(TKey key, TValue value);

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message</param>
    /// <returns>IDictionaryAssert</returns>
    public new IDictionaryAssert<TKey, TValue> OverrideFailureMessage(string message);

    /// <summary>
    ///     Verifies that the current dictionary is the same.
    /// </summary>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep parameter comparison use <see cref="IAssertBase.IsEqual(TValue)" />
    /// </remarks>
    /// <param name="expected">The dictionary to be the same</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> IsSame(IDictionary<TKey, TValue> expected);

    public IDictionaryAssert<TKey, TValue> IsSame(IEnumerable expected);

    /// <summary>
    ///     Verifies that the current dictionary is NOT the same.
    /// </summary>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep parameter comparison use <see cref="IAssertBase.IsNotEqual(TValue)" />
    /// </remarks>
    /// <param name="expected">The dictionary to be NOT the same</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> IsNotSame(IDictionary<TKey, TValue> expected);

    /// <summary>
    ///     Overrides the default failure message by given custom message.
    /// </summary>
    /// <param name="message">A custom failure message</param>
    /// <returns>IDictionaryAssert</returns>
    public IDictionaryAssert<TKey, TValue> IsNotSame(IEnumerable expected);
}
