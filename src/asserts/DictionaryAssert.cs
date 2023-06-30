using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GdUnit4.Asserts
{
    internal sealed class DictionaryAssert<K, V> : AssertBase<IDictionary<K, V>>, IDictionaryAssert<K, V> where K : notnull
    {
        public DictionaryAssert(IDictionary<K, V>? current) : base(current)
        { }

        public IDictionaryAssert<K, V> IsEmpty()
        {
            if (Current == null || Current.Count != 0)
                ThrowTestFailureReport(AssertFailures.IsEmpty(Current?.Count ?? 0, Current == null), Current, null);
            return this;
        }

        public IDictionaryAssert<K, V> IsNotEmpty()
        {
            IsNotNull();
            if (Current?.Count == 0)
                ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
            return this;
        }

        public IDictionaryAssert<K, V> HasSize(int expected)
        {
            IsNotNull();
            if (Current?.Count != expected)
                ThrowTestFailureReport(AssertFailures.HasSize(Current!, expected), Current, expected);
            return this;
        }

        public IDictionaryAssert<K, V> ContainsKeys(params K[] expected)
        {
            IsNotNull();
            IEnumerable<K> keys = Current?.Keys.Cast<K>().ToList() ?? new List<K>();
            List<K> notFound = expected.Where(key => !keys.Contains(key)).ToList<K>();

            if (notFound.Count() > 0)
                ThrowTestFailureReport(AssertFailures.Contains<K>(keys, expected!, notFound), Current, expected);
            return this;
        }
        public IDictionaryAssert<K, V> ContainsKeys(IEnumerable expected) => ContainsKeys(expected.Cast<K>().ToArray());

        public IDictionaryAssert<K, V> NotContainsKeys(params K[] expected)
        {
            IsNotNull();
            IEnumerable<K> keys = Current?.Keys.Cast<K>().ToList() ?? new List<K>();
            List<K> found = expected.Where(key => keys.Contains(key)).ToList<K>();
            if (found.Count() > 0)
                ThrowTestFailureReport(AssertFailures.NotContains<K>(keys, expected, found), Current, expected);
            return this;
        }

        public IDictionaryAssert<K, V> NotContainsKeys(IEnumerable expected) => NotContainsKeys(expected.Cast<K>().ToArray());

        public IDictionaryAssert<K, V> ContainsKeyValue(K key, V value)
        {
            IsNotNull();
            bool hasKey = Current!.ContainsKey(key);
            Dictionary<K, V> expectedKeyValue = new Dictionary<K, V>() { { key, value } };
            if (!hasKey)
                ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue), Current, expectedKeyValue);

            var currentValue = Current[key];
            var result = Comparable.IsEqual(currentValue, value);
            if (!result.Valid)
                ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue, currentValue), Current, expectedKeyValue);
            return this;
        }

        public new IDictionaryAssert<K, V> OverrideFailureMessage(string message)
        {
            base.OverrideFailureMessage(message);
            return this;
        }
    }
}
