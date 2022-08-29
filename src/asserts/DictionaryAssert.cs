using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GdUnit3.Asserts
{
    internal sealed class DictionaryAssert : AssertBase<IDictionary>, IDictionaryAssert
    {
        public DictionaryAssert(IDictionary? current) : base(current)
        { }

        public IDictionaryAssert IsEmpty()
        {
            if (Current == null || Current.Count != 0)
                ThrowTestFailureReport(AssertFailures.IsEmpty(Current?.Count ?? 0, Current == null), Current, null);
            return this;
        }

        public IDictionaryAssert IsNotEmpty()
        {
            IsNotNull();
            if (Current?.Count == 0)
                ThrowTestFailureReport(AssertFailures.IsNotEmpty(), Current, null);
            return this;
        }

        public IDictionaryAssert HasSize(int expected)
        {
            IsNotNull();
            if (Current?.Count != expected)
                ThrowTestFailureReport(AssertFailures.HasSize(Current!, expected), Current, expected);
            return this;
        }

        public IDictionaryAssert ContainsKeys(params object[] expected)
        {
            IsNotNull();
            IEnumerable<object> keys = Current?.Keys.Cast<object>().ToList() ?? new List<object>();
            List<object?> notFound = expected.Where(key => !keys.Contains(key)).ToList<object?>();

            if (notFound.Count() > 0)
                ThrowTestFailureReport(AssertFailures.Contains(keys, expected!, notFound), Current, expected);
            return this;
        }
        public IDictionaryAssert ContainsKeys(IEnumerable expected) => ContainsKeys(expected.Cast<object>().ToArray());

        public IDictionaryAssert NotContainsKeys(params object[] expected)
        {
            IsNotNull();
            IEnumerable<object> keys = Current?.Keys.Cast<object>().ToList() ?? new List<object>();
            List<object?> found = expected.Where(key => keys.Contains(key)).ToList<object?>();
            if (found.Count() > 0)
                ThrowTestFailureReport(AssertFailures.NotContains(keys, expected, found), Current, expected);
            return this;
        }

        public IDictionaryAssert NotContainsKeys(IEnumerable expected) => NotContainsKeys(expected.Cast<object>().ToArray());

        public IDictionaryAssert ContainsKeyValue(object key, object value)
        {
            IsNotNull();
            bool hasKey = Current?.Contains(key) ?? false;
            Dictionary<object, object> expectedKeyValue = new Dictionary<object, object>() { { key, value } };
            if (!hasKey)
                ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue), Current, expectedKeyValue);

            var currentValue = Current?[key];
            var result = Comparable.IsEqual(currentValue, value);
            if (!result.Valid)
                ThrowTestFailureReport(AssertFailures.ContainsKeyValue(expectedKeyValue, currentValue), Current, expectedKeyValue);
            return this;
        }

        public new IDictionaryAssert OverrideFailureMessage(string message)
        {
            base.OverrideFailureMessage(message);
            return this;
        }
    }
}
