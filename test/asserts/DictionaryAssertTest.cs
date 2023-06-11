// GdUnit generated TestSuite
using System.Collections;
using System.Collections.Generic;

namespace GdUnit4.Asserts
{
    using Exceptions;
    using static Assertions;

    [TestSuite]
    public class DictionaryAssertTest
    {
        // TestSuite generated from
        private const string sourceClazzPath = "D:/develop/workspace/gdUnit4Mono/src/asserts/DictionaryAssert.cs";


        [TestCase]
        public void OverrideFailureMessage()
        {
            AssertThrown(() => AssertThat((IDictionary?)null)
                    .OverrideFailureMessage("Custom failure message")
                    .IsNotNull())
                .IsInstanceOf<TestFailedException>()
                .HasPropertyValue("LineNumber", 20)
                .HasMessage("Custom failure message");
        }

        [TestCase]
        public void IsEqual_Hashtable()
        {
            var current = new SortedDictionary<string, object>();
            var expected = new SortedDictionary<string, object>();
            AssertThat(current).IsEqual(expected);

            current = new SortedDictionary<string, object>() {
                { "a1", "100"},
                { "a2", "200"},
            };
            expected = new SortedDictionary<string, object>() {
                { "a1", "100"},
                { "a2", "200"},
            };
            AssertThat(current).IsEqual(expected);

            current.Add("a3", 300);
            AssertThrown(() => AssertThat(current).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasPropertyValue("LineNumber", 46)
                .HasMessage("""
                    Expecting be equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      {"a1", "100"}; {"a2", "200"}; {"a3", 300}
                    """);
            AssertThrown(() => AssertThat((Hashtable?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      <Null>
                    """);
        }

        [TestCase]
        public void IsEqual_Dictionary()
        {
            var current = new Dictionary<string, long>();
            var expected = new Dictionary<string, long>();
            AssertThat(current).IsEqual(expected);

            current = new Dictionary<string, long>() {
                {"a1", 100},
                {"a2", 200},
            };
            expected = new Dictionary<string, long>(current);
            AssertThat(current).IsEqual(expected);

            current.Add("a3", 300);
            AssertThrown(() => AssertThat(current).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be equal:
                      {"a1", 100}; {"a2", 200}
                     but is
                      {"a1", 100}; {"a2", 200}; {"a3", 300}
                    """);
            AssertThrown(() => AssertThat((Dictionary<string, long>?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be equal:
                      {"a1", 100}; {"a2", 200}
                     but is
                      <Null>
                    """);
        }

        [TestCase]
        public void IsEqual_GodotDictionary()
        {
            var current = new Godot.Collections.Dictionary();
            var expected = new Godot.Collections.Dictionary();
            AssertThat(current).IsEqual(expected);

            current = new Godot.Collections.Dictionary() {
                {"a1", "100"},
                {"a2", "200"},
            };
            expected = current.Duplicate();
            AssertThat(current).IsEqual(expected);

            current.Add("a3", 300);
            AssertThrown(() => AssertThat(current).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      {"a1", "100"}; {"a2", "200"}; {"a3", 300}
                    """);
            AssertThrown(() => AssertThat((Godot.Collections.Dictionary?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      <Null>
                    """);
        }

        [TestCase]
        public void IsNotEqual_Hashtable()
        {
            var current = new SortedDictionary<string, object>() {
                {"a1", "100"},
                {"a2", "200"},
            };
            var expected = new SortedDictionary<string, object>(){
                {"a1", "101"},
                {"a2", "200"},
            };
            AssertThat(current).IsNotEqual(expected);

            expected = new SortedDictionary<string, object>(current);
            AssertThrown(() => AssertThat(current).IsNotEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be NOT equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      {"a1", "100"}; {"a2", "200"}
                    """);
        }

        [TestCase]
        public void IsNotEqual_Dictionary()
        {
            var current = new Dictionary<string, long>() {
                {"a1", 100},
                {"a2", 200},
            };
            var expected = new Dictionary<string, long>(){
                {"a1", 101},
                {"a2", 200},
            };
            AssertThat(current).IsNotEqual(expected);

            expected = new Dictionary<string, long>(current);
            AssertThrown(() => AssertThat(current).IsNotEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be NOT equal:
                      {"a1", 100}; {"a2", 200}
                     but is
                      {"a1", 100}; {"a2", 200}
                    """);
        }

        [TestCase]
        public void IsNotEqual_GodotDictionary()
        {
            var current = new Godot.Collections.Dictionary(){
                {"a1", "100"},
                {"a2", "200"},
            };
            var expected = new Godot.Collections.Dictionary(){
                {"a1", "101"},
                {"a2", "200"},
            };
            AssertThat(current).IsNotEqual(expected);

            expected = current.Duplicate();
            AssertThrown(() => AssertThat(current).IsNotEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be NOT equal:
                      {"a1", "100"}; {"a2", "200"}
                     but is
                      {"a1", "100"}; {"a2", "200"}
                    """);
        }

        [TestCase]
        public void IsNull()
        {
            AssertThat((Hashtable?)null).IsNull();
            AssertThat((Dictionary<string, long>?)null).IsNull();
            AssertThat((Godot.Collections.Dictionary?)null).IsNull();

            AssertThrown(() => AssertThat(new Hashtable()).IsNull())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be <Null>:
                     but is
                      <Empty>
                    """);
        }

        [TestCase]
        public void IsNotNull()
        {
            AssertThat(new Hashtable()).IsNotNull();
            AssertThat(new Dictionary<string, long>()).IsNotNull();
            AssertThat(new Godot.Collections.Dictionary()).IsNotNull();

            AssertThrown(() => AssertThat((IDictionary?)null).IsNotNull())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be NOT <Null>:");
        }

        [TestCase]
        public void IsEmpty()
        {
            AssertThat(new Hashtable()).IsEmpty();
            AssertThat(new Dictionary<string, long>()).IsEmpty();
            AssertThat(new Godot.Collections.Dictionary()).IsEmpty();

            var expected = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThrown(() => AssertThat(expected).IsEmpty())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be empty:
                     but has size '2'
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).IsEmpty())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting be empty:
                     but is <Null>
                    """);
        }

        [TestCase]
        public void IsNotEmpty()
        {
            var current = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThat(current).IsNotEmpty();

            AssertThrown(() => AssertThat(new Hashtable()).IsNotEmpty())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting being NOT empty:
                     but is empty
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).IsNotEmpty())
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }

        [TestCase]
        public void HasSize()
        {
            var current = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThat(current).HasSize(2);
            AssertThat(new Hashtable()).HasSize(0);

            AssertThrown(() => AssertThat(current).HasSize(10))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting size:
                      '10' but is {"a1", 100}; {"a2", 200}
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).HasSize(10))
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }

        [TestCase]
        public void ContainsKeys()
        {
            var current = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThat(current).ContainsKeys("a1", "a2");
            AssertThat(current).ContainsKeys(new List<string>() { "a1", "a2" });

            AssertThrown(() => AssertThat(current).ContainsKeys("a4", "a2", "a3", "a1"))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting contains elements:
                      ["a1", "a2"]
                     do contains (in any order)
                      ["a4", "a2", "a3", "a1"]
                     but could not find elements:
                      ["a4", "a3"]
                    """);
            AssertThrown(() => AssertThat(current).ContainsKeys(new List<string>() { "a4", "a2", "a3", "a1" }))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting contains elements:
                      ["a1", "a2"]
                     do contains (in any order)
                      ["a4", "a2", "a3", "a1"]
                     but could not find elements:
                      ["a4", "a3"]
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).ContainsKeys("a1"))
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }

        [TestCase]
        public void NotContainsKeys()
        {
            var current = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThat(current).NotContainsKeys("b1", "a3");
            AssertThat(current).NotContainsKeys(new List<string>() { "b1", "a3" });

            AssertThrown(() => AssertThat(current).NotContainsKeys("a4", "a2", "a3"))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting:
                      ["a1", "a2"]
                     do NOT contains (in any order)
                      ["a4", "a2", "a3"]
                     but found elements:
                      ["a2"]
                    """);
            AssertThrown(() => AssertThat(current).NotContainsKeys(new List<string>() { "a4", "a2", "a3" }))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting:
                      ["a1", "a2"]
                     do NOT contains (in any order)
                      ["a4", "a2", "a3"]
                     but found elements:
                      ["a2"]
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).NotContainsKeys("a1"))
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }

        [TestCase]
        public void ContainsKeyValue()
        {
            var current = new Dictionary<string, long>(){
                {"a1", 100},
                {"a2", 200},
            };
            AssertThat(current).ContainsKeyValue("a1", 100L);

            AssertThrown(() => AssertThat(current).ContainsKeyValue("a1", 200L))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting do contain entry:
                      {"a1", 200}
                     found key but value is
                      '100'
                    """);
            AssertThrown(() => AssertThat(current).ContainsKeyValue("a3", 300L))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("""
                    Expecting do contain entry:
                      {"a3", 300}
                    """);
            AssertThrown(() => AssertThat((IDictionary?)null).ContainsKeyValue("a1", 200L))
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }
    }
}
