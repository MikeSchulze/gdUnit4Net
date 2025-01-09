// ReSharper disable InvalidXmlDocComment

namespace GdUnit4.Asserts;

using System.Collections.Generic;

using Godot.Collections;

/// <summary>
///     An Assertion tool to verify enumerates
/// </summary>
public interface IEnumerableAssert<in TValue> : IAssertBase<IEnumerable<TValue?>>
{
    /// <summary>
    ///     Verifies that the current enumerable is equal to the given one, ignoring case considerations
    /// </summary>
    /// <param name="expected">The expecting enumerable to be equal</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsEqualIgnoringCase(IEnumerable<TValue?> expected);

    /// <summary>
    ///     Verifies that the current enumerable is not equal to the given one, ignoring case considerations.
    /// </summary>
    /// <param name="expected">The expecting enumerable value to be NOT equal</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsNotEqualIgnoringCase(IEnumerable<TValue?> expected);

    /// <summary>
    ///     Verifies that the current enumerable is empty, it has a size of 0.
    /// </summary>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsEmpty();

    /// <summary>
    ///     Verifies that the current enumerable is not empty, it has a size of minimum 1.
    /// </summary>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsNotEmpty();

    /// <summary>
    ///     Verifies that the current enumerable is the same.
    /// </summary>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="IAssertBase{TValue}.IsEqual(TValue)" />
    /// </remarks>
    /// <param name="expected">The value to be the same</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsSame(IEnumerable<TValue?> expected);

    /// <summary>
    ///     Verifies that the current enumerable is not the same.
    /// </summary>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="IAssertBase{TValue}.IsNotEqual(TValue)" />
    /// </remarks>
    /// <param name="expected">The value to be NOT the same</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> IsNotSame(IEnumerable<TValue?> expected);

    /// <summary>
    ///     Verifies that the current enumerable has a size of given value.
    /// </summary>
    /// <param name="expected">The expected size</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> HasSize(int expected);

    /// <summary>
    ///     Verifies that the current enumerable contains the given values, in any order.
    /// </summary>
    /// <param name="expected">The values to be contains</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> Contains(params TValue?[] expected);

    public IEnumerableAssert<TValue?> Contains(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> Contains(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable contains the given values, in any order by object reference equals.
    /// </summary>
    /// <param name="expected">The values to be contains</param>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="Contains" />
    /// </remarks>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> ContainsSame(params TValue?[] expected);

    public IEnumerableAssert<TValue?> ContainsSame(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSame(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable contains exactly only the given values and nothing else, in same order.
    /// </summary>
    /// <param name="expected">The values to be contains exactly</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> ContainsExactly(params TValue?[] expected);

    public IEnumerableAssert<TValue?> ContainsExactly(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsExactly(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable contains exactly only the given values and nothing else, in same order by object reference equals.
    /// </summary>
    /// <param name="expected">The values to be contains exactly</param>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="ContainsExactly" />
    /// </remarks>
    /// ///
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> ContainsSameExactly(params TValue?[] expected);

    public IEnumerableAssert<TValue?> ContainsSameExactly(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSameExactly(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable contains exactly only the given values and nothing else, in any order.
    /// </summary>
    /// <param name="expected">The values to be contains exactly in any order</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(params TValue?[] expected);

    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable contains exactly only the given values and nothing else, in any order by object reference equals.
    /// </summary>
    /// ///
    /// <param name="expected">The values to be contains exactly in any order</param>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="ContainsExactlyInAnyOrder" />
    /// </remarks>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(params TValue?[] expected);

    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable do NOT contain the given values, in any order.
    /// </summary>
    /// <param name="expected">The values to NOT contains</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> NotContains(params TValue?[] expected);

    public IEnumerableAssert<TValue?> NotContains(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> NotContains(Array expected);

    /// <summary>
    ///     Verifies that the current enumerable do NOT contain the given values, in any order by object reference equals.
    /// </summary>
    /// <param name="expected">The values to NOT contains</param>
    /// <remarks>
    ///     Compares the current by object reference equals, for deep comparison use <see cref="NotContains" />
    /// </remarks>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<TValue?> NotContainsSame(params TValue?[] expected);

    public IEnumerableAssert<TValue?> NotContainsSame(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> NotContainsSame(Array expected);

    /// <summary>
    ///     Extracts all values by given function name and optional arguments into a new EnumerableAssert<br />
    ///     If the elements not accessible by `methodName` the value is converted to `"n.a"`, expecting null values
    /// </summary>
    /// <param name="methodName">The name of the method to extract</param>
    /// <param name="args">The method arguments is needed</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<object?> Extract(string methodName, params object[] args);

    /// <summary>
    ///     Extracts all values by given extractor's into a new EnumerableAssert<br />
    ///     If the elements not extractable than the value is converted to `"n.a"`, expecting null values
    /// </summary>
    /// <param name="methodName">A set of value extractors</param>
    /// <returns>IEnumerableAssert</returns>
    public IEnumerableAssert<object?> ExtractV(params IValueExtractor[] extractors);

    public new IEnumerableAssert<TValue?> OverrideFailureMessage(string message);
}
