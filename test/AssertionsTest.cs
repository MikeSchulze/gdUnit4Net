namespace GdUnit3.Tests
{
    using static Assertions;
    using GdUnit3.Asserts;

    [TestSuite]
    public class AssertionsTest
    {

        [TestCase]
        public void DoAssertNotYetImplemented()
        {
            AssertThrown(() => AssertNotYetImplemented())
                .HasPropertyValue("LineNumber", 13)
                .HasMessage("Test not yet implemented!");
        }

        [TestCase]
        public void AssertThat_NumberTypes()
        {
            AssertObject(AssertThat((sbyte)-1)).IsInstanceOf<INumberAssert<sbyte>>();
            AssertObject(AssertThat((byte)1)).IsInstanceOf<INumberAssert<byte>>();
            AssertObject(AssertThat((short)-1)).IsInstanceOf<INumberAssert<short>>();
            AssertObject(AssertThat((ushort)1)).IsInstanceOf<INumberAssert<ushort>>();
            AssertObject(AssertThat((int)-1)).IsInstanceOf<INumberAssert<int>>();
            AssertObject(AssertThat((uint)1)).IsInstanceOf<INumberAssert<uint>>();
            AssertObject(AssertThat((long)-1)).IsInstanceOf<INumberAssert<long>>();
            AssertObject(AssertThat((ulong)1)).IsInstanceOf<INumberAssert<ulong>>();
            AssertObject(AssertThat((float)1.1f)).IsInstanceOf<INumberAssert<float>>();
            AssertObject(AssertThat((double)1.1d)).IsInstanceOf<INumberAssert<double>>();
            AssertObject(AssertThat((decimal)1.1m)).IsInstanceOf<INumberAssert<decimal>>();

            AssertObject(AssertThat(System.SByte.MaxValue)).IsInstanceOf<INumberAssert<System.SByte>>();
            AssertObject(AssertThat(System.Byte.MaxValue)).IsInstanceOf<INumberAssert<System.Byte>>();
            AssertObject(AssertThat(System.Int16.MaxValue)).IsInstanceOf<INumberAssert<System.Int16>>();
            AssertObject(AssertThat(System.UInt16.MaxValue)).IsInstanceOf<INumberAssert<System.UInt16>>();
            AssertObject(AssertThat(System.Int32.MaxValue)).IsInstanceOf<INumberAssert<System.Int32>>();
            AssertObject(AssertThat(System.UInt32.MaxValue)).IsInstanceOf<INumberAssert<System.UInt32>>();
            AssertObject(AssertThat(System.Int64.MaxValue)).IsInstanceOf<INumberAssert<System.Int64>>();
            AssertObject(AssertThat(System.UInt64.MaxValue)).IsInstanceOf<INumberAssert<System.UInt64>>();
            AssertObject(AssertThat(System.Single.MaxValue)).IsInstanceOf<INumberAssert<System.Single>>();
            AssertObject(AssertThat(System.Double.MaxValue)).IsInstanceOf<INumberAssert<System.Double>>();
            AssertObject(AssertThat(System.Decimal.MaxValue)).IsInstanceOf<INumberAssert<System.Decimal>>();
        }

        [TestCase]
        public void AssertThat_String()
        {
            AssertObject(AssertThat("abc")).IsInstanceOf<IStringAssert>();
            AssertObject(AssertThat(new System.String("abc"))).IsInstanceOf<IStringAssert>();
        }

        [TestCase]
        public void AssertThat_Bool()
        {
            AssertObject(AssertThat(true)).IsInstanceOf<IBoolAssert>();
            AssertObject(AssertThat(false)).IsInstanceOf<IBoolAssert>();
            AssertObject(AssertThat(new System.Boolean())).IsInstanceOf<IBoolAssert>();
        }

        [TestCase]
        public void AssertThat_Enumerables()
        {
            AssertObject(AssertThat(new byte[] { })).IsInstanceOf<IEnumerableAssert>();
            AssertObject(AssertThat(new System.Collections.ArrayList())).IsInstanceOf<IEnumerableAssert>();
            AssertObject(AssertThat(new System.Collections.BitArray(new bool[] { true, false }))).IsInstanceOf<IEnumerableAssert>();
            AssertObject(AssertThat(new System.Collections.Generic.HashSet<byte>())).IsInstanceOf<IEnumerableAssert>();
            AssertObject(AssertThat(new System.Collections.Generic.List<byte>())).IsInstanceOf<IEnumerableAssert>();
            AssertObject(AssertThat(new Godot.Collections.Array())).IsInstanceOf<IEnumerableAssert>();
        }

        [TestCase]
        public void AssertThat_Dictionary()
        {
            AssertObject(AssertThat(new System.Collections.Hashtable())).IsInstanceOf<IDictionaryAssert>();
            AssertObject(AssertThat(new System.Collections.Generic.Dictionary<string, object>())).IsInstanceOf<IDictionaryAssert>();
            AssertObject(AssertThat(new Godot.Collections.Dictionary())).IsInstanceOf<IDictionaryAssert>();
            AssertObject(AssertThat(new Godot.Collections.Dictionary())).IsInstanceOf<IDictionaryAssert>();
            AssertObject(AssertThat(new Godot.Collections.Dictionary<string, object>())).IsInstanceOf<IDictionaryAssert>();
        }

        [TestCase]
        public void AutoFree_OnNull()
        {
            Godot.Node obj = AutoFree((Godot.Node)null!);
            AssertThat(obj).IsNull();
        }
    }
}
