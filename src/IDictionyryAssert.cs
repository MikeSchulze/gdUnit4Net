using System.Collections;

namespace GdUnit3.Asserts
{
    /// <summary> An Assertion Tool to verify dictionary values </summary>
    public interface IDictionaryAssert : IAssertBase<IDictionary>
    {
        /// <summary> Verifies that the current dictionary is empty, it has a size of 0.</summary>
        public IDictionaryAssert IsEmpty();

        /// <summary> Verifies that the current dictionary is not empty, it has a size of minimum 1.</summary>
        public IDictionaryAssert IsNotEmpty();

        /// <summary> Verifies that the current dictionary has a size of given value.</summary>
        public IDictionaryAssert HasSize(int expected);


        /// <summary> Verifies that the current dictionary contains the given key(s).</summary>
        public IDictionaryAssert ContainsKeys(params object[] expected);
        public IDictionaryAssert ContainsKeys(IEnumerable expected);

        /// <summary> Verifies that the current dictionary not contains the given key(s).</summary>
        public IDictionaryAssert NotContainsKeys(params object[] expected);
        public IDictionaryAssert NotContainsKeys(IEnumerable expected);

        /// <summary> Verifies that the current dictionary contains the given key and value.</summary>
        public IDictionaryAssert ContainsKeyValue(object key, object value);

        public new IDictionaryAssert OverrideFailureMessage(string message);
    }
}
