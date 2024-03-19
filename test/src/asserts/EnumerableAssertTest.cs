namespace GdUnit4.Tests.Asserts;

using System;

using Godot;
using static Assertions;
using Executions;
using Exceptions;


[TestSuite]
public partial class EnumerableAssertTest
{
    [TestCase]
    public void IsNull()
    {
        AssertArray(null).IsNull();

        // should fail because the current is not null
        AssertThrown(() => AssertArray(Array.Empty<object>()).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(20)
            .HasMessage("""
                    Expecting be <Null>:
                     but is
                        <Empty>
                    """);
        AssertThrown(() => AssertArray(Array.Empty<int>()).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(28)
            .HasMessage("""
                    Expecting be <Null>:
                     but is
                        <Empty>
                    """);
        // with godot array
        AssertThrown(() => AssertArray(new Godot.Collections.Array()).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(37)
            .HasMessage("""
                    Expecting be <Null>:
                     but is
                        <Empty>
                    """);
    }

    [TestCase]
    public void IsNotNull()
    {
        AssertArray(Array.Empty<object>()).IsNotNull();
        AssertArray(Array.Empty<int>()).IsNotNull();
        AssertArray(Array.Empty<int>()).IsNotNull();
        AssertArray(new Godot.Collections.Array()).IsNotNull();

        // should fail because the current is null
        AssertThrown(() => AssertArray(null).IsNotNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(56)
            .HasMessage("Expecting be NOT <Null>:");
    }

    [TestCase]
    public void IsEqual()
    {
        AssertArray(Array.Empty<object>()).IsEqual(Array.Empty<object>());
        AssertArray(Array.Empty<int>()).IsEqual(Array.Empty<int>());
        AssertArray(Array.Empty<int>()).IsEqual(Array.Empty<int>());
        AssertArray(new Godot.Collections.Array()).IsEqual(new Godot.Collections.Array());
        AssertArray(new int[] { 1, 2, 4, 5 }).IsEqual(new int[] { 1, 2, 4, 5 });
        AssertArray(new Godot.Collections.Array(new Variant[] { 1, 2, 4, 5 })).IsEqual(new Godot.Collections.Array(new Variant[] { 1, 2, 4, 5 }));

        AssertThrown(() => AssertArray(new int[] { 1, 2, 4, 5 }).IsEqual(new int[] { 1, 2, 3, 4, 2, 5 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(72)
            .HasMessage("""
                    Expecting be equal:
                        [1, 2, 3, 4, 2, 5]
                     but is
                        [1, 2, 4, 5]
                    """);
        AssertThrown(() => AssertArray(new Godot.Collections.Array(new Variant[] { 1, 2, 4, 5 })).IsEqual(new Godot.Collections.Array(new Variant[] { 1, 2, 3, 4, 2, 5 })))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(81)
            .HasMessage("""
                    Expecting be equal:
                        [1, 2, 3, 4, 2, 5]
                     but is
                        [1, 2, 4, 5]
                    """);
        AssertThrown(() => AssertArray(null).IsEqual(Array.Empty<object>()))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(90)
            .HasMessage("""
                    Expecting be equal:
                        <Empty>
                     but is
                        <Null>
                    """);
    }

    [TestCase]
    public void IsEqualIgnoringCase()
    {
        AssertArray(new string[] { "this", "is", "a", "message" }).IsEqualIgnoringCase(new string[] { "This", "is", "a", "Message" });
        // should fail because the array not contains same elements
        AssertThrown(() => AssertArray(new string[] { "this", "is", "a", "message" })
                .IsEqualIgnoringCase(new string[] { "This", "is", "an", "Message" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(106)
            .HasMessage("""
                    Expecting be equal (ignoring case):
                        ["This", "is", "an", "Message"]
                     but is
                        ["this", "is", "a", "message"]
                    """);
        AssertThrown(() => AssertArray(null)
               .IsEqualIgnoringCase(new string[] { "This", "is" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(116)
            .HasMessage("""
                    Expecting be equal (ignoring case):
                        ["This", "is"]
                     but is
                        <Null>
                    """);
    }

    [TestCase]
    public void IsNotEqual()
    {
        AssertArray(null).IsNotEqual(new int[] { 1, 2, 3, 4, 5 });
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).IsNotEqual(new int[] { 1, 2, 3, 4, 5, 6 });
        // should fail because the array  contains same elements
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).IsNotEqual(new int[] { 1, 2, 3, 4, 5 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(134)
            .HasMessage("""
                    Expecting be NOT equal:
                        [1, 2, 3, 4, 5]
                     but is
                        [1, 2, 3, 4, 5]
                    """);
    }

    [TestCase]
    public void IsNotEqualIgnoringCase()
    {
        AssertArray(null).IsNotEqualIgnoringCase(new string[] { "This", "is", "an", "Message" });
        AssertArray(new string[] { "this", "is", "a", "message" }).IsNotEqualIgnoringCase(new string[] { "This", "is", "an", "Message" });
        // should fail because the array contains same elements ignoring case sensitive
        AssertThrown(() => AssertArray(new string[] { "this", "is", "a", "message" })
                .IsNotEqualIgnoringCase(new string[] { "This", "is", "a", "Message" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(151)
            .HasMessage("""
                    Expecting be NOT equal (ignoring case):
                        ["This", "is", "a", "Message"]
                     but is
                        ["this", "is", "a", "message"]
                    """);
    }

    [TestCase]
    public void IsEmpty()
    {
        AssertArray(Array.Empty<int>()).IsEmpty();
        // should fail because the array is not empty it has a size of one
        AssertThrown(() => AssertArray(new int[] { 1 }).IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(168)
            .HasMessage("""
                    Expecting be empty:
                     but has size '1'
                    """);
        AssertThrown(() => AssertArray(null).IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(175)
            .HasMessage("""
                    Expecting be empty:
                     but is <Null>
                    """);
    }

    [TestCase]
    public void IsNotEmpty()
    {
        AssertArray(null).IsNotEmpty();
        AssertArray(new int[] { 1 }).IsNotEmpty();
        // should fail because the array is empty
        AssertThrown(() => AssertArray(Array.Empty<int>()).IsNotEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(190)
            .HasMessage("""
                    Expecting being NOT empty:
                     but is empty
                    """);
    }

    [TestCase]
    public void HasSize()
    {
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).HasSize(5);
        AssertArray(new string[] { "a", "b", "c", "d", "e", "f" }).HasSize(6);
        // should fail because the array has a size of 5
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).HasSize(4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(205)
            .HasMessage("""
                    Expecting size:
                        '4' but is '5'
                    """);
        AssertThrown(() => AssertArray(null).HasSize(4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(212)
            .HasMessage("""
                    Expecting size:
                        '4' but is unknown
                    """);
    }

    [TestCase]
    public void ContainsStringsAsArray()
    {
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains(Array.Empty<string>());
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains(new string[] { "ddd", "bbb" });
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains(new string[] { "eee", "ddd", "ccc", "bbb", "aaa" });
        // should fail because the array not contains 'xxx' and 'yyy'
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains(new string[] { "bbb", "xxx", "yyy" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(228)
            .HasMessage("""
                    Expecting contains elements:
                        ["aaa", "bbb", "ccc", "ddd", "eee"]
                     do contains (in any order)
                        ["bbb", "xxx", "yyy"]
                     but could not find elements:
                        ["xxx", "yyy"]
                    """);
        AssertThrown(() => AssertArray(null).Contains(new string[] { "bbb", "xxx", "yyy" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(239)
            .HasMessage("""
                    Expecting contains elements:
                        <Null>
                     do contains (in any order)
                        ["bbb", "xxx", "yyy"]
                     but could not find elements:
                        ["bbb", "xxx", "yyy"]
                    """);
    }

    [TestCase]
    public void ContainsStringsAsElements()
    {
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains();
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains("ddd", "bbb");
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains("eee", "ddd", "ccc", "bbb", "aaa");
        // should fail because the array not contains 7 and 6
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).Contains("bbb", "xxx", "yyy"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(259)
            .HasMessage("""
                    Expecting contains elements:
                        ["aaa", "bbb", "ccc", "ddd", "eee"]
                     do contains (in any order)
                        ["bbb", "xxx", "yyy"]
                     but could not find elements:
                        ["xxx", "yyy"]
                    """);
    }

    [TestCase]
    public void ContainsNumbersAsArray()
    {
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(Array.Empty<int>());
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(new int[] { 5, 2 });
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(new int[] { 5, 4, 3, 2, 1 });
        // should fail because the array not contains 7 and 6
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(new int[] { 2, 7, 6 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(279)
            .HasMessage("""
                    Expecting contains elements:
                        [1, 2, 3, 4, 5]
                     do contains (in any order)
                        [2, 7, 6]
                     but could not find elements:
                        [7, 6]
                    """);
    }

    [TestCase]
    public void ContainsNumbersAsElements()
    {
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains();
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(5, 2);
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(5, 4, 3, 2, 1);
        // should fail because the array not contains 7 and 6
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).Contains(2, 7, 6))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(299)
            .HasMessage("""
                    Expecting contains elements:
                        [1, 2, 3, 4, 5]
                     do contains (in any order)
                        [2, 7, 6]
                     but could not find elements:
                        [7, 6]
                    """);
    }

    [TestCase]
    public void ContainsExactlyStringsAsArray()
    {
        // test against only one element
        AssertArray(new string[] { "abc" }).ContainsExactly(new string[] { "abc" });
        AssertThrown(() => AssertArray(new string[] { "abc" }).ContainsExactly(new string[] { "abXc" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(317)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc"]
                     do contains (in same order)
                        ["abXc"]
                     but some elements where not expected:
                        ["abc"]
                     and could not find elements:
                        ["abXc"]
                    """);

        // test against many elements
        AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly(new string[] { "abc", "def", "xyz" });
        // should fail because if contains the same elements but in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly(new string[] { "abc", "xyz", "def" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(334)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "xyz"]
                     do contains (in same order)
                        ["abc", "xyz", "def"]
                     but has different order at position '1'
                        "def" vs "xyz"
                    """);

        // should fail because it contains more elements and in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "foo", "bar", "xyz" }).ContainsExactly(new string[] { "abc", "xyz", "def" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(347)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "foo", "bar", "xyz"]
                     do contains (in same order)
                        ["abc", "xyz", "def"]
                     but some elements where not expected:
                        ["foo", "bar"]
                    """);

        // should fail because it contains less elements and in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly(new string[] { "abc", "def", "bar", "foo", "xyz" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(360)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "xyz"]
                     do contains (in same order)
                        ["abc", "def", "bar", "foo", "xyz"]
                     but could not find elements:
                        ["bar", "foo"]
                    """);
        AssertThrown(() => AssertArray(null).ContainsExactly(new string[] { "abc", "def", "bar", "foo", "xyz" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(371)
            .HasMessage("""
                    Expecting contains exactly elements:
                        <Null>
                     do contains (in same order)
                        ["abc", "def", "bar", "foo", "xyz"]
                     but could not find elements:
                        ["abc", "def", "bar", "foo", "xyz"]
                    """);
    }

    [TestCase]
    public void ContainsExactlyStringsAsElements()
    {
        // test against only one element
        AssertArray(new string[] { "abc" }).ContainsExactly("abc");
        AssertThrown(() => AssertArray(new string[] { "abc" }).ContainsExactly("abXc"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(389)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc"]
                     do contains (in same order)
                        ["abXc"]
                     but some elements where not expected:
                        ["abc"]
                     and could not find elements:
                        ["abXc"]
                    """);
        // test against many elements
        AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly("abc", "def", "xyz");
        // should fail because if contains the same elements but in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly("abc", "xyz", "def"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(405)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "xyz"]
                     do contains (in same order)
                        ["abc", "xyz", "def"]
                     but has different order at position '1'
                        "def" vs "xyz"
                    """);
        // should fail because it contains more elements and in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "foo", "bar", "xyz" }).ContainsExactly("abc", "xyz", "def"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(417)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "foo", "bar", "xyz"]
                     do contains (in same order)
                        ["abc", "xyz", "def"]
                     but some elements where not expected:
                        ["foo", "bar"]
                    """);
        // should fail because it contains less elements and in a different order
        AssertThrown(() => AssertArray(new string[] { "abc", "def", "xyz" }).ContainsExactly("abc", "def", "bar", "foo", "xyz"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(429)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["abc", "def", "xyz"]
                     do contains (in same order)
                        ["abc", "def", "bar", "foo", "xyz"]
                     but could not find elements:
                        ["bar", "foo"]
                    """);
    }

    [TestCase]
    public void ContainsExactlyNumbersAsArray()
    {
        // test against array with only one element
        AssertArray(new int[] { 1 }).ContainsExactly(new int[] { 1 });
        AssertThrown(() => AssertArray(new int[] { 1 }).ContainsExactly(new int[] { 2 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(447)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1]
                     do contains (in same order)
                        [2]
                     but some elements where not expected:
                        [1]
                     and could not find elements:
                        [2]
                    """);
        // test against array with many elements
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(new int[] { 1, 2, 3, 4, 5 });
        // should fail because if contains the same elements but in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(new int[] { 1, 4, 3, 2, 5 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(463)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5]
                     do contains (in same order)
                        [1, 4, 3, 2, 5]
                     but has different order at position '1'
                        '2' vs '4'
                    """);
        // should fail because it contains more elements and in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5, 6, 7 }).ContainsExactly(new int[] { 1, 4, 3, 2, 5 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(475)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5, 6, 7]
                     do contains (in same order)
                        [1, 4, 3, 2, 5]
                     but some elements where not expected:
                        [6, 7]
                    """);
        // should fail because it contains less elements and in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(new int[] { 1, 4, 3, 2, 5, 6, 7 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(487)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5]
                     do contains (in same order)
                        [1, 4, 3, 2, 5, 6, 7]
                     but could not find elements:
                        [6, 7]
                    """);
    }

    [TestCase]
    public void ContainsExactlyNumbersAsElements()
    {
        // test against array with only one element
        AssertArray(new int[] { 1 }).ContainsExactly(1);
        AssertThrown(() => AssertArray(new int[] { 1 }).ContainsExactly(2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(505)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1]
                     do contains (in same order)
                        [2]
                     but some elements where not expected:
                        [1]
                     and could not find elements:
                        [2]
                    """);
        // test against array with many elements
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(1, 2, 3, 4, 5);
        // should fail because if contains the same elements but in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(1, 4, 3, 2, 5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(521)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5]
                     do contains (in same order)
                        [1, 4, 3, 2, 5]
                     but has different order at position '1'
                        '2' vs '4'
                    """);
        // should fail because it contains more elements and in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5, 6, 7 }).ContainsExactly(1, 4, 3, 2, 5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(533)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5, 6, 7]
                     do contains (in same order)
                        [1, 4, 3, 2, 5]
                     but some elements where not expected:
                        [6, 7]
                    """);
        // should fail because it contains less elements and in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactly(1, 4, 3, 2, 5, 6, 7))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(545)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 3, 4, 5]
                     do contains (in same order)
                        [1, 4, 3, 2, 5, 6, 7]
                     but could not find elements:
                        [6, 7]
                    """);
    }

    [TestCase]
    public void ContainsExactlyInAnyOrderStringsAsArray()
    {
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" });
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder(new string[] { "eee", "ddd", "ccc", "bbb", "aaa" });
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder(new string[] { "bbb", "aaa", "ccc", "eee", "ddd" });

        // should fail because is contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd" })
                .ContainsExactlyInAnyOrder(new string[] { "xxx", "aaa", "yyy", "bbb", "ccc", "ddd" }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(566)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["aaa", "bbb", "ccc", "ddd"]
                     do contains (in any order)
                        ["xxx", "aaa", "yyy", "bbb", "ccc", "ddd"]
                     but could not find elements:
                        ["xxx", "yyy"]
                    """);
        //should fail because is contains the same elements but in a different order
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee", "fff" })
                .ContainsExactlyInAnyOrder(new string[] { "fff", "aaa", "ddd", "bbb", "eee", }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(579)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["aaa", "bbb", "ccc", "ddd", "eee", "fff"]
                     do contains (in any order)
                        ["fff", "aaa", "ddd", "bbb", "eee"]
                     but some elements where not expected:
                        ["ccc"]
                    """);
        AssertThrown(() => AssertArray(null)
                .ContainsExactlyInAnyOrder(new string[] { "fff", "aaa", "ddd", "bbb", "eee", }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(591)
            .HasMessage("""
                    Expecting contains exactly elements:
                        <Null>
                     do contains (in any order)
                        ["fff", "aaa", "ddd", "bbb", "eee"]
                     but could not find elements:
                        ["fff", "aaa", "ddd", "bbb", "eee"]
                    """);
    }

    [TestCase]
    public void ContainsExactlyInAnyOrderStringsAsElements()
    {
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder("aaa", "bbb", "ccc", "ddd", "eee");
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder("eee", "ddd", "ccc", "bbb", "aaa");
        AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee" }).ContainsExactlyInAnyOrder("bbb", "aaa", "ccc", "eee", "ddd");

        // should fail because is contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd" })
                .ContainsExactlyInAnyOrder("xxx", "aaa", "yyy", "bbb", "ccc", "ddd"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(613)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["aaa", "bbb", "ccc", "ddd"]
                     do contains (in any order)
                        ["xxx", "aaa", "yyy", "bbb", "ccc", "ddd"]
                     but could not find elements:
                        ["xxx", "yyy"]
                    """);
        //should fail because is contains the same elements but in a different order
        AssertThrown(() => AssertArray(new string[] { "aaa", "bbb", "ccc", "ddd", "eee", "fff" })
                .ContainsExactlyInAnyOrder("fff", "aaa", "ddd", "bbb", "eee"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(626)
            .HasMessage("""
                    Expecting contains exactly elements:
                        ["aaa", "bbb", "ccc", "ddd", "eee", "fff"]
                     do contains (in any order)
                        ["fff", "aaa", "ddd", "bbb", "eee"]
                     but some elements where not expected:
                        ["ccc"]
                    """);
    }

    [TestCase]
    public void ContainsExactlyInAnyOrderNumbersAsArray()
    {
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(new int[] { 1, 2, 3, 4, 5 });
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(new int[] { 5, 3, 2, 4, 1 });
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(new int[] { 5, 1, 2, 4, 3 });

        // should fail because is contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 6, 4, 5 }).ContainsExactlyInAnyOrder(new int[] { 5, 3, 2, 4, 1, 9, 10 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(648)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 6, 4, 5]
                     do contains (in any order)
                        [5, 3, 2, 4, 1, 9, 10]
                     but some elements where not expected:
                        [6]
                     and could not find elements:
                        [3, 9, 10]
                    """);
        //should fail because is contains the same elements but in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 6, 9, 10, 4, 5 }).ContainsExactlyInAnyOrder(new int[] { 5, 3, 2, 4, 1 }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(662)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 6, 9, 10, 4, 5]
                     do contains (in any order)
                        [5, 3, 2, 4, 1]
                     but some elements where not expected:
                        [6, 9, 10]
                     and could not find elements:
                        [3]
                    """);
    }

    [TestCase]
    public void ContainsExactlyInAnyOrderNumbersAsElements()
    {
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(1, 2, 3, 4, 5);
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(5, 3, 2, 4, 1);
        AssertArray(new int[] { 1, 2, 3, 4, 5 }).ContainsExactlyInAnyOrder(5, 1, 2, 4, 3);

        // should fail because is contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 6, 4, 5 }).ContainsExactlyInAnyOrder(5, 3, 2, 4, 1, 9, 10))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(685)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 6, 4, 5]
                     do contains (in any order)
                        [5, 3, 2, 4, 1, 9, 10]
                     but some elements where not expected:
                        [6]
                     and could not find elements:
                        [3, 9, 10]
                    """);
        //should fail because is contains the same elements but in a different order
        AssertThrown(() => AssertArray(new int[] { 1, 2, 6, 9, 10, 4, 5 }).ContainsExactlyInAnyOrder(5, 3, 2, 4, 1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(699)
            .HasMessage("""
                    Expecting contains exactly elements:
                        [1, 2, 6, 9, 10, 4, 5]
                     do contains (in any order)
                        [5, 3, 2, 4, 1]
                     but some elements where not expected:
                        [6, 9, 10]
                     and could not find elements:
                        [3]
                    """);
    }

    [TestCase]
    public void Fluent()
        => AssertArray(Array.Empty<int>())
            .Contains(Array.Empty<int>())
            .ContainsExactly(Array.Empty<int>())
            .HasSize(0)
            .IsEmpty()
            .IsNotNull();

    [TestCase]
    public void Extract()
    {
        // try to extract on base types
        AssertArray(new object?[] { 1, false, 3.14, null, Colors.AliceBlue }).Extract("GetClass")
            .ContainsExactly("n.a.", "n.a.", "n.a.", null, "n.a.");
        // extracting by a func without arguments
        AssertArray(new object[] { new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())! }).Extract("GetClass")
            .ContainsExactly("RefCounted", "n.a.", "AStarGrid2D", "Node");
        // extracting by a func with arguments
        AssertArray(new object[] { new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())! }).Extract("HasSignal", new object[] { "tree_entered" })
            .ContainsExactly(false, "n.a.", false, true);

        // try extract on object via a func that not exists
        AssertArray(new object[] { new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())! }).Extract("InvalidMethod")
            .ContainsExactly("n.a.", "n.a.", "n.a.", "n.a.");
        // try extract on object via a func that has no return value
        AssertArray(new object[] { new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())! }).Extract("RemoveMeta", new object[] { "" })
            .ContainsExactly(null, "n.a.", null, null);
        // must fail we can't extract from a null instance
        AssertThrown(() => AssertArray(null).Extract("GetClass").ContainsExactly("AStar", "Node"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(743)
            .HasMessage("""
                    Expecting contains exactly elements:
                        <Null>
                     do contains (in same order)
                        ["AStar", "Node"]
                     but could not find elements:
                        ["AStar", "Node"]
                    """);
    }

    private sealed partial class TestObj : RefCounted
    {
        private string name;
        private readonly object? value;
        private readonly object? x;

        public TestObj(string name, object? value, object? x = null)
        {
            this.name = name;
            this.value = value;
            this.x = x;
        }

        public string GetName() => name;
        public object? GetValue() => value;
        public object? GetX() => x;
        public string GetX1() => "x1";
        public string GetX2() => "x2";
        public string GetX3() => "x3";
        public string GetX4() => "x4";
        public string GetX5() => "x5";
        public string GetX6() => "x6";
        public string GetX7() => "x7";
        public string GetX8() => "x8";
        public string GetX9() => "x9";
    }

    [TestCase]
    public void ExtractV()
    {
        // single extract
        AssertArray(new object?[] { 1, false, 3.14, null, Colors.AliceBlue })
            .ExtractV(Extr("GetClass"))
            .ContainsExactly("n.a.", "n.a.", "n.a.", null, "n.a.");
        // tuple of two
        AssertArray(new object[] { new TestObj("A", 10), new TestObj("B", "foo"), Colors.AliceBlue, new TestObj("C", 11) })
            .ExtractV(Extr("GetName"), Extr("GetValue"))
            .ContainsExactly(Tuple("A", 10), Tuple("B", "foo"), Tuple("n.a.", "n.a."), Tuple("C", 11));
        // tuple of three
        AssertArray(new object[] { new TestObj("A", 10), new TestObj("B", "foo", "bar"), new TestObj("C", 11, 42) })
            .ExtractV(Extr("GetName"), Extr("GetValue"), Extr("GetX"))
            .ContainsExactly(Tuple("A", 10, null), Tuple("B", "foo", "bar"), Tuple("C", 11, 42));
        AssertThrown(() => AssertArray(null)
                .ExtractV(Extr("GetName"), Extr("GetValue"), Extr("GetX"))
                .ContainsExactly(Tuple("A", 10, null)))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(798)
            .HasMessage("""
                    Expecting contains exactly elements:
                        <Null>
                     do contains (in same order)
                        [tuple("A", 10, <Null>)]
                     but could not find elements:
                        [tuple("A", 10, <Null>)]
                    """);
    }

    [TestCase]
    public void ExtractVChained()
    {
        var root_a = new TestObj("root_a", null);
        var obj_a = new TestObj("A", root_a);
        var obj_b = new TestObj("B", root_a);
        var obj_c = new TestObj("C", root_a);
        var root_b = new TestObj("root_b", root_a);
        var obj_x = new TestObj("X", root_b);
        var obj_y = new TestObj("Y", root_b);

        AssertArray(new object[] { obj_a, obj_b, obj_c, obj_x, obj_y })
            .ExtractV(Extr("GetName"), Extr("GetValue.GetName"))
            .ContainsExactly(
                Tuple("A", "root_a"),
                Tuple("B", "root_a"),
                Tuple("C", "root_a"),
                Tuple("X", "root_b"),
                Tuple("Y", "root_b")
            );
    }

    [TestCase]
    public void ExtractChained()
    {
        var root_a = new TestObj("root_a", null);
        var obj_a = new TestObj("A", root_a);
        var obj_b = new TestObj("B", root_a);
        var obj_c = new TestObj("C", root_a);
        var root_b = new TestObj("root_b", root_a);
        var obj_x = new TestObj("X", root_b);
        var obj_y = new TestObj("Y", root_b);

        AssertArray(new object[] { obj_a, obj_b, obj_c, obj_x, obj_y })
            .Extract("GetValue.GetName")
            .ContainsExactly(
                "root_a",
                "root_a",
                "root_a",
                "root_b",
                "root_b"
            );
    }

    [TestCase]
    public void ExtractInvalidMethod()
        => AssertArray(new object[] { "abc" })
            .Extract("NotExistMethod")
            .ContainsExactly("n.a.");

    [TestCase]
    public void ExtractVManyArgs()
        => AssertArray(new object[] { new TestObj("A", 10), new TestObj("B", "foo", "bar"), new TestObj("C", 11, 42) })
            .ExtractV(
                Extr("GetName"),
                Extr("GetX1"),
                Extr("GetX2"),
                Extr("GetX3"),
                Extr("GetX4"),
                Extr("GetX5"),
                Extr("GetX6"),
                Extr("GetX7"),
                Extr("GetX8"),
                Extr("GetX9"))
            .ContainsExactly(
                Tuple("A", "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8", "x9"),
                Tuple("B", "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8", "x9"),
                Tuple("C", "x1", "x2", "x3", "x4", "x5", "x6", "x7", "x8", "x9"));

    [TestCase]
    public void OverrideFailureMessage()
        => AssertThrown(() => AssertArray(Array.Empty<object>())
                .OverrideFailureMessage("Custom failure message")
                .IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(884)
            .HasMessage("Custom failure message");

    [TestCase]
    public void InterruptIsFailure()
    {
        // we disable failure reporting until we simulate an failure
        if (ExecutionContext.Current != null)
            ExecutionContext.Current.FailureReporting = false;
        // try to fail
        AssertArray(Array.Empty<object>()).IsNotEmpty();

        // expect this line will never called because of the test is interrupted by a failing assert
        AssertBool(true).OverrideFailureMessage("This line should never be called").IsFalse();
    }
}
