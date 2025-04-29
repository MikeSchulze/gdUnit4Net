namespace Example.Tests.API.Asserts;

using System.Collections;

using GdUnit4;
using GdUnit4.Asserts;

using Godot;
using Godot.Collections;

using static GdUnit4.Assertions;

using Array = Array;
using Vector2 = System.Numerics.Vector2;

[TestSuite]
public class AssertionsTest
{
    [TestCase]
    public void DoAssertNotYetImplemented()
        => AssertThrown(() => AssertNotYetImplemented())
            .HasFileLineNumber(21)
            .HasMessage("Test not yet implemented!");

    [TestCase]
    [RequireGodotRuntime]
    public void AssertThatVariants()
    {
        // Godot number Variants
        AssertObject(AssertThat(Variant.From((sbyte)-1))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(11))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(12L))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(1.4f))).IsInstanceOf<INumberAssert<float>>();
        AssertObject(AssertThat(Variant.From(1.5d))).IsInstanceOf<INumberAssert<float>>();
    }

    [TestCase(Description = "https://github.com/MikeSchulze/gdUnit4Net/issues/84")]
    public void AssertThatOnDynamics()
    {
        // object asserts
        AssertThat(Vector2.One).IsEqual(Vector2.One);
        AssertThat(TimeSpan.FromMilliseconds(124)).IsEqual(TimeSpan.FromMilliseconds(124));

        // dictionary asserts
        AssertThat(new Hashtable()).HasSize(0);

        // enumerable asserts
        AssertThat(new List<int>()).HasSize(0);
    }

    [TestCase]
    public void AssertThatNumberTypes()
    {
        AssertObject(AssertThat((sbyte)-1)).IsInstanceOf<INumberAssert<sbyte>>();
        AssertObject(AssertThat((byte)1)).IsInstanceOf<INumberAssert<byte>>();
        AssertObject(AssertThat((short)-1)).IsInstanceOf<INumberAssert<short>>();
        AssertObject(AssertThat((ushort)1)).IsInstanceOf<INumberAssert<ushort>>();
        AssertObject(AssertThat(-1)).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat((uint)1)).IsInstanceOf<INumberAssert<uint>>();
        AssertObject(AssertThat((long)-1)).IsInstanceOf<INumberAssert<long>>();
        AssertObject(AssertThat((ulong)1)).IsInstanceOf<INumberAssert<ulong>>();
        AssertObject(AssertThat(1.1f)).IsInstanceOf<INumberAssert<float>>();
        AssertObject(AssertThat(1.1d)).IsInstanceOf<INumberAssert<double>>();
        AssertObject(AssertThat(1.1m)).IsInstanceOf<INumberAssert<decimal>>();

        AssertObject(AssertThat(sbyte.MaxValue)).IsInstanceOf<INumberAssert<sbyte>>();
        AssertObject(AssertThat(byte.MaxValue)).IsInstanceOf<INumberAssert<byte>>();
        AssertObject(AssertThat(short.MaxValue)).IsInstanceOf<INumberAssert<short>>();
        AssertObject(AssertThat(ushort.MaxValue)).IsInstanceOf<INumberAssert<ushort>>();
        AssertObject(AssertThat(int.MaxValue)).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(uint.MaxValue)).IsInstanceOf<INumberAssert<uint>>();
        AssertObject(AssertThat(long.MaxValue)).IsInstanceOf<INumberAssert<long>>();
        AssertObject(AssertThat(ulong.MaxValue)).IsInstanceOf<INumberAssert<ulong>>();
        AssertObject(AssertThat(float.MaxValue)).IsInstanceOf<INumberAssert<float>>();
        AssertObject(AssertThat(double.MaxValue)).IsInstanceOf<INumberAssert<double>>();
        AssertObject(AssertThat(decimal.MaxValue)).IsInstanceOf<INumberAssert<decimal>>();
    }

    [TestCase]
    public void AssertThatString()
    {
        AssertObject(AssertThat("abc")).IsInstanceOf<IStringAssert>();
        AssertObject(AssertThat(new string("abc"))).IsInstanceOf<IStringAssert>();
    }

    [TestCase]
    public void AssertThatBool()
    {
        AssertObject(AssertThat(true)).IsInstanceOf<IBoolAssert>();
        AssertObject(AssertThat(false)).IsInstanceOf<IBoolAssert>();
        AssertObject(AssertThat(new bool())).IsInstanceOf<IBoolAssert>();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AssertThatEnumerable()
    {
        AssertObject(AssertThat(Array.Empty<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new ArrayList())).IsInstanceOf<IEnumerableAssert<object>>();
        AssertObject(AssertThat(new BitArray(new[] { true, false }))).IsInstanceOf<IEnumerableAssert<bool>>();
        AssertObject(AssertThat(new HashSet<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new List<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new Godot.Collections.Array())).IsInstanceOf<IEnumerableAssert<Variant>>();
        AssertObject(AssertThat(new Array<int>())).IsInstanceOf<IEnumerableAssert<int>>();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AssertThatDictionary()
    {
        AssertObject(AssertThat(new Dictionary())).IsInstanceOf<IDictionaryAssert<Variant, Variant>>();
        AssertObject(AssertThat(new Dictionary<string, Variant>())).IsInstanceOf<IDictionaryAssert<string, Variant>>();
        AssertObject(AssertThat(new Dictionary<string, string>())).IsInstanceOf<IDictionaryAssert<string, string>>();
        AssertObject(AssertThat(new Hashtable())).IsInstanceOf<IDictionaryAssert<object, object>>();
        AssertObject(AssertThat(new System.Collections.Generic.Dictionary<string, object>())).IsInstanceOf<IDictionaryAssert<string, object>>();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void AutoFreeOnNull()
    {
        var obj = AutoFree((Node)null!);
        AssertThat(obj).IsNull();
    }

    internal enum TestEnum
    {
        Foo,
        Bar
    }
}
