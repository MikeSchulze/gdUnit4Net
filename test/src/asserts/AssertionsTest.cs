
namespace GdUnit4.Tests.Asserts;

using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Godot;

using GdUnit4.Asserts;
using GdUnit4.Executions;
using static Assertions;

[TestSuite]
public class AssertionsTest
{

    [TestCase]
    public void DoAssertNotYetImplemented()
        => AssertThrown(() => AssertNotYetImplemented())
            .HasFileLineNumber(21)
            .HasMessage("Test not yet implemented!");

    [TestCase]
    public void AssertThatVariants()
    {
        // Godot number Variants
        AssertObject(AssertThat(Variant.From((sbyte)-1))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(11))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(12L))).IsInstanceOf<INumberAssert<int>>();
        AssertObject(AssertThat(Variant.From(1.4f))).IsInstanceOf<INumberAssert<float>>();
        AssertObject(AssertThat(Variant.From(1.5d))).IsInstanceOf<INumberAssert<float>>();
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
        AssertObject(AssertThat(Array.Empty<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new System.Collections.ArrayList())).IsInstanceOf<IEnumerableAssert<object>>();
        AssertObject(AssertThat(new System.Collections.BitArray(new bool[] { true, false }))).IsInstanceOf<IEnumerableAssert<bool>>();
        AssertObject(AssertThat(new System.Collections.Generic.HashSet<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new System.Collections.Generic.List<byte>())).IsInstanceOf<IEnumerableAssert<byte>>();
        AssertObject(AssertThat(new Godot.Collections.Array())).IsInstanceOf<IEnumerableAssert<Variant>>();
        AssertObject(AssertThat(new Godot.Collections.Array<int>())).IsInstanceOf<IEnumerableAssert<int>>();
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

    [TestCase]
    public void AsObjectId()
    {
        var obj1 = new object();
        var obj2 = new object();

        AssertThat(AssertFailures.AsObjectId(null)).IsEqual($"<Null>");
        AssertThat(AssertFailures.AsObjectId(obj1)).IsEqual($"<System.Object> (objId: {obj1.GetHashCode()})");
        AssertThat(AssertFailures.AsObjectId(obj1)).IsNotEqual(AssertFailures.AsObjectId(obj2));

        // on Godot Objects
        var obj3 = new RefCounted();
        AssertThat(AssertFailures.AsObjectId(obj3)).IsEqual($"<Godot.RefCounted> (objId: {obj3.GetInstanceId()})");
        var obj4 = new QuadMesh();
        AssertThat(AssertFailures.AsObjectId(obj4)).IsEqual($"<Godot.QuadMesh> (objId: {obj4.GetInstanceId()})");

        // on Godot Variants
        var obj5 = new RefCounted();
        AssertThat(AssertFailures.AsObjectId(obj5.ToVariant())).IsEqual($"<Godot.RefCounted> (objId: {obj5.GetInstanceId()})");

        object? obj6 = null;
        AssertThat(AssertFailures.AsObjectId(obj6.ToVariant())).IsEqual($"<Godot.Variant> (Null)");

        // without overrides ToString
        var obj7 = new ClassWithoutToString("Custom");
        AssertThat(AssertFailures.AsObjectId(obj7)).IsEqual($"<GdUnit4.Tests.Asserts.AssertionsTest+ClassWithoutToString> (objId: {obj7.GetHashCode()})");
        // with overrides ToString
        var obj8 = new ClassWithToString("Custom");
        AssertThat(AssertFailures.AsObjectId(obj8)).IsEqual($"Custom (objId: {obj8.GetHashCode()})");
    }

    [TestCase]
    public void UsingMSTestAssertions()
    {
        var currentCulture = CultureInfo.DefaultThreadCurrentCulture;
        var currentUICulture = CultureInfo.DefaultThreadCurrentUICulture;

        try
        {
            // we want to test against english culture
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
            var assertion = AssertThrown(() => Assert.AreEqual(1, 2))
                .IsInstanceOf<AssertFailedException>()
                .StartsWithMessage("Assert.AreEqual failed. Expected:<1>. Actual:<2>.");
            AssertTrimmedExceptionStackTrace(assertion)
                .NotContains("Microsoft.VisualStudio.TestTools");
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = currentUICulture;
        }
    }

    private static IStringAssert AssertTrimmedExceptionStackTrace(IExceptionAssert? exceptionAssert)
    {
        var stackTrace = (exceptionAssert as ExceptionAssert<Exception>)?.GetExceptionStackTrace()!;
        var trimmedStackTrace = ExecutionStage<int>.TrimStackTrace(stackTrace);
        return AssertString(trimmedStackTrace);
    }

    private class ClassWithoutToString
    {
        public string Value { get; }

        public ClassWithoutToString(string value)
            => Value = value;
    }

    private sealed class ClassWithToString : ClassWithoutToString
    {
        public ClassWithToString(string value) : base(value)
        {
        }
        public override string ToString() => $"{Value}";
    }
}
