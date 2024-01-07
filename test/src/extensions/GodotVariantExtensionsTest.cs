using System;
using Godot;

namespace GdUnit4.Tests.Extensions
{
    using static Assertions;

    [TestSuite]
    public class GodotVariantExtensionsTest
    {

        [TestCase(null, Variant.Type.Nil)]
        [TestCase('A', Variant.Type.Int)]
        [TestCase(SByte.MaxValue, Variant.Type.Int)]
        [TestCase(Byte.MaxValue, Variant.Type.Int)]
        [TestCase(Int16.MaxValue, Variant.Type.Int)]
        [TestCase(UInt16.MaxValue, Variant.Type.Int)]
        [TestCase(Int32.MaxValue, Variant.Type.Int)]
        [TestCase(UInt32.MaxValue, Variant.Type.Int)]
        [TestCase(Int64.MaxValue, Variant.Type.Int)]
        [TestCase(UInt64.MaxValue, Variant.Type.Int)]
        [TestCase(Single.MaxValue, Variant.Type.Float)]
        [TestCase(Double.MaxValue, Variant.Type.Float)]
        [TestCase("HalloWorld", Variant.Type.String)]
        [TestCase(true, Variant.Type.Bool)]
        //[TestCase(Decimal.MaxValue, Variant.Type.Float)]
#nullable enable
        public void ToVariant(dynamic? value, Variant.Type type)
        {
            object? val = value;
#nullable disable
            Godot.Variant v = val.ToVariant();
            AssertObject(v).IsEqual(value == null ? new Variant() : Godot.Variant.CreateFrom(value));
            AssertObject(v.VariantType).IsEqual(type);
        }

    }
}
