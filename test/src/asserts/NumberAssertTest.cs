namespace GdUnit4.Tests.Asserts;

using System;

using static Assertions;
using Executions;
using Exceptions;

[TestSuite]
public class NumberAssertTest
{
    [TestCase]
    public void IsNull()
        => AssertThrown(() => AssertInt(23).IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 14)
            .HasMessage("""
                Expecting be <Null>:
                 but is
                    '23'
                """);

    [TestCase]
    public void IsNotNull()
    {
        AssertInt(-23).IsNotNull();
        AssertInt(0).IsNotNull();
        AssertInt(23).IsNotNull();
    }

    [TestCase]
    public void IsEqual()
    {
        AssertInt(-23).IsEqual(-23);
        AssertInt(0).IsEqual(0);
        AssertInt(23).IsEqual(23);
        // this assertion fails because 23 are not equal to 42
        AssertThrown(() => AssertInt(38).IsEqual(42))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 38)
            .HasMessage("""
                Expecting be equal:
                    '42' but is '38'
                """);
    }

    [TestCase]
    public void IsNotEqual()
    {
        AssertInt(23).IsNotEqual(-23);
        AssertInt(23).IsNotEqual(42);
        // this assertion fails because 23 are equal to 23
        AssertThrown(() => AssertInt(23).IsNotEqual(23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 53)
            .HasMessage("""
                Expecting be NOT equal:
                    '23' but is '23'
                """);
    }

    [TestCase]
    public void IsLess()
    {
        AssertInt(-23).IsLess(-22);
        AssertInt(23).IsLess(42);
        AssertInt(23).IsLess(24);
        // this assertion fails because 23 is not less than 23
        AssertThrown(() => AssertInt(23).IsLess(23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 69)
            .HasMessage("""
                Expecting to be less than:
                    '23' but is '23'
                """);
        AssertThrown(() => AssertInt(23).IsLess(22))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 76)
            .HasMessage("""
                Expecting to be less than:
                    '22' but is '23'
                """);
        AssertThrown(() => AssertInt(-23).IsLess(-23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 83)
            .HasMessage("""
                Expecting to be less than:
                    '-23' but is '-23'
                """);
        AssertThrown(() => AssertInt(-23).IsLess(-24))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 90)
            .HasMessage("""
                Expecting to be less than:
                    '-24' but is '-23'
                """);
    }

    [TestCase]
    public void IsLessEqual()
    {
        AssertInt(-23).IsLessEqual(-22);
        AssertInt(-23).IsLessEqual(-23);
        AssertInt(0).IsLessEqual(0);
        AssertInt(23).IsLessEqual(23);
        AssertInt(23).IsLessEqual(42);
        // this assertion fails because 23 is not less than or equal to 22
        AssertThrown(() => AssertInt(23).IsLessEqual(22)).IsInstanceOf<TestFailedException>()
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 108)
            .HasMessage("""
                Expecting to be less than or equal:
                    '22' but is '23'
                """);
        AssertThrown(() => AssertInt(-23).IsLessEqual(-24))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 115)
            .HasMessage("""
                Expecting to be less than or equal:
                    '-24' but is '-23'
                """);
    }

    [TestCase]
    public void IsGreater()
    {
        AssertInt(-23).IsGreater(-24);
        AssertInt(1).IsGreater(0);
        AssertInt(23).IsGreater(20);
        AssertInt(23).IsGreater(22);
        // this assertion fails because 23 is not greater than 23
        AssertThrown(() => AssertInt(23).IsGreater(23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 132)
            .HasMessage("""
                Expecting to be greater than:
                    '23' but is '23'
                """);
        AssertThrown(() => AssertInt(23).IsGreater(24))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 139)
            .HasMessage("""
                Expecting to be greater than:
                    '24' but is '23'
                """);
        AssertThrown(() => AssertInt(-23).IsGreater(-23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 146)
            .HasMessage("""
                Expecting to be greater than:
                    '-23' but is '-23'
                """);
        AssertThrown(() => AssertInt(-23).IsGreater(-22))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 153)
            .HasMessage("""
                Expecting to be greater than:
                    '-22' but is '-23'
                """);
    }

    [TestCase]
    public void IsGreaterEqual()
    {
        AssertInt(-23).IsGreaterEqual(-24);
        AssertInt(-23).IsGreaterEqual(-23);
        AssertInt(0).IsGreaterEqual(0);
        AssertInt(23).IsGreaterEqual(20);
        AssertInt(23).IsGreaterEqual(23);
        // this assertion fails because 23 is not greater than 23
        AssertThrown(() => AssertInt(23).IsGreaterEqual(24))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 171)
            .HasMessage("""
                Expecting to be greater than or equal:
                    '24' but is '23'
                """);
        AssertThrown(() => AssertInt(-23).IsGreaterEqual(-22))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 178)
            .HasMessage("""
                Expecting to be greater than or equal:
                    '-22' but is '-23'
                """);
    }

    [TestCase]
    public void IsEven()
    {
        AssertInt(-200).IsEven();
        AssertInt(-22).IsEven();
        AssertInt(0).IsEven();
        AssertInt(22).IsEven();
        AssertInt(200).IsEven();

        AssertThrown(() => AssertInt(-13).IsEven())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 196)
            .HasMessage("""
                Expecting be even:
                 but is '-13'
                """);
        AssertThrown(() => AssertInt(13).IsEven())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 203)
            .HasMessage("""
                Expecting be even:
                 but is '13'
                """);
    }

    [TestCase]
    public void IsOdd()
    {
        AssertInt(-13).IsOdd();
        AssertInt(13).IsOdd();
        AssertThrown(() => AssertInt(-12).IsOdd())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 217)
            .HasMessage("""
                Expecting be odd:
                 but is '-12'
                """);
        AssertThrown(() => AssertInt(0).IsOdd())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 224)
            .HasMessage("""
                Expecting be odd:
                 but is '0'
                """);
        AssertThrown(() => AssertInt(12).IsOdd())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 231)
            .HasMessage("""
                Expecting be odd:
                 but is '12'
                """);
    }

    [TestCase]
    public void IsNegative()
    {
        AssertInt(-1).IsNegative();
        AssertInt(-23).IsNegative();
        AssertThrown(() => AssertInt(0).IsNegative())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 245)
            .HasMessage("""
                Expecting be negative:
                 but is '0'
                """);
        AssertThrown(() => AssertInt(13).IsNegative())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 252)
            .HasMessage("""
                Expecting be negative:
                 but is '13'
                """);
    }

    [TestCase]
    public void IsNotNegative()
    {
        AssertInt(0).IsNotNegative();
        AssertInt(13).IsNotNegative();
        AssertThrown(() => AssertInt(-1).IsNotNegative())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 266)
            .HasMessage("""
                Expecting be NOT negative:
                 but is '-1'
                """);
        AssertThrown(() => AssertInt(-13).IsNotNegative())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 273)
            .HasMessage("""
                Expecting be NOT negative:
                 but is '-13'
                """);
    }

    [TestCase]
    public void IsZero()
    {
        AssertInt(0).IsZero();
        // this assertion fail because the value is not zero
        AssertThrown(() => AssertInt(-1).IsZero())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 287)
            .HasMessage("""
                Expecting be zero:
                 but is '-1'
                """);
        AssertThrown(() => AssertInt(1).IsZero())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 294)
            .HasMessage("""
                Expecting be zero:
                 but is '1'
                """);
    }

    [TestCase]
    public void IsNotZero()
    {
        AssertInt(-1).IsNotZero();
        AssertInt(1).IsNotZero();
        // this assertion fail because the value is not zero
        AssertThrown(() => AssertInt(0).IsNotZero())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 309)
            .HasMessage("""
                Expecting be NOT zero:
                 but is '0'
                """);
    }

    [TestCase]
    public void IsIn()
    {
        AssertInt(5).IsIn(3, 4, 5, 6);
        AssertInt(5).IsIn(new int[] { 3, 4, 5, 6 });
        // this assertion fail because 7 is not in [3, 4, 5, 6]
        AssertThrown(() => AssertInt(7).IsIn(new int[] { 3, 4, 5, 6 }))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 324)
            .HasMessage("""
                Expecting:
                    '7'
                 is in
                    [3, 4, 5, 6]
                """);
        AssertThrown(() => AssertInt(7).IsIn(Array.Empty<int>()))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 333)
            .HasMessage("""
                Expecting:
                    '7'
                 is in
                    <Empty>
                """);
    }

    [TestCase]
    public void IsNotIn()
    {
        AssertInt(5).IsNotIn();
        AssertInt(5).IsNotIn(Array.Empty<int>());
        AssertInt(5).IsNotIn(new int[] { 3, 4, 6, 7 });
        AssertInt(5).IsNotIn(3, 4, 6, 7);
        // this assertion fail because 7 is not in [3, 4, 5, 6]
        AssertThrown(() => AssertInt(5).IsNotIn(new int[] { 3, 4, 5, 6 }))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 352)
            .HasMessage("""
                Expecting:
                    '5'
                 is not in
                    [3, 4, 5, 6]
                """);
    }

    [TestCase(Iterations = 40)]
    public void IsBetween([Fuzzer(-20)] int value)
        => AssertInt(value).IsBetween(-20, 20);

    [TestCase]
    public void IsBetweenMustFail()
    {
        AssertThrown(() => AssertInt(-10).IsBetween(-9, 0))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 370)
            .HasMessage("""
                Expecting:
                    '-10'
                 in range between
                    '-9' <> '0'
                """);
        AssertThrown(() => AssertInt(0).IsBetween(1, 10))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 379)
            .HasMessage("""
                Expecting:
                    '0'
                 in range between
                    '1' <> '10'
                """);
        AssertThrown(() => AssertInt(10).IsBetween(11, 21))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 388)
            .HasMessage("""
                Expecting:
                    '10'
                 in range between
                    '11' <> '21'
                """);
    }

    [TestCase]
    public void OverrideFailureMessage()
        => AssertThrown(() => AssertInt(10)
                .OverrideFailureMessage("Custom failure message")
                .IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 401)
            .HasMessage("Custom failure message");

    [TestCase]
    public void InterruptIsFailure()
    {
        // we disable failure reporting until we simulate an failure
        if (ExecutionContext.Current != null)
            ExecutionContext.Current.FailureReporting = false;
        // force an assertion failure
        AssertInt(10).IsZero();

        // expect this line will never called because of the test is interrupted by a failing assert
        AssertBool(true).OverrideFailureMessage("This line should never be called").IsFalse();
    }
}
