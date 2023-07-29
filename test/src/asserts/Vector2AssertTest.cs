// GdUnit generated TestSuite
using Godot;

namespace GdUnit4.Asserts
{
    using static Assertions;

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
                .HasPropertyValue("LineNumber", 25)
                .HasMessage("""
                    Expecting:
                        '(0, -0.1)'
                     in range between
                        '(0, 0)' <> '(1, 1)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1.1f, 0)).IsBetween(Vector2.Zero, Vector2.One))
                .HasMessage("""
                    Expecting:
                        '(1.1, 0)'
                     in range between
                        '(0, 0)' <> '(1, 1)'
                    """);
        }

        [TestCase]
        public void IsEqual()
        {
            AssertVec2(Vector2.One).IsEqual(Vector2.One);
            AssertVec2(Vector2.Inf).IsEqual(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsEqual(new Vector2(1.2f, 1.000001f));
            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 49)
                .HasMessage("""
                    Expecting be equal:
                        '(1.2, 1.000001)' but is '(1, 1)'
                    """);
        }

        [TestCase]
        public void IsNotEqual()
        {
            AssertVec2(Vector2.One).IsNotEqual(Vector2.Inf);
            AssertVec2(Vector2.Inf).IsNotEqual(Vector2.One);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000002f));
            // false test
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 64)
                .HasMessage("""
                    Expecting be NOT equal:
                        '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                    """);
        }

        [TestCase]
        public void IsEqualApprox()
        {
            AssertVec2(Vector2.One).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
            AssertVec2(new Vector2(0.996f, 0.996f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
            AssertVec2(new Vector2(1.004f, 1.004f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));

            // false test
            AssertThrown(() => AssertVec2(new Vector2(1.005f, 1f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f)))
                .HasPropertyValue("LineNumber", 80)
                .HasMessage("""
                    Expecting:
                        '(1.005, 1)'
                     in range between
                        '(0.996, 0.996)' <> '(1.004, 1.004)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1f, 0.995f)).IsEqualApprox(Vector2.One, new Vector2(0f, 0.004f)))
                .HasPropertyValue("LineNumber", 88)
                .HasMessage("""
                    Expecting:
                        '(1, 0.995)'
                     in range between
                        '(1, 0.996)' <> '(1, 1.004)'
                    """);
        }

        [TestCase]
        public void IsGreater()
        {
            AssertVec2(Vector2.Inf).IsGreater(Vector2.One);
            AssertVec2(new Vector2(1.2f, 1.000002f)).IsGreater(new Vector2(1.2f, 1.000001f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.Zero).IsGreater(Vector2.One))
                .HasPropertyValue("LineNumber", 105)
                .HasMessage("""
                    Expecting to be greater than:
                        '(1, 1)' but is '(0, 0)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsGreater(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 111)
                .HasMessage("""
                    Expecting to be greater than:
                        '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                    """);
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
                .HasPropertyValue("LineNumber", 128)
                .HasMessage("""
                    Expecting to be greater than or equal:
                        '(1, 1)' but is '(0, 0)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000002f)).IsGreaterEqual(new Vector2(1.2f, 1.000003f)))
                .HasPropertyValue("LineNumber", 134)
                .HasMessage("""
                    Expecting to be greater than or equal:
                        '(1.2, 1.000003)' but is '(1.2, 1.000002)'
                    """);
        }

        [TestCase]
        public void IsLess()
        {
            AssertVec2(Vector2.One).IsLess(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000002f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsLess(Vector2.One))
                .HasPropertyValue("LineNumber", 149)
                .HasMessage("""
                    Expecting to be less than:
                        '(1, 1)' but is '(1, 1)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 155)
                .HasMessage("""
                    Expecting to be less than:
                        '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                    """);
        }

        [TestCase]
        public void IsLessEqual()
        {
            AssertVec2(Vector2.One).IsLessEqual(Vector2.Inf);
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000001f));
            AssertVec2(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000002f));

            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsLessEqual(Vector2.Zero))
                .HasPropertyValue("LineNumber", 171)
                .HasMessage("""
                    Expecting to be less than or equal:
                        '(0, 0)' but is '(1, 1)'
                    """);
            AssertThrown(() => AssertVec2(new Vector2(1.2f, 1.000002f)).IsLessEqual(new Vector2(1.2f, 1.000001f)))
                .HasPropertyValue("LineNumber", 177)
                .HasMessage("""
                    Expecting to be less than or equal:
                        '(1.2, 1.000001)' but is '(1.2, 1.000002)'
                    """);
        }

        [TestCase]
        public void IsNotBetween()
        {
            AssertVec2(new Vector2(1f, 1.0002f)).IsNotBetween(Vector2.Zero, Vector2.One);
            // false test
            AssertThrown(() => AssertVec2(Vector2.One).IsNotBetween(Vector2.Zero, Vector2.One))
                .HasPropertyValue("LineNumber", 190)
                .HasMessage("""
                    Expecting:
                        '(1, 1)'
                     be NOT in range between
                        '(0, 0)' <> '(1, 1)'
                    """);
        }

        [TestCase]
        public void OverrideFailureMessage()
        {
            AssertThrown(() => AssertVec2(Vector2.One).OverrideFailureMessage("Custom Error").IsEqual(Vector2.Zero))
               .HasMessage("Custom Error");
        }
    }
}
