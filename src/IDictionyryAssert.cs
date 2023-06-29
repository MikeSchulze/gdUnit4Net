using System.Collections;
using System.Collections.Generic;

namespace GdUnit4.Asserts
{
    /// <summary> An Assertion Tool to verify dictionary values </summary>
    public interface IDictionaryAssert<K, V> : IAssertBase<IDictionary<K, V>> where K : notnull
    {
        /// <summary> Verifies that the current dictionary is empty, it has a size of 0.</summary>
        public IDictionaryAssert<K, V> IsEmpty();

        /// <summary> Verifies that the current dictionary is not empty, it has a size of minimum 1.</summary>
        public IDictionaryAssert<K, V> IsNotEmpty();

        /// <summary> Verifies that the current dictionary has a size of given value.</summary>
        public IDictionaryAssert<K, V> HasSize(int expected);


        /// <summary> Verifies that the current dictionary contains the given key(s).</summary>
        public IDictionaryAssert<K, V> ContainsKeys(params K[] expected);
        public IDictionaryAssert<K, V> ContainsKeys(IEnumerable expected);

        /// <summary> Verifies that the current dictionary not contains the given key(s).</summary>
        public IDictionaryAssert<K, V> NotContainsKeys(params K[] expected);
        public IDictionaryAssert<K, V> NotContainsKeys(IEnumerable expected);

        /// <summary> Verifies that the current dictionary contains the given key and value.</summary>
        public IDictionaryAssert<K, V> ContainsKeyValue(K key, V value);

        public new IDictionaryAssert<K, V> OverrideFailureMessage(string message);
    }
}
