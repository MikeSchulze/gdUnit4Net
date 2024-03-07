
namespace GdUnit4.Tests.Asserts;

using Godot;
using GdUnit4.Asserts;

using static Assertions;

[TestSuite]
public class AssertionsTest
{

    [TestCase]
    public void DoAssertNotYetImplemented()
        => AssertThrown(() => AssertNotYetImplemented())
            .HasPropertyValue("LineNumber", 15)
            .HasMessage("Test not yet implemented!");

    [TestCase]
    public void AssertThatVariants()
    {
        // Godot number Variants
        AssertObject(AssertThat(Variant.From((sbyte)-1))).IsInstanceOf<INumberAssert<long>>();
        AssertObject(AssertThat(Variant.From(11))).IsInstanceOf<INumberAssert<long>>();
        AssertObject(AssertThat(Variant.From(12L))).IsInstanceOf<INumberAssert<long>>();
        AssertObject(AssertThat(Variant.From(1.4f))).IsInstanceOf<INumberAssert<double>>();
        AssertObject(AssertThat(Variant.From(1.5d))).IsInstanceOf<INumberAssert<double>>();
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
        AssertObject(AssertThat((float)1.1f)).IsInstanceOf<INumberAssert<float>>();
        AssertObject(AssertThat((double)1.1d)).IsInstanceOf<INumberAssert<double>>();
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
    public void AssertThatEnumerable()
    {
        AssertObject(AssertThat(System.Array.Empty<byte>())).IsInstanceOf<IEnumerableAssert>();
        AssertObject(AssertThat(new System.Collections.ArrayList())).IsInstanceOf<IEnumerableAssert>();
        AssertObject(AssertThat(new System.Collections.BitArray(new bool[] { true, false }))).IsInstanceOf<IEnumerableAssert>();
        AssertObject(AssertThat(new System.Collections.Generic.HashSet<byte>())).IsInstanceOf<IEnumerableAssert>();
        AssertObject(AssertThat(new System.Collections.Generic.List<byte>())).IsInstanceOf<IEnumerableAssert>();
        AssertObject(AssertThat(new Godot.Collections.Array())).IsInstanceOf<IEnumerableAssert>();
    }

    [TestCase]
    public void AssertThatDictionary()
    {
        AssertObject(AssertThat(new Godot.Collections.Dictionary())).IsInstanceOf<IDictionaryAssert<Variant, Variant>>();
        AssertObject(AssertThat(new Godot.Collections.Dictionary<string, Variant>())).IsInstanceOf<IDictionaryAssert<string, Variant>>();
        AssertObject(AssertThat(new Godot.Collections.Dictionary<string, string>())).IsInstanceOf<IDictionaryAssert<string, string>>();
        AssertObject(AssertThat(new System.Collections.Hashtable())).IsInstanceOf<IDictionaryAssert<object, object>>();
        AssertObject(AssertThat(new System.Collections.Generic.Dictionary<string, object>())).IsInstanceOf<IDictionaryAssert<string, object>>();
    }

    [TestCase]
    public void AutoFreeOnNull()
    {
        var obj = AutoFree((Node)null!);
        AssertThat(obj).IsNull();
    }
}
