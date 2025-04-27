namespace GdUnit4.Tests.Extensions;

using GdUnit4.Core.Extensions;

using Godot.Collections;

using static Assertions;

[TestSuite]
[RequireGodotRuntime]
public class GodotObjectExtensionsTest
{
    [TestCase]
    public void ToGodotDictionary()
    {
        var result = new System.Collections.Generic.Dictionary<string, object>
        {
            { "path", "res://foo/barTest.cs" },
            { "line", 42 }
        };
        var expected = new Dictionary
        {
            { "path", "res://foo/barTest.cs" },
            { "line", 42 }
        };
        AssertThat(result.ToGodotDictionary()).IsEqual(expected);
    }

    [TestCase]
    public void ToGodotDictionaryNestedDictionary()
    {
        var result = new System.Collections.Generic.Dictionary<string, object>
        {
            { "path", "res://foo/barTest.cs" },
            { "line", 42 },
            {
                "statistics", new System.Collections.Generic.Dictionary<string, object>
                {
                    { "foo", "vale" },
                    { "bar", 42 }
                }
            }
        };
        var expected = new Dictionary
        {
            { "path", "res://foo/barTest.cs" },
            { "line", 42 },
            {
                "statistics", new Dictionary
                {
                    { "foo", "vale" },
                    { "bar", 42 }
                }
            }
        };
        AssertThat(result.ToGodotDictionary()).IsEqual(expected);
    }
}
