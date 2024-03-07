namespace GdUnit4.Tests.Asserts;

using static Assertions;
using static GdUnit4.Asserts.IStringAssert.Compare;
using Exceptions;
using Executions;

[TestSuite]
public class StringAssertTest
{
    [TestCase]
    public void IsNull()
    {
        AssertString(null).IsNull();
        // should fail because the current is not null
        AssertThrown(() => AssertString("abc").IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 16)
            .StartsWithMessage("""
                Expecting be <Null>:
                 but is
                    "abc"
                """);
    }

    [TestCase]
    public void IsNotNull()
    {
        AssertString("abc").IsNotNull();
        // should fail because the current is null
        AssertThrown(() => AssertString(null).IsNotNull())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 31)
            .HasMessage("Expecting be NOT <Null>:");
    }

    [TestCase]
    public void IsEqual()
    {
        AssertString("This is a test message").IsEqual("This is a test message");
        AssertThrown(() => AssertString("This is a test message").IsEqual("This is a test Message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 41)
            .HasMessage("""
                Expecting be equal:
                    "This is a test Message"
                 but is
                    "This is a test message"
                """);
        AssertThrown(() => AssertString(null).IsEqual("This is a test Message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 50)
            .HasMessage("""
                Expecting be equal:
                    "This is a test Message"
                 but is
                    <Null>
                """);
    }

    [TestCase]
    public void IsEqualIgnoringCase()
    {
        AssertString("This is a test message").IsEqualIgnoringCase("This is a test Message");
        AssertThrown(() => AssertString("This is a test message").IsEqualIgnoringCase("This is a Message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 65)
            .HasMessage("""
                Expecting be equal (ignoring case):
                    "This is a Message"
                 but is
                    "This is a test message"
                """);
        AssertThrown(() => AssertString(null).IsEqualIgnoringCase("This is a Message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 74)
            .HasMessage("""
                Expecting be equal (ignoring case):
                    "This is a Message"
                 but is
                    <Null>
                """);
    }

    [TestCase]
    public void IsNotEqual()
    {
        AssertString(null).IsNotEqual("This is a test message");
        AssertString("This is a test message").IsNotEqual("This is a test Message");
        AssertThrown(() => AssertString("This is a test message").IsNotEqual("This is a test message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 90)
            .HasMessage("""
                Expecting be NOT equal:
                    "This is a test message"
                 but is
                    "This is a test message"
                """);
    }

    [TestCase]
    public void IsNotEqualIgnoringCase()
    {
        AssertString(null).IsNotEqualIgnoringCase("This is a Message");
        AssertString("This is a test message").IsNotEqualIgnoringCase("This is a Message");
        AssertThrown(() => AssertString("This is a test message").IsNotEqualIgnoringCase("This is a test Message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 106)
            .HasMessage("""
                Expecting be NOT equal (ignoring case):
                    "This is a test Message"
                 but is
                    "This is a test message"
                """);
    }

    [TestCase]
    public void IsEmpty()
    {
        AssertString("").IsEmpty();
        // should fail because the current value is not empty it contains a space
        AssertThrown(() => AssertString(" ").IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 122)
            .HasMessage("""
                Expecting be empty:
                 but is
                    " "
                """);
        AssertThrown(() => AssertString("abc").IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 130)
            .HasMessage("""
                Expecting be empty:
                 but is
                    "abc"
                """);
        AssertThrown(() => AssertString(null).IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 138)
            .HasMessage("""
                Expecting be empty:
                 but is
                    <Null>
                """);
    }

    [TestCase]
    public void IsNotEmpty()
    {
        AssertString(null).IsNotEmpty();
        AssertString(" ").IsNotEmpty();
        AssertString("	").IsNotEmpty();
        AssertString("abc").IsNotEmpty();
        // should fail because current is empty
        AssertThrown(() => AssertString("").IsNotEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 156)
            .HasMessage("""
                Expecting being NOT empty:
                 but is empty
                """);
    }

    [TestCase]
    public void Contains()
    {
        AssertString("This is a test message").Contains("a test");
        // must fail because of camel case difference
        AssertThrown(() => AssertString("This is a test message").Contains("a Test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 170)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 do contains
                    "a Test"
                """);
        AssertThrown(() => AssertString(null).Contains("a Test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 179)
            .HasMessage("""
                Expecting:
                    <Null>
                 do contains
                    "a Test"
                """);
    }

    [TestCase]
    public void NotContains()
    {
        AssertString(null).NotContains("a test");
        AssertString("This is a test message").NotContains("a text");
        AssertThrown(() => AssertString("This is a test message").NotContains("a test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 195)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 do not contain
                    "a test"
                """);
    }

    [TestCase]
    public void ContainsIgnoringCase()
    {
        AssertString("This is a test message").ContainsIgnoringCase("a Test");
        AssertThrown(() => AssertString("This is a test message").ContainsIgnoringCase("a Text"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 210)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 do contains (ignoring case)
                    "a Text"
                """);
        AssertThrown(() => AssertString(null).ContainsIgnoringCase("a Text"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 219)
            .HasMessage("""
                Expecting:
                    <Null>
                 do contains (ignoring case)
                    "a Text"
                """);
    }

    [TestCase]
    public void NotContainsIgnoringCase()
    {
        AssertString(null).NotContainsIgnoringCase("a Text");
        AssertString("This is a test message").NotContainsIgnoringCase("a Text");
        AssertThrown(() => AssertString("This is a test message").NotContainsIgnoringCase("a Test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 235)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 do not contain (ignoring case)
                    "a Test"
                """);
    }

    [TestCase]
    public void StartsWith()
    {
        AssertString("This is a test message").StartsWith("This is");
        AssertThrown(() => AssertString("This is a test message").StartsWith("This iss"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 250)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 to start with
                    "This iss"
                """);
        AssertThrown(() => AssertString("This is a test message").StartsWith("this is"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 259)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 to start with
                    "this is"
                """);
        AssertThrown(() => AssertString("This is a test message").StartsWith("test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 268)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 to start with
                    "test"
                """);
        AssertThrown(() => AssertString(null).StartsWith("test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 277)
            .HasMessage("""
                Expecting:
                    <Null>
                 to start with
                    "test"
                """);
    }

    [TestCase]
    public void EndsWith()
    {
        AssertString("This is a test message").EndsWith("test message");
        AssertThrown(() => AssertString("This is a test message").EndsWith("tes message"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 292)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 to end with
                    "tes message"
                """);
        AssertThrown(() => AssertString("This is a test message").EndsWith("a test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 301)
            .HasMessage("""
                Expecting:
                    "This is a test message"
                 to end with
                    "a test"
                """);
        AssertThrown(() => AssertString(null).EndsWith("a test"))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 310)
            .HasMessage("""
                Expecting:
                    <Null>
                 to end with
                    "a test"
                """);
    }

    [TestCase]
    public void HasLength()
    {
        AssertString("This is a test message").HasLength(22);
        AssertString("").HasLength(0);
        AssertThrown(() => AssertString("This is a test message").HasLength(23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 326)
            .HasMessage("""
                Expecting length:
                    '23' but is '22'
                """);
        AssertThrown(() => AssertString(null).HasLength(23))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 333)
            .HasMessage("""
                Expecting length:
                    '23' but is unknown
                """);
    }

    [TestCase]
    public void HasLengthLessThan()
    {
        AssertString("This is a test message").HasLength(23, LESS_THAN);
        AssertString("This is a test message").HasLength(42, LESS_THAN);
        AssertThrown(() => AssertString("This is a test message").HasLength(22, LESS_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 347)
            .HasMessage("""
                Expecting length to be less than:
                    '22' but is '22'
                """);
        AssertThrown(() => AssertString(null).HasLength(22, LESS_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 354)
            .HasMessage("""
                Expecting length to be less than:
                    '22' but is unknown
                """);
    }

    [TestCase]
    public void HasLengthLessEqual()
    {
        AssertString("This is a test message").HasLength(22, LESS_EQUAL);
        AssertString("This is a test message").HasLength(23, LESS_EQUAL);
        AssertThrown(() => AssertString("This is a test message").HasLength(21, LESS_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 368)
            .HasMessage("""
                Expecting length to be less than or equal:
                    '21' but is '22'
                """);
        AssertThrown(() => AssertString(null).HasLength(21, LESS_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 375)
            .HasMessage("""
                Expecting length to be less than or equal:
                    '21' but is unknown
                """);
    }

    [TestCase]
    public void HasLengthGreaterThan()
    {
        AssertString("This is a test message").HasLength(21, GREATER_THAN);
        AssertThrown(() => AssertString("This is a test message").HasLength(22, GREATER_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 388)
            .HasMessage("""
                Expecting length to be greater than:
                    '22' but is '22'
                """);
        AssertThrown(() => AssertString(null).HasLength(22, GREATER_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 395)
            .HasMessage("""
                Expecting length to be greater than:
                    '22' but is unknown
                """);
    }

    [TestCase]
    public void HasLengthGreaterEqual()
    {
        AssertString("This is a test message").HasLength(21, GREATER_EQUAL);
        AssertString("This is a test message").HasLength(22, GREATER_EQUAL);
        AssertThrown(() => AssertString("This is a test message").HasLength(23, GREATER_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 409)
            .HasMessage("""
                Expecting length to be greater than or equal:
                    '23' but is '22'
                """);
        AssertThrown(() => AssertString(null).HasLength(23, GREATER_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 416)
            .HasMessage("""
                Expecting length to be greater than or equal:
                    '23' but is unknown
                """);
    }

    [TestCase]
    public void Fluent()
        => AssertString("value a").HasLength(7)
            .IsNotEqual("a")
            .IsEqual("value a")
            .IsNotNull();

    [TestCase]
    public void OverrideFailureMessage()
        => AssertThrown(() => AssertString("").OverrideFailureMessage("Custom failure message").IsNull())
            .IsInstanceOf<TestFailedException>()
            .HasPropertyValue("LineNumber", 434)
            .HasMessage("Custom failure message");

    [TestCase]
    public void InterruptIsFailure()
    {
        // we disable failure reporting until we simulate an failure
        if (ExecutionContext.Current != null)
            ExecutionContext.Current.FailureReporting = false;
        // try to fail
        AssertString("").IsNotEmpty();

        // expect this line will never called because of the test is interrupted by a failing assert
        AssertBool(true).OverrideFailureMessage("This line should never be called").IsFalse();
    }
}
