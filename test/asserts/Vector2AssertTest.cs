// GdUnit generated TestSuite
using Godot;
using GdUnit3;

namespace GdUnit3.Asserts
{
    using static Assertions;
    using static Utils;

    [TestSuite]
    public class Vector2AssertTest
    {
        // TestSuite generated from
        private const string sourceClazzPath = "d:/src/asserts/Vector2Assert.cs";
        [TestCase]
        public void AssertThatMapsToAssertVec2()
        {
            AssertObject(AssertThat(Vector2.One)).IsInstanceOf<IVector2Assert>();
        }

        [TestCase]
        public void IsBetween()
        {
            AssertVec2(Vector2.Zero).IsBetween(Vector2.Zero, Vector2.One);
            AssertVec2(Vector2.One).IsBetween(Vector2.Zero, Vector2.One);
            // false test
            AssertThrown(() => AssertVec2(new Vector2(0, -.1f)).IsBetween(Vector2.Zero, Vector2.One))
                .HasPropertyValue("LineNumber", 27)
                .HasMessage("Expecting:\n" + "  '(0, -0.1)'\n" + " in range between\n" + "  '(0, 0)' <> '(1, 1)'");
            AssertThrown(() => AssertVec2(new Vector2(1.1f, 0)).IsBetween(Vector2.Zero, Vector2.One))
                .HasMessage("Expecting:\n" + "  '(1.1, 0)'\n" + " in range between\n" + "  '(0, 0)' <> '(1, 1)'");
        }

        [TestCase]
        public void IsEqual()
        {
            AssertVec2(Vector2.One).IsEqual(Vector2.One);
            AssertVec2(Vector2.Inf).IsEqual(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsEqual(new Vector2(1.2f, 1.000001f));
            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 41)
                .HasMessage("Expecting be equal:\n  '(1.2, 1.000001)' but is '(1, 1)'");
        }

        [TestCase]
        public void IsNotEqual()
        {
            AssertVec2(Vector2.One).IsNotEqual(Vector2.Inf);
            AssertVec2(Vector2.Inf).IsNotEqual(Vector2.One);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000002f));
            // false test
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 53)
                .HasMessage("Expecting be NOT equal:\n  '(1.2, 1.000001)' but is '(1.2, 1.000001)'");
        }

        [TestCase]
        public void IsEqualApprox()
        {
            AssertVec2(Vector2.One).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
            AssertVec2(new Vector2(0.996f, 0.996f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
            AssertVec2(new Vector2(1.004f, 1.004f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));

            // false test
            AssertThrown(() => AssertVec2(new Vector2(1.005f, 1f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f)))
                .HasPropertyValue("LineNumber", 66)
                .HasMessage("Expecting:\n  '(1.005, 1)'\n in range between\n  '(0.996, 0.996)' <> '(1.004, 1.004)'");
            AssertThrown(() => AssertVec2(new Vector2(1f, 0.995f)).IsEqualApprox(Vector2.One, new Vector2(0f, 0.004f)))
                .HasPropertyValue("LineNumber", 69)
                .HasMessage("Expecting:\n  '(1, 0.995)'\n in range between\n  '(1, 0.996)' <> '(1, 1.004)'");
        }

        [TestCase]
        public void IsGreater()
        {
            AssertVec2(Vector2.Inf).IsGreater(Vector2.One);
            AssertVec2(new Vector2(1.2f, 1.000002f)).IsGreater(new Vector2(1.2f, 1.000001f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.Zero).IsGreater(Vector2.One))
                .HasPropertyValue("LineNumber", 81)
                .HasMessage("Expecting to be greater than:\n  '(1, 1)' but is '(0, 0)'");
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsGreater(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 84)
                .HasMessage("Expecting to be greater than:\n  '(1.2, 1.000001)' but is '(1.2, 1.000001)'");
        }

        [TestCase]
        public void IsGreaterEqual()
        {
            AssertVec2(Vector2.Inf).IsGreaterEqual(Vector2.One);
            AssertVec2(Vector2.One).IsGreaterEqual(Vector2.One);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsGreaterEqual(new Vector2(1.2f, 1.000001f));
            AssertVec2(new Vector2(1.2f, 1.000002f)).IsGreaterEqual(new Vector2(1.2f, 1.000001f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.Zero).IsGreaterEqual(Vector2.One))
                .HasPropertyValue("LineNumber", 98)
                .HasMessage("Expecting to be greater than or equal:\n  '(1, 1)' but is '(0, 0)'");
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000002f)).IsGreaterEqual(new Vector2(1.2f, 1.000003f)))
                .HasPropertyValue("LineNumber", 101)
                .HasMessage("Expecting to be greater than or equal:\n  '(1.2, 1.000003)' but is '(1.2, 1.000002)'");
        }

        [TestCase]
        public void IsLess()
        {
            AssertVec2(Vector2.One).IsLess(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000002f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsLess(Vector2.One))
                .HasPropertyValue("LineNumber", 113)
                .HasMessage("Expecting to be less than:\n  '(1, 1)' but is '(1, 1)'");
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 116)
                .HasMessage("Expecting to be less than:\n  '(1.2, 1.000001)' but is '(1.2, 1.000001)'");
        }

        [TestCase]
        public void IsLessEqual()
        {
            AssertVec2(Vector2.One).IsLessEqual(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000001f));
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000002f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsLessEqual(Vector2.Zero))
                .HasPropertyValue("LineNumber", 129)
                .HasMessage("Expecting to be less than or equal:\n  '(0, 0)' but is '(1, 1)'");
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000002f)).IsLessEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 132)
                .HasMessage("Expecting to be less than or equal:\n  '(1.2, 1.000001)' but is '(1.2, 1.000002)'");
        }

        [TestCase]
        public void IsNotBetween()
        {
            AssertVec2(new Vector2(1f, 1.0002f)).IsNotBetween(Vector2.Zero, Vector2.One);
            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsNotBetween(Vector2.Zero, Vector2.One))
                .HasPropertyValue("LineNumber", 142)
                .HasMessage("Expecting:\n  '(1, 1)'\n be NOT in range between\n  '(0, 0)' <> '(1, 1)'");
        }

        [TestCase]
        public void OverrideFailureMessage()
        {
            AssertThrown(() => AssertVec2(Vector2.One).OverrideFailureMessage("Custom Error").IsEqual(Vector2.Zero))
               .HasMessage("Custom Error");
        }

    }
}