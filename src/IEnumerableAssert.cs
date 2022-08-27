using System.Collections;

namespace GdUnit3.Asserts
{
    /// <summary> An Assertion tool to verify enumerables </summary>
    public interface IEnumerableAssert : IAssertBase<IEnumerable>
    {
        /// <summary> Verifies that the current enumerable is equal to the given one, ignoring case considerations.</summary>
        public IEnumerableAssert IsEqualIgnoringCase(IEnumerable expected);

        /// <summary> Verifies that the current enumerable is not equal to the given one, ignoring case considerations.</summary>
        public IEnumerableAssert IsNotEqualIgnoringCase(IEnumerable expected);

        /// <summary> Verifies that the current enumerable is empty, it has a size of 0.</summary>
        public IEnumerableAssert IsEmpty();

        /// <summary> Verifies that the current enumerable is not empty, it has a size of minimum 1.</summary>
        public IEnumerableAssert IsNotEmpty();

        /// <summary> Verifies that the current enumerable has a size of given value.</summary>
        public IEnumerableAssert HasSize(int expected);

        /// <summary> Verifies that the current enumerable contains the given values, in any order.</summary>
        public IEnumerableAssert Contains(IEnumerable expected);

        /// <summary> Verifies that the current enumerable contains the given values, in any order.</summary>
        public IEnumerableAssert Contains(params object?[] expected);

        /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in same order.</summary>
        public IEnumerableAssert ContainsExactly(params object?[] expected);

        /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in same order.</summary>
        public IEnumerableAssert ContainsExactly(IEnumerable expected);

        /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in any order.</summary>
        public IEnumerableAssert ContainsExactlyInAnyOrder(params object?[] expected);

        /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in any order.</summary>
        public IEnumerableAssert ContainsExactlyInAnyOrder(IEnumerable expected);

        /// <summary>
        /// Extracts all values by given function name and optional arguments into a new EnumerableAssert<br />
        /// If the elements not accessible by `func_name` the value is converted to `"n.a"`, expecting null values
        /// </summary>
        public IEnumerableAssert Extract(string funcName, params object[] args);

        /// <summary>
        /// Extracts all values by given extractor's into a new EnumerableAssert<br />
        /// If the elements not extractable than the value is converted to `"n.a"`, expecting null values
        /// </summary>
        public IEnumerableAssert ExtractV(params IValueExtractor[] extractors);

        public new IEnumerableAssert OverrideFailureMessage(string message);
    }
}
