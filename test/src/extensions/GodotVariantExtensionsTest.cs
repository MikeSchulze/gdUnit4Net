namespace GdUnit4.Tests.Extensions;

using Godot;

using static Assertions;

[TestSuite]
public class GodotVariantExtensionsTest
{

    [TestCase(null, Variant.Type.Nil)]
    [TestCase('A', Variant.Type.Int)]
    [TestCase(sbyte.MaxValue, Variant.Type.Int)]
    [TestCase(byte.MaxValue, Variant.Type.Int)]
    [TestCase(short.MaxValue, Variant.Type.Int)]
    [TestCase(ushort.MaxValue, Variant.Type.Int)]
    [TestCase(int.MaxValue, Variant.Type.Int)]
    [TestCase(uint.MaxValue, Variant.Type.Int)]
    [TestCase(long.MaxValue, Variant.Type.Int)]
    [TestCase(ulong.MaxValue, Variant.Type.Int)]
    [TestCase(float.MaxValue, Variant.Type.Float)]
    [TestCase(double.MaxValue, Variant.Type.Float)]
    [TestCase("HalloWorld", Variant.Type.String)]
    [TestCase(true, Variant.Type.Bool)]
    //[TestCase(Decimal.MaxValue, Variant.Type.Float)]

    public void ToVariant(dynamic? value, Variant.Type type)
    {
        object? val = value;
        var v = val.ToVariant();
        AssertObject(v).IsEqual(value == null ? new Variant() : Variant.CreateFrom(value));
        AssertObject(v.VariantType).IsEqual(type);
    }
}
