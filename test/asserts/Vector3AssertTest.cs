// GdUnit generated TestSuite
using Godot;
using GdUnit3;

namespace GdUnit3.Asserts
{
    using static Assertions;
    using static Utils;

    [TestSuite]
    public class Vector3AssertTest
    {
        // TestSuite generated from
        private const string sourceClazzPath = "d:/src/asserts/Vector3Assert.cs";
        [TestCase]
        public void AssertThatMapsToAssertVec3()
        {
            AssertObject(AssertThat(Vector3.One)).IsInstanceOf<IVector3Assert>();
        }

        [TestCase]
        public void IsBetween()
        {
            AssertVec3(Vector3.Zero).IsBetween(Vector3.Zero, Vector3.One);
            AssertVec3(Vector3.One).IsBetween(Vector3.Zero, Vector3.One);
            // false test
            AssertThrown(() => AssertVec3(new Vector3(0, -.1f, 1f)).IsBetween(Vector3.Zero, Vector3.One))
                .HasPropertyValue("LineNumber", 27)
                .HasMessage("Expecting:\n" + "  '(0, -0.1, 1)'\n" + " in range between\n" + "  '(0, 0, 0)' <> '(1, 1, 1)'");
            AssertThrown(() => AssertVec3(new Vector3(1.1f, 0, 1f)).IsBetween(Vector3.Zero, Vector3.One))
                .HasMessage("Expecting:\n" + "  '(1.1, 0, 1)'\n" + " in range between\n" + "  '(0, 0, 0)' <> '(1, 1, 1)'");
        }

        [TestCase]
        public void IsEqual()
        {
            AssertVec3(Vector3.One).IsEqual(Vector3.One);
            AssertVec3(Vector3.Inf).IsEqual(Vector3.Inf);
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsEqual(new Vector3(1.2f, 1.000001f, 1f));
            // false test
            AssertThrown(() => AssertVec3(Vector3.One).IsEqual(new Vector3(1.2f, 1.000001f, 1f)))
                .HasPropertyValue("LineNumber", 41)
                .HasMessage("Expecting be equal:\n  '(1.2, 1.000001, 1)' but is '(1, 1, 1)'");
        }

        [TestCase]
        public void IsNotEqual()
        {
            AssertVec3(Vector3.One).IsNotEqual(Vector3.Inf);
            AssertVec3(Vector3.Inf).IsNotEqual(Vector3.One);
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsNotEqual(new Vector3(1.2f, 1.000002f, 1f));
            // false test
            AssertThrown(() => AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsNotEqual(new Vector3(1.2f, 1.000001f, 1f)))
                .HasPropertyValue("LineNumber", 53)
                .HasMessage("Expecting be NOT equal:\n  '(1.2, 1.000001, 1)' but is '(1.2, 1.000001, 1)'");
        }

        [TestCase]
        public void IsEqualApprox()
        {
            AssertVec3(Vector3.One).IsEqualApprox(Vector3.One, new Vector3(0.004f, 0.004f, 0.004f));
            AssertVec3(new Vector3(0.996f, 0.996f, 0.996f)).IsEqualApprox(Vector3.One, new Vector3(0.004f, 0.004f, 0.004f));
            AssertVec3(new Vector3(1.004f, 1.004f, 1.004f)).IsEqualApprox(Vector3.One, new Vector3(0.004f, 0.004f, 0.004f));

            // false test
            AssertThrown(() => AssertVec3(new Vector3(1.005f, 1f, 1f)).IsEqualApprox(Vector3.One, new Vector3(0.004f, 0.004f, 0.004f)))
                .HasPropertyValue("LineNumber", 66)
                .HasMessage("Expecting:\n  '(1.005, 1, 1)'\n in range between\n  '(0.996, 0.996, 0.996)' <> '(1.004, 1.004, 1.004)'");
            AssertThrown(() => AssertVec3(new Vector3(1f, 0.995f, 1f)).IsEqualApprox(Vector3.One, new Vector3(0f, 0.004f, 0f)))
                .HasPropertyValue("LineNumber", 69)
                .HasMessage("Expecting:\n  '(1, 0.995, 1)'\n in range between\n  '(1, 0.996, 1)' <> '(1, 1.004, 1)'");
        }

        [TestCase]
        public void IsGreater()
        {
            AssertVec3(Vector3.Inf).IsGreater(Vector3.One);
            AssertVec3(new Vector3(1.2f, 1.000002f, 1f)).IsGreater(new Vector3(1.2f, 1.000001f, 1f));

            // false test
            AssertThrown(() => AssertVec3(Vector3.Zero).IsGreater(Vector3.One))
                .HasPropertyValue("LineNumber", 81)
                .HasMessage("Expecting to be greater than:\n  '(1, 1, 1)' but is '(0, 0, 0)'");
            AssertThrown(() => AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsGreater(new Vector3(1.2f, 1.000001f, 1f)))
                .HasPropertyValue("LineNumber", 84)
                .HasMessage("Expecting to be greater than:\n  '(1.2, 1.000001, 1)' but is '(1.2, 1.000001, 1)'");
        }

        [TestCase]
        public void IsGreaterEqual()
        {
            AssertVec3(Vector3.Inf).IsGreaterEqual(Vector3.One);
            AssertVec3(Vector3.One).IsGreaterEqual(Vector3.One);
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsGreaterEqual(new Vector3(1.2f, 1.000001f, 1f));
            AssertVec3(new Vector3(1.2f, 1.000002f, 1f)).IsGreaterEqual(new Vector3(1.2f, 1.000001f, 1f));

            // false test
            AssertThrown(() => AssertVec3(Vector3.Zero).IsGreaterEqual(Vector3.One))
                .HasPropertyValue("LineNumber", 98)
                .HasMessage("Expecting to be greater than or equal:\n  '(1, 1, 1)' but is '(0, 0, 0)'");
            AssertThrown(() => AssertVec3(new Vector3(1.2f, 1.000002f, 1f)).IsGreaterEqual(new Vector3(1.2f, 1.000003f, 1f)))
                .HasPropertyValue("LineNumber", 101)
                .HasMessage("Expecting to be greater than or equal:\n  '(1.2, 1.000003, 1)' but is '(1.2, 1.000002, 1)'");
        }

        [TestCase]
        public void IsLess()
        {
            AssertVec3(Vector3.One).IsLess(Vector3.Inf);
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsLess(new Vector3(1.2f, 1.000002f, 1f));

            // false test
            AssertThrown(() => AssertVec3(Vector3.One).IsLess(Vector3.One))
                .HasPropertyValue("LineNumber", 113)
                .HasMessage("Expecting to be less than:\n  '(1, 1, 1)' but is '(1, 1, 1)'");
            AssertThrown(() => AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsLess(new Vector3(1.2f, 1.000001f, 1f)))
                .HasPropertyValue("LineNumber", 116)
                .HasMessage("Expecting to be less than:\n  '(1.2, 1.000001, 1)' but is '(1.2, 1.000001, 1)'");
        }

        [TestCase]
        public void IsLessEqual()
        {
            AssertVec3(Vector3.One).IsLessEqual(Vector3.Inf);
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsLessEqual(new Vector3(1.2f, 1.000001f, 1f));
            AssertVec3(new Vector3(1.2f, 1.000001f, 1f)).IsLessEqual(new Vector3(1.2f, 1.000002f, 1f));

            // false test
            AssertThrown(() => AssertVec3(Vector3.One).IsLessEqual(Vector3.Zero))
                .HasPropertyValue("LineNumber", 129)
                .HasMessage("Expecting to be less than or equal:\n  '(0, 0, 0)' but is '(1, 1, 1)'");
            AssertThrown(() => AssertVec3(new Vector3(1.2f, 1.000002f, 1f)).IsLessEqual(new Vector3(1.2f, 1.000001f, 1f)))
                .HasPropertyValue("LineNumber", 132)
                .HasMessage("Expecting to be less than or equal:\n  '(1.2, 1.000001, 1)' but is '(1.2, 1.000002, 1)'");
        }

        [TestCase]
        public void IsNotBetween()
        {
            AssertVec3(new Vector3(1f, 1.0002f, 1f)).IsNotBetween(Vector3.Zero, Vector3.One);
            // false test
            AssertThrown(() => AssertVec3(Vector3.One).IsNotBetween(Vector3.Zero, Vector3.One))
                .HasPropertyValue("LineNumber", 142)
                .HasMessage("Expecting:\n  '(1, 1, 1)'\n be NOT in range between\n  '(0, 0, 0)' <> '(1, 1, 1)'");
        }

        [TestCase]
        public void OverrideFailureMessage()
        {
            AssertThrown(() => AssertVec3(Vector3.One).OverrideFailureMessage("Custom Error").IsEqual(Vector3.Zero))
               .HasMessage("Custom Error");
        }

    }
}