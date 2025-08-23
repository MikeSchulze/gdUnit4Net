namespace GdUnit4.Tests.Asserts;

using System.Collections.Generic;

using GdUnit4.Asserts;
using GdUnit4.Core.Execution;
using GdUnit4.Core.Execution.Exceptions;

using Godot;
using Godot.Collections;

using static Assertions;

using Array = System.Array;

[TestSuite]
public partial class EnumerableAssertTest
{
    // TODO: replace it by https://github.com/MikeSchulze/gdUnit4Net/issues/46
    private static readonly object[] TestDataPointEmptyArrays = [Array.Empty<object>(), new List<object>(), new Godot.Collections.Array(), new Array<RefCounted>()];

    private static readonly object[] TestDataPointStringValues =
    [
        new object[] { new[] { "a", "b", "c", "a" }, new[] { "a", "b", "c", "a" }, "X" }, new object[]
        {
            new List<string>
            {
                "a",
                "b",
                "c",
                "a"
            },
            new List<string>
            {
                "a",
                "b",
                "c",
                "a"
            },
            "X"
        },
        new object[]
        {
            new Godot.Collections.Array
            {
                "a",
                "b",
                "c",
                "a"
            },
            new Godot.Collections.Array
            {
                "a",
                "b",
                "c",
                "a"
            },
            "X"
        },
        new object[]
        {
            new Array<string>
            {
                "a",
                "b",
                "c",
                "a"
            },
            new Array<string>
            {
                "a",
                "b",
                "c",
                "a"
            },
            "X"
        }
    ];

    public static readonly object[] TestDataPointObjectValues =
    [
        new object[] { new object[] { new(), new(), new(), new() }, new object[] { new(), new(), new(), new() }, new() }, new object[]
        {
            new List<object>
            {
                new(),
                new(),
                new(),
                new()
            },
            new List<object>
            {
                new(),
                new(),
                new(),
                new()
            },
            new()
        },
        new object[]
        {
            new Godot.Collections.Array
            {
                new RDUniform { Binding = 0 },
                new RDUniform { Binding = 1 },
                new RDUniform { Binding = 2 },
                new RDUniform { Binding = 3 }
            },
            new Godot.Collections.Array
            {
                new RDUniform { Binding = 0 },
                new RDUniform { Binding = 1 },
                new RDUniform { Binding = 2 },
                new RDUniform { Binding = 3 }
            },
            new RDUniform { Binding = 10 }
        },
        new object[]
        {
            new Array<GodotObject>
            {
                new RDUniform { Binding = 0 },
                new RDUniform { Binding = 1 },
                new RDUniform { Binding = 2 },
                new RDUniform { Binding = 3 }
            },
            new Array<GodotObject>
            {
                new RDUniform { Binding = 0 },
                new RDUniform { Binding = 1 },
                new RDUniform { Binding = 2 },
                new RDUniform { Binding = 3 }
            },
            new RDUniform { Binding = 10 }
        }
    ];

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void IsNull(int dataIndex)
    {
        dynamic current = TestDataPointEmptyArrays[dataIndex];
        AssertArray(null).IsNull();

        // should fail because the current is not null
        AssertThrown(() => AssertArray(current).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(149)
            .HasMessage("""
                        Expecting be <Null>:
                         but is
                            <Empty>
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void IsNotNull(int dataIndex)
    {
        dynamic current = TestDataPointEmptyArrays[dataIndex];

        AssertArray(current).IsNotNull();
        // should fail because the current is null
        AssertThrown(() => AssertArray(null).IsNotNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(170)
            .HasMessage("Expecting be NOT <Null>:");
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsEqual()
    {
        AssertArray([]).IsEqual([]);
        AssertArray(Array.Empty<int>()).IsEqual([]);
        AssertArray(new[] { 1, 2, 4, 5 }).IsEqual([1, 2, 4, 5]);
        AssertArray(new Godot.Collections.Array()).IsEqual(new Godot.Collections.Array());
        AssertArray(new Array<Variant>()).IsEqual(new Array<Variant>());
        AssertArray(new Godot.Collections.Array
        {
            1,
            2,
            3,
            4
        }).IsEqual(new Godot.Collections.Array
        {
            1,
            2,
            3,
            4
        });
        AssertArray(new Array<Variant>
        {
            1,
            2,
            4,
            5
        }).IsEqual(new Array<Variant>
        {
            1,
            2,
            4,
            5
        });

        AssertThrown(() => AssertArray(new[] { 1, 2, 4, 5 }).IsEqual([1, 2, 3, 4, 2, 5]))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(212)
            .HasMessage("""
                        Expecting be equal:
                            [1, 2, 3, 4, 2, 5]
                         but is
                            [1, 2, 4, 5]
                        """);
        AssertThrown(() => AssertArray(new Array<Variant>
            {
                1,
                2,
                4,
                5
            }).IsEqual(new Array<Variant>
            {
                1,
                2,
                3,
                4,
                2,
                5
            }))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(221)
            .HasMessage("""
                        Expecting be equal:
                            [1, 2, 3, 4, 2, 5]
                         but is
                            [1, 2, 4, 5]
                        """);
        AssertThrown(() => AssertArray<object>(null).IsEqual([]))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(244)
            .HasMessage("""
                        Expecting be equal:
                            <Empty>
                         but is
                            <Null>
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsEqualIgnoringCase()
    {
        AssertArray(["this", "is", "a", "message"])
            .IsEqualIgnoringCase(["This", "is", "a", "Message"]);
        AssertArray(new List<string>
            {
                "this",
                "is",
                "a",
                "message"
            })
            .IsEqualIgnoringCase(new List<string>
            {
                "This",
                "is",
                "a",
                "Message"
            });
        AssertArray(new Godot.Collections.Array
            {
                "this",
                "is",
                "a",
                "message"
            })
            .IsEqualIgnoringCase(new Godot.Collections.Array
            {
                "This",
                "is",
                "a",
                "Message"
            });
        AssertArray(new Array<string>
            {
                "this",
                "is",
                "a",
                "message"
            })
            .IsEqualIgnoringCase(new Array<string>
            {
                "This",
                "is",
                "a",
                "Message"
            });
        // should fail because the array not contains same elements
        AssertThrown(() => AssertArray(["this", "is", "a", "message"])
                .IsEqualIgnoringCase(["This", "is", "an", "Message"]))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(304)
            .HasMessage("""
                        Expecting be equal (ignoring case):
                            ["This", "is", "an", "Message"]
                         but is
                            ["this", "is", "a", "message"]
                        """);
        AssertThrown(() => AssertArray(new Godot.Collections.Array
                {
                    "this",
                    "is",
                    "a",
                    "message"
                })
                .IsEqualIgnoringCase(new Godot.Collections.Array
                {
                    "This",
                    "is",
                    "an",
                    "Message"
                }))
            .IsInstanceOf<TestFailedException>()
            .HasMessage("""
                        Expecting be equal (ignoring case):
                            ["This", "is", "an", "Message"]
                         but is
                            ["this", "is", "a", "message"]
                        """);
        AssertThrown(() => AssertArray<object>(null)
                .IsEqualIgnoringCase(["This", "is"]))
            .IsInstanceOf<TestFailedException>()
            .HasMessage("""
                        Expecting be equal (ignoring case):
                            ["This", "is"]
                         but is
                            <Null>
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsNotEqual()
    {
        AssertArray<int>(null).IsNotEqual([1, 2, 3, 4, 5]);
        AssertArray(new[] { 1, 2, 3, 4, 5 }).IsNotEqual([1, 2, 3, 4, 5, 6]);
        AssertArray(new List<int>
        {
            1,
            2,
            3,
            4,
            5
        }).IsNotEqual(new List<int>
        {
            1,
            2,
            3,
            4,
            5,
            6
        });
        AssertArray(new Godot.Collections.Array
        {
            1,
            2,
            3,
            4,
            5
        }).IsNotEqual(new Godot.Collections.Array
        {
            1,
            2,
            3,
            4,
            5,
            6
        });
        AssertArray(new Array<int>
        {
            1,
            2,
            3,
            4,
            5
        }).IsNotEqual(new Array<int>
        {
            1,
            2,
            3,
            4,
            5,
            6
        });
        // should fail because the array  contains same elements
        AssertThrown(() => AssertArray(new[] { 1, 2, 3, 4, 5 }).IsNotEqual([1, 2, 3, 4, 5]))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(401)
            .HasMessage("""
                        Expecting be NOT equal:
                            [1, 2, 3, 4, 5]
                         but is
                            [1, 2, 3, 4, 5]
                        """);
        AssertThrown(() => AssertArray(new Godot.Collections.Array
            {
                1,
                2,
                3,
                4,
                5
            }).IsNotEqual(new Godot.Collections.Array
            {
                1,
                2,
                3,
                4,
                5
            }))
            .IsInstanceOf<TestFailedException>()
            .HasMessage("""
                        Expecting be NOT equal:
                            [1, 2, 3, 4, 5]
                         but is
                            [1, 2, 3, 4, 5]
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsNotEqualIgnoringCase()
    {
        AssertArray(null).IsNotEqualIgnoringCase(["This", "is", "an", "Message"]);
        AssertArray(["this", "is", "a", "message"]).IsNotEqualIgnoringCase(["This", "is", "an", "Message"]);
        AssertArray(new List<string>
        {
            "this",
            "is",
            "a",
            "message"
        }).IsNotEqualIgnoringCase(new List<string>
        {
            "This",
            "is",
            "an",
            "Message"
        });
        AssertArray(new Godot.Collections.Array
        {
            "this",
            "is",
            "a",
            "message"
        }).IsNotEqualIgnoringCase(new Godot.Collections.Array
        {
            "This",
            "is",
            "an",
            "Message"
        });
        AssertArray(new Array<string>
        {
            "this",
            "is",
            "a",
            "message"
        }).IsNotEqualIgnoringCase(new Array<string>
        {
            "This",
            "is",
            "an",
            "Message"
        });
        // should fail because the array contains same elements ignoring case-sensitive
        AssertThrown(() => AssertArray(["this", "is", "a", "message"])
                .IsNotEqualIgnoringCase(["This", "is", "a", "Message"]))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(480)
            .HasMessage("""
                        Expecting be NOT equal (ignoring case):
                            ["This", "is", "a", "Message"]
                         but is
                            ["this", "is", "a", "message"]
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void IsEmpty(int dataIndex)
    {
        dynamic empty = TestDataPointEmptyArrays[dataIndex];
        dynamic? expected = TestDataPointStringValues[dataIndex] as object[];
        var filled = expected![0];

        AssertArray(empty).IsEmpty();
        // should fail because the array is not empty it has a size of one
        AssertThrown(() => AssertArray(filled).IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(505)
            .HasMessage("""
                        Expecting be empty:
                         but has size '4'
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void IsNotEmpty(int dataIndex)
    {
        dynamic empty = TestDataPointEmptyArrays[dataIndex];
        dynamic? expected = TestDataPointStringValues[dataIndex] as object[];
        var filled = expected![0];

        AssertArray(null).IsNotEmpty();
        AssertArray(filled).IsNotEmpty();
        // should fail because the array is empty
        AssertThrown(() => AssertArray(empty).IsNotEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(528)
            .HasMessage("""
                        Expecting being NOT empty:
                         but is empty
                        """);
    }


    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void HasSize(int dataIndex)
    {
        dynamic? expected = TestDataPointStringValues[dataIndex] as object[];
        var current = expected![0];

        AssertArray(current).HasSize(4);
        // should fail because the array has a size of 4
        AssertThrown(() => AssertArray(current).HasSize(5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(550)
            .HasMessage("""
                        Expecting size:
                            '5' but is '4'
                        """);
        AssertThrown(() => AssertArray(null).HasSize(4))
            .IsInstanceOf<TestFailedException>()
            .HasMessage("""
                        Expecting size:
                            '4' but is unknown
                        """);
    }

    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsOnStrings(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).Contains(obj1);
        AssertArray(current).Contains(obj2);
        AssertArray(current).Contains(obj2, obj3);
        AssertArray(current).Contains(obj1, obj2, obj3, obj4);
        AssertArray(current).Contains(obj4, obj1, obj3, obj2);
        AssertArray(current).Contains(current);
        AssertThrown(() => AssertArray(current).Contains(obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(587)
            .HasMessage("""
                        Expecting contains elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["X"]
                         but could not find elements:
                            ["X"]
                        """);

        AssertThrown(() => AssertArray(current).Contains(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(599)
            .HasMessage("""
                        Expecting contains elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "b", "X"]
                         but could not find elements:
                            ["X"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsOnObjects(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).Contains(obj1);
        AssertArray(current).Contains(obj2);
        AssertArray(current).Contains(obj2, obj3);
        AssertArray(current).Contains(obj1, obj2, obj3, obj4);
        AssertArray(current).Contains(obj4, obj1, obj3, obj2);
        AssertArray(current).Contains(current);
        AssertThrown(() => AssertArray(current).Contains(obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(634)
            .HasMessage("""
                Expecting contains elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj5]
                 but could not find elements:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));

        AssertThrown(() => AssertArray(current).Contains(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(651)
            .HasMessage("""
                Expecting contains elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj2, $obj5]
                 but could not find elements:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsExactlyOnStrings(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).ContainsExactly(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsExactly(obj1, obj2, obj3, obj1);
        AssertArray(current).ContainsExactly(current);
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(688)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a"]
                         but others where not expected:
                            ["b", "c", "a"]
                        """);
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(699)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a", "b"]
                         but others where not expected:
                            ["c", "a"]
                        """);
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(710)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a", "X"]
                         but others where not expected:
                            ["b", "c", "a"]
                         and some elements not found:
                            ["X"]
                        """);
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj3, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(723)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a", "c", "b", "a"]
                         but there has differences in order:
                            - element at index 0 expect "c" but is "b"
                            - element at index 1 expect "b" but is "c"
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsExactlyOnObjects(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).ContainsExactly(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsExactly(current);
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(755)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1]
                 but others where not expected:
                    [$obj2, $obj3, $obj4]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4)));
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(770)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj2]
                 but others where not expected:
                    [$obj3, $obj4]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4)));
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(785)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj5]
                 but others where not expected:
                    [$obj2, $obj3, $obj4]
                 and some elements not found:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
        AssertThrown(() => AssertArray(current).ContainsExactly(obj1, obj3, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(803)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj3, $obj2, $obj4]
                 but there has differences in order:
                    - element at index 0 expect $obj3 but is $obj2
                    - element at index 1 expect $obj2 but is $obj3
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsExactlyInAnyOrderOnString(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsExactlyInAnyOrder(current);
        AssertArray(current).ContainsExactlyInAnyOrder(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsExactlyInAnyOrder(obj4, obj2, obj3, obj1);
        AssertArray(current).ContainsExactlyInAnyOrder(obj2, obj4, obj3, obj1);
        if (obj1 is Variant)
        {
            AssertArray(current).ContainsExactlyInAnyOrder(new Variant[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsExactlyInAnyOrder(new Variant[] { obj4, obj2, obj3, obj1 });
        }
        else
        {
            AssertArray(current).ContainsExactlyInAnyOrder(new string[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsExactlyInAnyOrder(new string[] { obj4, obj2, obj3, obj1 });
        }

        // should fail because it contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(current)
                .ContainsExactlyInAnyOrder(obj1, obj2, obj5, obj3, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(853)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "b", "X", "c", "a"]
                         but could not find elements:
                            ["X"]
                        """);
        //should fail because it contains not all elements
        AssertThrown(() => AssertArray(current)
                .ContainsExactlyInAnyOrder(obj1, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(866)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "b", "a"]
                         but some elements where not expected:
                            ["c"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsExactlyInAnyOrderOnObject(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsExactlyInAnyOrder(current);
        AssertArray(current).ContainsExactlyInAnyOrder(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsExactlyInAnyOrder(obj4, obj2, obj3, obj1);
        AssertArray(current).ContainsExactlyInAnyOrder(obj2, obj4, obj3, obj1);
        if (obj1 is Variant)
        {
            AssertArray(current).ContainsExactlyInAnyOrder(new Variant[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsExactlyInAnyOrder(new Variant[] { obj4, obj2, obj3, obj1 });
        }
        else if (obj1 is RefCounted)
        {
            AssertArray(current).ContainsExactlyInAnyOrder(new RefCounted[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsExactlyInAnyOrder(new RefCounted[] { obj4, obj2, obj3, obj1 });
        }
        else
        {
            AssertArray(current).ContainsExactlyInAnyOrder(new object[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsExactlyInAnyOrder(new object[] { obj4, obj2, obj3, obj1 });
        }

        // should fail because it contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(current)
                .ContainsExactlyInAnyOrder(obj1, obj2, obj5, obj3, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(916)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj2, $obj5, $obj3, $obj4]
                 but could not find elements:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
        //should fail because it contains not all elements
        AssertThrown(() => AssertArray(current)
                .ContainsExactlyInAnyOrder(obj1, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(934)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj2, $obj4]
                 but some elements where not expected:
                    [$obj3]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }


    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsSameExactlyInAnyOrderOnString(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsSameExactlyInAnyOrder(current);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj4, obj2, obj3, obj1);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj2, obj4, obj3, obj1);
        if (obj1 is Variant)
        {
            AssertArray(current).ContainsSameExactlyInAnyOrder(new Variant[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsSameExactlyInAnyOrder(new Variant[] { obj4, obj2, obj3, obj1 });
        }
        else
        {
            AssertArray(current).ContainsSameExactlyInAnyOrder(new string[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsSameExactlyInAnyOrder(new string[] { obj4, obj2, obj3, obj1 });
        }

        // should fail because it contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(current)
                .ContainsSameExactlyInAnyOrder(obj1, obj2, obj5, obj3, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(985)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "b", "X", "c", "a"]
                         but could not find elements:
                            ["X"]
                        """);
        //should fail because it contains not all elements
        AssertThrown(() => AssertArray(current)
                .ContainsSameExactlyInAnyOrder(obj1, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(998)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "b", "a"]
                         but some elements where not expected:
                            ["c"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsSameExactlyInAnyOrderOnObject(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsSameExactlyInAnyOrder(current);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj4, obj2, obj3, obj1);
        AssertArray(current).ContainsSameExactlyInAnyOrder(obj2, obj4, obj3, obj1);
        if (obj1 is Variant)
        {
            AssertArray(current).ContainsSameExactlyInAnyOrder(new Variant[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsSameExactlyInAnyOrder(new Variant[] { obj4, obj2, obj3, obj1 });
        }
        else if (obj1 is RefCounted)
        {
            AssertArray(current).ContainsSameExactlyInAnyOrder(new RefCounted[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsSameExactlyInAnyOrder(new RefCounted[] { obj4, obj2, obj3, obj1 });
        }
        else
        {
            AssertArray(current).ContainsSameExactlyInAnyOrder(new object[] { obj1, obj2, obj3, obj4 });
            AssertArray(current).ContainsSameExactlyInAnyOrder(new object[] { obj4, obj2, obj3, obj1 });
        }

        // should fail because it contains not exactly the same elements in any order
        AssertThrown(() => AssertArray(current)
                .ContainsSameExactlyInAnyOrder(obj1, obj2, obj5, obj3, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1048)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj2, $obj5, $obj3, $obj4]
                 but could not find elements:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
        //should fail because it contains not all elements
        AssertThrown(() => AssertArray(current)
                .ContainsSameExactlyInAnyOrder(obj1, obj2, obj4))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1066)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj2, $obj4]
                 but some elements where not expected:
                    [$obj3]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase]
    public void Fluent()
        => AssertArray(Array.Empty<int>())
            .Contains()
            .ContainsExactly()
            .HasSize(0)
            .IsEmpty()
            .IsNotNull();

    [TestCase]
    [RequireGodotRuntime]
    public void Extract()
    {
        // try to extract on base types
        AssertArray([1, false, 3.14, null, Colors.AliceBlue]).Extract("GetClass")
            .ContainsExactly("n.a.", "n.a.", "n.a.", null, "n.a.");
        // extracting by a func without arguments
        AssertArray([new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())]).Extract("GetClass")
            .ContainsExactly("RefCounted", "n.a.", "AStarGrid2D", "Node");
        // extracting by a func with arguments
        AssertArray([new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())]).Extract("HasSignal", "tree_entered")
            .ContainsExactly(false, "n.a.", false, true);

        // try extract on object via a func that not exists
        AssertArray([new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())]).Extract("InvalidMethod")
            .ContainsExactly("n.a.", "n.a.", "n.a.", "n.a.");
        // try extract on object via a func that has no return value
        AssertArray([new RefCounted(), 2, new AStarGrid2D(), AutoFree(new Node())]).Extract("RemoveMeta", "")
            .ContainsExactly(null, "n.a.", null, null);
        // must fail we can't extract from a null instance
        AssertThrown(() => AssertArray(null).Extract("GetClass").ContainsExactly("AStar", "Node"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1115)
            .HasMessage("""
                        Expecting contains exactly elements:
                            <Null>
                         do contains (in same order)
                            ["AStar", "Node"]
                         but some elements not found:
                            ["AStar", "Node"]
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void ExtractV()
    {
        // single extract
        AssertArray([1, false, 3.14, null, Colors.AliceBlue])
            .ExtractV(Extr("GetClass"))
            .ContainsExactly("n.a.", "n.a.", "n.a.", null, "n.a.");
        // tuple of two
        AssertArray([new TestObj("A", 10), new TestObj("B", "foo"), Colors.AliceBlue, new TestObj("C", 11)])
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
            .HasFileLineNumber(1144)
            .HasMessage("""
                        Expecting contains exactly elements:
                            <Null>
                         do contains (in same order)
                            [tuple("A", 10, <Null>)]
                         but some elements not found:
                            [tuple("A", 10, <Null>)]
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
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
    [RequireGodotRuntime]
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
    [RequireGodotRuntime]
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
        => AssertThrown(() => AssertArray([])
                .OverrideFailureMessage("Custom failure message")
                .IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1233)
            .HasMessage("Custom failure message");

    [TestCase]
    public void InterruptIsFailure()
    {
        // we disable failure reporting until we simulate a failure
        if (ExecutionContext.Current != null)
            ExecutionContext.Current.FailureReporting = false;
        // try to fail
        AssertArray([]).IsNotEmpty();

        // expect this line will never call because of the test is interrupted by a failing assert
        AssertBool(true).OverrideFailureMessage("This line should never be called").IsFalse();
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void IsSame(int dataPointIndex)
    {
        var dataPoint = TestDataPointStringValues[dataPointIndex] as object[];
        dynamic current = dataPoint![0];
        dynamic other = dataPoint[1];
        AssertArray(current).IsSame(current);

        AssertThrown(() => AssertArray(current).IsSame(other))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1265)
            .HasMessage("""
                        Expecting be same:
                            ["a", "b", "c", "a"]
                         to refer to the same object
                            ["a", "b", "c", "a"]
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void IsNotSame(int dataPointIndex)
    {
        var dataPoint = TestDataPointStringValues[dataPointIndex] as object[];
        dynamic current = dataPoint![0];
        dynamic other = dataPoint[1];
        AssertArray(current).IsNotSame(other);

        AssertThrown(() => AssertArray(current).IsNotSame(current))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1288)
            .HasMessage("""
                        Expecting be NOT same: ["a", "b", "c", "a"]
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsSameOnString(int dataPointIndex)
    {
        var testData = TestDataPointStringValues[dataPointIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsSame(obj1);
        AssertArray(current).ContainsSame(obj2);
        AssertArray(current).ContainsSame(obj2, obj3);
        AssertArray(current).ContainsSame(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSame(obj4, obj1, obj3, obj2);
        AssertArray(current).ContainsSame(current);
        // should fail because the array not contains 'xxx' and 'yyy'
        AssertThrown(() => AssertArray(current).ContainsSame(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1318)
            .HasMessage("""
                        Expecting contains elements:
                            ["a", "b", "c", "a"]
                         do contains (in any order)
                            ["a", "X"]
                         but could not find elements:
                            ["X"]
                        """);
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsSameOnObjects(int dataPointIndex)
    {
        var testData = TestDataPointObjectValues[dataPointIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).ContainsSame(obj1);
        AssertArray(current).ContainsSame(obj2);
        AssertArray(current).ContainsSame(obj2, obj3);
        AssertArray(current).ContainsSame(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSame(obj4, obj1, obj3, obj2);
        AssertArray(current).ContainsSame(current);
        // should fail because the array not contains 'obj5'
        AssertThrown(() => AssertArray(current).ContainsSame(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1353)
            .HasMessage("""
                Expecting contains elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in any order)
                    [$obj1, $obj5]
                 but could not find elements:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase(0, TestName = "Array")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void ContainsSameExactlyOnString(int dataPointIndex)
    {
        var testData = TestDataPointStringValues[dataPointIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).ContainsSameExactly(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSameExactly(obj1, obj2, obj3, obj1); // for string obj1 is reference equals to obj4
        AssertArray(current).ContainsSameExactly(current);
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1390)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a"]
                         but others where not expected:
                            ["b", "c", "a"]
                        """);

        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1, obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1402)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a", "b"]
                         but others where not expected:
                            ["c", "a"]
                        """);
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1413)
            .HasMessage("""
                        Expecting contains exactly elements:
                            ["a", "b", "c", "a"]
                         do contains (in same order)
                            ["a", "X"]
                         but others where not expected:
                            ["b", "c", "a"]
                         and some elements not found:
                            ["X"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void ContainsSameExactlyOnObjects(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        // test against only one element
        AssertArray(current).ContainsSameExactly(obj1, obj2, obj3, obj4);
        AssertArray(current).ContainsSameExactly(current);
        // when compare by reference equal obj1 != obj4
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1, obj2, obj3, obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1447)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj2, $obj3, $obj1]
                 but others where not expected:
                    [$obj4]
                 and some elements not found:
                    [$obj1]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4)));
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1464)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1]
                 but others where not expected:
                    [$obj2, $obj3, $obj4]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4)));
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1, obj2))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1479)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj2]
                 but others where not expected:
                    [$obj3, $obj4]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4)));
        AssertThrown(() => AssertArray(current).ContainsSameExactly(obj1, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1494)
            .HasMessage("""
                Expecting contains exactly elements:
                    [$obj1, $obj2, $obj3, $obj4]
                 do contains (in same order)
                    [$obj1, $obj5]
                 but others where not expected:
                    [$obj2, $obj3, $obj4]
                 and some elements not found:
                    [$obj5]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void NotContainsOnStrings(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        dynamic obj5 = testData[2];

        AssertArray(current).NotContains(obj5);
        AssertArray(current).NotContains(obj5, obj5);
        // we find `a` twice because is string equal
        AssertThrown(() => AssertArray(current).NotContains(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1530)
            .HasMessage("""
                        Expecting:
                            ["a", "b", "c", "a"]
                         do NOT contains (in any order)
                            ["a"]
                         but found elements:
                            ["a", "a"]
                        """);
        AssertThrown(() => AssertArray(current).NotContains(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1541)
            .HasMessage("""
                        Expecting:
                            ["a", "b", "c", "a"]
                         do NOT contains (in any order)
                            ["a", "b", "X"]
                         but found elements:
                            ["a", "b", "a"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void NotContainsOnObject(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).NotContains(obj5);
        AssertArray(current).NotContains(obj5, obj5);
        AssertThrown(() => AssertArray(current).NotContains(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1571)
            .HasMessage("""
                Expecting:
                    [$obj1, $obj2, $obj3, $obj4]
                 do NOT contains (in any order)
                    [$obj1]
                 but found elements:
                    [$obj1]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
        AssertThrown(() => AssertArray(current).NotContains(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1587)
            .HasMessage("""
                Expecting:
                    [$obj1, $obj2, $obj3, $obj4]
                 do NOT contains (in any order)
                    [$obj1, $obj2, $obj5]
                 but found elements:
                    [$obj1, $obj2]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase(0, TestName = "Array<string>")]
    [TestCase(1, TestName = "List<string>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<string>")]
    [RequireGodotRuntime]
    public void NotContainsSameOnStrings(int testDataIndex)
    {
        var testData = TestDataPointStringValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        dynamic obj5 = testData[2];

        AssertArray(current).NotContainsSame(obj5);
        AssertArray(current).NotContainsSame(obj5, obj5);
        // we find `a` twice because is string equal
        AssertThrown(() => AssertArray(current).NotContainsSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1621)
            .HasMessage("""
                        Expecting:
                            ["a", "b", "c", "a"]
                         do NOT contains (in any order)
                            ["a"]
                         but found elements:
                            ["a", "a"]
                        """);
        AssertThrown(() => AssertArray(current).NotContainsSame(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1632)
            .HasMessage("""
                        Expecting:
                            ["a", "b", "c", "a"]
                         do NOT contains (in any order)
                            ["a", "b", "X"]
                         but found elements:
                            ["a", "b", "a"]
                        """);
    }

    [TestCase(0, TestName = "Array<object>")]
    [TestCase(1, TestName = "List<object>")]
    [TestCase(2, TestName = "GodotArray")]
    [TestCase(3, TestName = "GodotArray<RefCount>")]
    [RequireGodotRuntime]
    public void NotContainsSameOnObject(int testDataIndex)
    {
        var testData = TestDataPointObjectValues[testDataIndex] as object[];
        dynamic current = testData![0];
        var obj1 = current[0];
        var obj2 = current[1];
        var obj3 = current[2];
        var obj4 = current[3];
        dynamic obj5 = testData[2];

        AssertArray(current).NotContainsSame(obj5);
        AssertArray(current).NotContainsSame(obj5, obj5);
        AssertThrown(() => AssertArray(current).NotContainsSame(obj1))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1662)
            .HasMessage("""
                Expecting:
                    [$obj1, $obj2, $obj3, $obj4]
                 do NOT contains (in any order)
                    [$obj1]
                 but found elements:
                    [$obj1]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
        AssertThrown(() => AssertArray(current).NotContainsSame(obj1, obj2, obj5))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1678)
            .HasMessage("""
                Expecting:
                    [$obj1, $obj2, $obj3, $obj4]
                 do NOT contains (in any order)
                    [$obj1, $obj2, $obj5]
                 but found elements:
                    [$obj1, $obj2]
                """
                .Replace("$obj1", AssertFailures.AsObjectId(obj1))
                .Replace("$obj2", AssertFailures.AsObjectId(obj2))
                .Replace("$obj3", AssertFailures.AsObjectId(obj3))
                .Replace("$obj4", AssertFailures.AsObjectId(obj4))
                .Replace("$obj5", AssertFailures.AsObjectId(obj5)));
    }

    [TestCase]
    public void AppendFailureMessage()
        => AssertThrown(() => AssertArray([])
                .AppendFailureMessage("custom data")
                .IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(1698)
            .HasMessage("""
                        Expecting be <Null>:
                         but is
                            <Empty>

                        Additional info:
                        custom data
                        """);

    [TestCase]
    [RequireGodotRuntime]
    public void MethodChainingBaseAssert()
    {
        var array = new Godot.Collections.Array
        {
            1,
            "hello",
            3.14,
            true
        };
        AssertThat(array).IsNotNull().Contains("hello").HasSize(4);
        AssertThat(array).Contains("hello").IsNotNull().HasSize(4);
    }

    // ReSharper disable once PartialTypeWithSinglePart
    // ReSharper disable MemberCanBePrivate.Local
    // ReSharper disable UnusedMember.Local
    private sealed partial class TestObj : RefCounted
    {
        private readonly string name;
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
}
