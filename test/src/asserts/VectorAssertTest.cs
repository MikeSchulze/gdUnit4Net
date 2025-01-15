// GdUnit generated TestSuite

namespace GdUnit4.Tests.Asserts;

using System.Globalization;

using GdUnit4.Asserts;

using Godot;

using static Assertions;

[TestSuite]
public class VectorAssertTest
{
    [BeforeTest]
    public void Setup() =>
        // we need for testing assert failure messages to run on 'en-US' locale
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US", true);


    [TestCase]
    [RequireGodotRuntime]
    public void AssertThatMapsToVectorAssert()
    {
        AssertObject(AssertThat(Vector2.One)).IsInstanceOf<IVectorAssert<Vector2>>();
        AssertObject(AssertThat(Vector2I.One)).IsInstanceOf<IVectorAssert<Vector2I>>();
        AssertObject(AssertThat(Vector3.One)).IsInstanceOf<IVectorAssert<Vector3>>();
        AssertObject(AssertThat(Vector3I.One)).IsInstanceOf<IVectorAssert<Vector3I>>();
        AssertObject(AssertThat(Vector4.One)).IsInstanceOf<IVectorAssert<Vector4>>();
        AssertObject(AssertThat(Vector4I.One)).IsInstanceOf<IVectorAssert<Vector4I>>();
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsBetween()
    {
        AssertThat(Vector2.Zero).IsBetween(Vector2.Zero, Vector2.One);
        AssertThat(Vector2.One).IsBetween(Vector2.Zero, Vector2.One);
        // false test
        AssertThrown(() => AssertThat(new Vector2(0, -.1f)).IsBetween(Vector2.Zero, Vector2.One))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting:
                            '(0, -0.1)'
                         in range between
                            '(0, 0)' <> '(1, 1)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1.1f, 0)).IsBetween(Vector2.Zero, Vector2.One))
            .HasMessage("""
                        Expecting:
                            '(1.1, 0)'
                         in range between
                            '(0, 0)' <> '(1, 1)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsEqual()
    {
        AssertThat(Vector2.One).IsEqual(Vector2.One);
        AssertThat(Vector2.Inf).IsEqual(Vector2.Inf);
        AssertThat(new Vector2(1.2f, 1.000001f)).IsEqual(new Vector2(1.2f, 1.000001f));
        // false test
        AssertThrown(() => AssertThat(Vector2.One).IsEqual(new Vector2(1.2f, 1.000001f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting be equal:
                            '(1.2, 1.000001)' but is '(1, 1)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsNotEqual()
    {
        AssertThat(Vector2.One).IsNotEqual(Vector2.Inf);
        AssertThat(Vector2.Inf).IsNotEqual(Vector2.One);
        AssertThat(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000002f));
        // false test
        AssertThrown(() => AssertThat(new Vector2(1.2f, 1.000001f)).IsNotEqual(new Vector2(1.2f, 1.000001f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting be NOT equal:
                            '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsEqualApprox()
    {
        AssertThat(Vector2.One).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
        AssertThat(new Vector2(0.996f, 0.996f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));
        AssertThat(new Vector2(1.004f, 1.004f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f));

        // false test
        AssertThrown(() => AssertThat(new Vector2(1.005f, 1f)).IsEqualApprox(Vector2.One, new Vector2(0.004f, 0.004f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting:
                            '(1.005, 1)'
                         in range between
                            '(0.996, 0.996)' <> '(1.004, 1.004)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1f, 0.995f)).IsEqualApprox(Vector2.One, new Vector2(0f, 0.004f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting:
                            '(1, 0.995)'
                         in range between
                            '(1, 0.996)' <> '(1, 1.004)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsGreater()
    {
        AssertThat(Vector2.Inf).IsGreater(Vector2.One);
        AssertThat(new Vector2(1.2f, 1.000002f)).IsGreater(new Vector2(1.2f, 1.000001f));

        // false test
        AssertThrown(() => AssertThat(Vector2.Zero).IsGreater(Vector2.One))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be greater than:
                            '(1, 1)' but is '(0, 0)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1.2f, 1.000001f)).IsGreater(new Vector2(1.2f, 1.000001f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be greater than:
                            '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsGreaterEqual()
    {
        AssertThat(Vector2.Inf).IsGreaterEqual(Vector2.One);
        AssertThat(Vector2.One).IsGreaterEqual(Vector2.One);
        AssertThat(new Vector2(1.2f, 1.000001f)).IsGreaterEqual(new Vector2(1.2f, 1.000001f));
        AssertThat(new Vector2(1.2f, 1.000002f)).IsGreaterEqual(new Vector2(1.2f, 1.000001f));

        // false test
        AssertThrown(() => AssertThat(Vector2.Zero).IsGreaterEqual(Vector2.One))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be greater than or equal:
                            '(1, 1)' but is '(0, 0)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1.2f, 1.000002f)).IsGreaterEqual(new Vector2(1.2f, 1.000003f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be greater than or equal:
                            '(1.2, 1.000003)' but is '(1.2, 1.000002)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsLess()
    {
        AssertThat(Vector2.One).IsLess(Vector2.Inf);
        AssertThat(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000002f));

        // false test
        AssertThrown(() => AssertThat(Vector2.One).IsLess(Vector2.One))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be less than:
                            '(1, 1)' but is '(1, 1)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1.2f, 1.000001f)).IsLess(new Vector2(1.2f, 1.000001f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be less than:
                            '(1.2, 1.000001)' but is '(1.2, 1.000001)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsLessForAllVectorTypes()
    {
        AssertThat(Vector2.One).IsLess(Vector2.Inf);
        AssertThat(Vector2I.One).IsLess(Vector2I.MaxValue);
        AssertThat(Vector3.One).IsLess(Vector3.Inf);
        AssertThat(Vector3I.One).IsLess(Vector3I.MaxValue);
        AssertThat(Vector4.One).IsLess(Vector4.Inf);
        AssertThat(Vector4I.One).IsLess(Vector4I.MaxValue);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsLessEqual()
    {
        AssertThat(Vector2.One).IsLessEqual(Vector2.Inf);
        AssertThat(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000001f));
        AssertThat(new Vector2(1.2f, 1.000001f)).IsLessEqual(new Vector2(1.2f, 1.000002f));

        // false test
        AssertThrown(() => AssertThat(Vector2.One).IsLessEqual(Vector2.Zero))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be less than or equal:
                            '(0, 0)' but is '(1, 1)'
                        """);
        AssertThrown(() => AssertThat(new Vector2(1.2f, 1.000002f)).IsLessEqual(new Vector2(1.2f, 1.000001f)))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting to be less than or equal:
                            '(1.2, 1.000001)' but is '(1.2, 1.000002)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void IsNotBetween()
    {
        AssertThat(new Vector2(1f, 1.0002f)).IsNotBetween(Vector2.Zero, Vector2.One);
        // false test
        AssertThrown(() => AssertThat(Vector2.One).IsNotBetween(Vector2.Zero, Vector2.One))
            .HasFileLineNumber(ExpectedLineNumber())
            .HasMessage("""
                        Expecting:
                            '(1, 1)'
                         be NOT in range between
                            '(0, 0)' <> '(1, 1)'
                        """);
    }

    [TestCase]
    [RequireGodotRuntime]
    public void OverrideFailureMessage() =>
        AssertThrown(() => AssertThat(Vector2.One).OverrideFailureMessage("Custom Error").IsEqual(Vector2.Zero))
            .HasMessage("Custom Error");
}
