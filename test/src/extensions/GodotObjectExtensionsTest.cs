using System.Collections.Generic;

namespace GdUnit4.Tests.Extensions
{
    using static Assertions;

    [TestSuite]
    public class GodotObjectExtensionsTest
    {

        [TestCase]
        public void ToGodotDictionary()
        {
            var result = new Dictionary<string, object> {
                    { "path", "res://foo/barTest.cs" },
                    { "line", 42 }
                };
            var expected = new Godot.Collections.Dictionary{
                    { "path", "res://foo/barTest.cs" },
                    { "line", 42 }
                };
            AssertThat(result.ToGodotDictionary()).IsEqual(expected);
        }

        [TestCase]
        public void ToGodotDictionaryNestedDictionary()
        {
            var result = new Dictionary<string, object> {
                    { "path", "res://foo/barTest.cs" },
                    { "line", 42 },
                    {"statistics", new Dictionary<string, object> {
                            { "foo", "vale" },
                            { "bar", 42 }
                        }
                    }
                };
            var expected = new Godot.Collections.Dictionary {
                    { "path", "res://foo/barTest.cs" },
                    { "line", 42 },
                    {"statistics", new Godot.Collections.Dictionary {
                            { "foo", "vale" },
                            { "bar", 42 }
                        }
                    }
                };
            AssertThat(result.ToGodotDictionary()).IsEqual(expected);
        }

    }
}
