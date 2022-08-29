// GdUnit generated TestSuite
using System.Collections;
using System.Collections.Generic;

namespace GdUnit3.Asserts
{
    using Exceptions;
    using static Assertions;
    using static Utils;

    [TestSuite]
    public class DictionaryAssertTest
    {
        // TestSuite generated from
        private const string sourceClazzPath = "D:/develop/workspace/gdUnit3Mono/src/asserts/DictionaryAssert.cs";


        [TestCase]
        public void OverrideFailureMessage()
        {
            AssertThrown(() => AssertThat((IDictionary?)null)
                    .OverrideFailureMessage("Custom failure message")
                    .IsNotNull())
                .IsInstanceOf<TestFailedException>()
                .HasPropertyValue("LineNumber", 21)
                .HasMessage("Custom failure message");
        }

        [TestCase]
        public void IsEqual_Hashtable()
        {
            var current = new Hashtable();
            var expected = new Hashtable();
            AssertThat(current).IsEqual(expected);

            current = new Hashtable() {
                {"a1", "100"},
                {"a2", "200"},
            };
            expected = new Hashtable(current);
            AssertThat(current).IsEqual(expected);

            current.Add("a3", 300);
            AssertThrown(() => AssertThat(current).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasPropertyValue("LineNumber", 44)
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a2, 200}; {a3, 300}; {a1, 100}");
            AssertThrown(() => AssertThat((Hashtable?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  <Null>");
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
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a1, 100}; {a2, 200}; {a3, 300}");
            AssertThrown(() => AssertThat((Dictionary<string, long>?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  <Null>");
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
            expected = new Godot.Collections.Dictionary(current);
            AssertThat(current).IsEqual(expected);

            current.Add("a3", 300);
            AssertThrown(() => AssertThat(current).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a1, 100}; {a2, 200}; {a3, 300}");
            AssertThrown(() => AssertThat((Godot.Collections.Dictionary?)null).IsEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be equal:\n  {a1, 100}; {a2, 200}\n but is\n  <Null>");
        }

        [TestCase]
        public void IsNotEqual_Hashtable()
        {
            var current = new Hashtable() {
                {"a1", "100"},
                {"a2", "200"},
            };
            var expected = new Hashtable(){
                {"a1", "101"},
                {"a2", "200"},
            };
            AssertThat(current).IsNotEqual(expected);

            expected = new Hashtable(current);
            AssertThrown(() => AssertThat(current).IsNotEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be NOT equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a1, 100}; {a2, 200}");
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
                .HasMessage("Expecting be NOT equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a1, 100}; {a2, 200}");
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

            expected = new Godot.Collections.Dictionary(current);
            AssertThrown(() => AssertThat(current).IsNotEqual(expected))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be NOT equal:\n  {a1, 100}; {a2, 200}\n but is\n  {a1, 100}; {a2, 200}");
        }

        [TestCase]
        public void IsNull()
        {
            AssertThat((Hashtable?)null).IsNull();
            AssertThat((Dictionary<string, long>?)null).IsNull();
            AssertThat((Godot.Collections.Dictionary?)null).IsNull();

            AssertThrown(() => AssertThat(new Hashtable()).IsNull())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be <Null>:\n but is\n  <Empty>");
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
                .HasMessage("Expecting be empty:\n but has size '2'");
            AssertThrown(() => AssertThat((IDictionary?)null).IsEmpty())
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting be empty:\n but is <Null>");
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
                .HasMessage("Expecting being NOT empty:\n but is empty");
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
                .HasMessage("Expecting size:\n  '10' but is {a1, 100}; {a2, 200}");
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
                .HasMessage("Expecting contains elements:\n"
                            + "  [a1, a2]\n"
                            + " do contains (in any order)\n"
                            + "  [a4, a2, a3, a1]\n"
                            + " but could not find elements:\n"
                            + "  [a4, a3]");
            AssertThrown(() => AssertThat(current).ContainsKeys(new List<string>() { "a4", "a2", "a3", "a1" }))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting contains elements:\n"
                            + "  [a1, a2]\n"
                            + " do contains (in any order)\n"
                            + "  [a4, a2, a3, a1]\n"
                            + " but could not find elements:\n"
                            + "  [a4, a3]");
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
                .HasMessage("Expecting:\n"
                            + "  [a1, a2]\n"
                            + " do NOT contains (in any order)\n"
                            + "  [a4, a2, a3]\n"
                            + " but found elements:\n"
                            + "  [a2]");
            AssertThrown(() => AssertThat(current).NotContainsKeys(new List<string>() { "a4", "a2", "a3" }))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting:\n"
                            + "  [a1, a2]\n"
                            + " do NOT contains (in any order)\n"
                            + "  [a4, a2, a3]\n"
                            + " but found elements:\n"
                            + "  [a2]");
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
                .HasMessage("Expecting do contain entry:\n  {a1, 200}\n found key but value is\n  '100'");
            AssertThrown(() => AssertThat(current).ContainsKeyValue("a3", 300L))
                .IsInstanceOf<TestFailedException>()
                .HasMessage("Expecting do contain entry:\n  {a3, 300}");
            AssertThrown(() => AssertThat((IDictionary?)null).ContainsKeyValue("a1", 200L))
                 .IsInstanceOf<TestFailedException>()
                 .HasMessage("Expecting be NOT <Null>:");
        }
    }
}
