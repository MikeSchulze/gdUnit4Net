namespace GdUnit4.Tests.Extensions;

using GdUnit4.Core.Extensions;

using Godot;

using static Assertions;

[TestSuite]
public class GodotVariantExtensionsTest
{
    [GodotTestCase(null, Variant.Type.Nil)]
    [GodotTestCase('A', Variant.Type.Int)]
    [GodotTestCase(sbyte.MaxValue, Variant.Type.Int)]
    [GodotTestCase(byte.MaxValue, Variant.Type.Int)]
    [GodotTestCase(short.MaxValue, Variant.Type.Int)]
    [GodotTestCase(ushort.MaxValue, Variant.Type.Int)]
    [GodotTestCase(int.MaxValue, Variant.Type.Int)]
    [GodotTestCase(uint.MaxValue, Variant.Type.Int)]
    [GodotTestCase(long.MaxValue, Variant.Type.Int)]
    [GodotTestCase(ulong.MaxValue, Variant.Type.Int)]
    [GodotTestCase(float.MaxValue, Variant.Type.Float)]
    [GodotTestCase(double.MaxValue, Variant.Type.Float)]
    [GodotTestCase("HalloWorld", Variant.Type.String)]
    [GodotTestCase(true, Variant.Type.Bool)]
    //[TestCase(Decimal.MaxValue, Variant.Type.Float)]
    public void ToVariant(dynamic? value, Variant.Type type)
    {
        object? val = value;
        var v = val.ToVariant();
        AssertObject(v).IsEqual(value == null ? new Variant() : Variant.CreateFrom(value));
        AssertObject(v.VariantType).IsEqual(type);
    }
}
