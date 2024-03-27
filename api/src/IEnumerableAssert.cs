namespace GdUnit4.Asserts;

using System.Collections.Generic;

/// <summary> An Assertion tool to verify enumerates </summary>
public interface IEnumerableAssert<TValue> : IAssertBase<IEnumerable<TValue?>>
{
    /// <summary> Verifies that the current enumerable is equal to the given one, ignoring case considerations.</summary>
    public IEnumerableAssert<TValue?> IsEqualIgnoringCase(IEnumerable<TValue?> expected);

    /// <summary> Verifies that the current enumerable is not equal to the given one, ignoring case considerations.</summary>
    public IEnumerableAssert<TValue?> IsNotEqualIgnoringCase(IEnumerable<TValue?> expected);

    /// <summary> Verifies that the current enumerable is empty, it has a size of 0.</summary>
    public IEnumerableAssert<TValue?> IsEmpty();

    /// <summary> Verifies that the current enumerable is not empty, it has a size of minimum 1.</summary>
    public IEnumerableAssert<TValue?> IsNotEmpty();

    /// <summary>
    /// Verifies that the current enumerable is the same.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="IAssertBase.IsEqual(TValue)"/>
    /// </remarks>
    /// <param name="expected"></param>
    public IEnumerableAssert<TValue?> IsSame(IEnumerable<TValue?> expected);

    /// <summary>
    /// Verifies that the current enumerable is not the same.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="IAssertBase.IsNotEqual(TValue)"/>
    /// </remarks>
    /// <param name="expected"></param>
    public IEnumerableAssert<TValue?> IsNotSame(IEnumerable<TValue?> expected);


    /// <summary> Verifies that the current enumerable has a size of given value.</summary>
    public IEnumerableAssert<TValue?> HasSize(int expected);

    /// <summary> Verifies that the current enumerable contains the given values, in any order.</summary>
    public IEnumerableAssert<TValue?> Contains(params TValue?[] expected);
    public IEnumerableAssert<TValue?> Contains(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> Contains(Godot.Collections.Array expected);

    /// <summary>
    /// Verifies that the current enumerable contains the given values, in any order by object reference equals.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="Contains"/>
    /// </remarks>
    public IEnumerableAssert<TValue?> ContainsSame(params TValue?[] expected);
    public IEnumerableAssert<TValue?> ContainsSame(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSame(Godot.Collections.Array expected);

    /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in same order.</summary>
    public IEnumerableAssert<TValue?> ContainsExactly(params TValue?[] expected);
    public IEnumerableAssert<TValue?> ContainsExactly(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsExactly(Godot.Collections.Array expected);

    /// <summary>
    /// Verifies that the current enumerable contains exactly only the given values and nothing else, in same order by object reference equals.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="ContainsExactly"/>
    /// </remarks>
    public IEnumerableAssert<TValue?> ContainsSameExactly(params TValue?[] expected);
    public IEnumerableAssert<TValue?> ContainsSameExactly(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSameExactly(Godot.Collections.Array expected);

    /// <summary> Verifies that the current enumerable contains exactly only the given values and nothing else, in any order.</summary>
    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(params TValue?[] expected);
    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsExactlyInAnyOrder(Godot.Collections.Array expected);

    /// <summary>
    /// Verifies that the current enumerable contains exactly only the given values and nothing else, in any order by object reference equals.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="ContainsExactlyInAnyOrder"/>
    /// </remarks>
    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(params TValue?[] expected);
    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> ContainsSameExactlyInAnyOrder(Godot.Collections.Array expected);

    /// <summary>
    /// Verifies that the current enumerable do NOT contains the given values, in any order.
    /// </summary>
    public IEnumerableAssert<TValue?> NotContains(params TValue?[] expected);
    public IEnumerableAssert<TValue?> NotContains(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> NotContains(Godot.Collections.Array expected);


    /// <summary>
    /// Verifies that the current enumerable do NOT contains the given values, in any order by object reference equals.
    /// </summary>
    /// <remarks>
    /// Compares the current by object reference equals, for deep comparison use <see cref="NotContains"/>
    /// </remarks>
    public IEnumerableAssert<TValue?> NotContainsSame(params TValue?[] expected);
    public IEnumerableAssert<TValue?> NotContainsSame(IEnumerable<TValue?> expected);
    public IEnumerableAssert<TValue?> NotContainsSame(Godot.Collections.Array expected);
    /// <summary>
    /// Extracts all values by given function name and optional arguments into a new EnumerableAssert<br />
    /// If the elements not accessible by `func_name` the value is converted to `"n.a"`, expecting null values
    /// </summary>
    public IEnumerableAssert<object?> Extract(string funcName, params object[] args);

    /// <summary>
    /// Extracts all values by given extractor's into a new EnumerableAssert<br />
    /// If the elements not extractable than the value is converted to `"n.a"`, expecting null values
    /// </summary>
    public IEnumerableAssert<object?> ExtractV(params IValueExtractor[] extractors);

    public new IEnumerableAssert<TValue?> OverrideFailureMessage(string message);
}
