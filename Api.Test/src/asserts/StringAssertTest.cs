namespace GdUnit4.Tests.Asserts;

using GdUnit4.Core.Execution;
using GdUnit4.Core.Execution.Exceptions;

using static Assertions;

using static GdUnit4.Asserts.IStringAssert.Compare;

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
            .HasFileLineNumber(18)
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
            .HasFileLineNumber(33)
            .HasMessage("Expecting be NOT <Null>:");
    }

    [TestCase]
    public void IsEqual()
    {
        AssertString("This is a test message").IsEqual("This is a test message");
        AssertThrown(() => AssertString("This is a test message").IsEqual("This is a test Message"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(43)
            .HasMessage("""
                        Expecting be equal:
                            "This is a test Message"
                         but is
                            "This is a test message"
                        """);
        AssertThrown(() => AssertString(null).IsEqual("This is a test Message"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(52)
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
            .HasFileLineNumber(67)
            .HasMessage("""
                        Expecting be equal (ignoring case):
                            "This is a Message"
                         but is
                            "This is a test message"
                        """);
        AssertThrown(() => AssertString(null).IsEqualIgnoringCase("This is a Message"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(76)
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
            .HasFileLineNumber(92)
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
            .HasFileLineNumber(108)
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
            .HasFileLineNumber(124)
            .HasMessage("""
                        Expecting be empty:
                         but is
                            " "
                        """);
        AssertThrown(() => AssertString("abc").IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(132)
            .HasMessage("""
                        Expecting be empty:
                         but is
                            "abc"
                        """);
        AssertThrown(() => AssertString(null).IsEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(140)
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
            .HasFileLineNumber(158)
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
            .HasFileLineNumber(172)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         do contains
                            "a Test"
                        """);
        AssertThrown(() => AssertString(null).Contains("a Test"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(181)
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
            .HasFileLineNumber(197)
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
            .HasFileLineNumber(212)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         do contains (ignoring case)
                            "a Text"
                        """);
        AssertThrown(() => AssertString(null).ContainsIgnoringCase("a Text"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(221)
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
            .HasFileLineNumber(237)
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
            .HasFileLineNumber(252)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         to start with
                            "This iss"
                        """);
        AssertThrown(() => AssertString("This is a test message").StartsWith("this is"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(261)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         to start with
                            "this is"
                        """);
        AssertThrown(() => AssertString("This is a test message").StartsWith("test"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(270)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         to start with
                            "test"
                        """);
        AssertThrown(() => AssertString(null).StartsWith("test"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(279)
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
            .HasFileLineNumber(294)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         to end with
                            "tes message"
                        """);
        AssertThrown(() => AssertString("This is a test message").EndsWith("a test"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(303)
            .HasMessage("""
                        Expecting:
                            "This is a test message"
                         to end with
                            "a test"
                        """);
        AssertThrown(() => AssertString(null).EndsWith("a test"))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(312)
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
            .HasFileLineNumber(328)
            .HasMessage("""
                        Expecting length:
                            '23' but is '22'
                        """);
        AssertThrown(() => AssertString(null).HasLength(23))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(335)
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
            .HasFileLineNumber(349)
            .HasMessage("""
                        Expecting length to be less than:
                            '22' but is '22'
                        """);
        AssertThrown(() => AssertString(null).HasLength(22, LESS_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(356)
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
            .HasFileLineNumber(370)
            .HasMessage("""
                        Expecting length to be less than or equal:
                            '21' but is '22'
                        """);
        AssertThrown(() => AssertString(null).HasLength(21, LESS_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(377)
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
            .HasFileLineNumber(390)
            .HasMessage("""
                        Expecting length to be greater than:
                            '22' but is '22'
                        """);
        AssertThrown(() => AssertString(null).HasLength(22, GREATER_THAN))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(397)
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
            .HasFileLineNumber(411)
            .HasMessage("""
                        Expecting length to be greater than or equal:
                            '23' but is '22'
                        """);
        AssertThrown(() => AssertString(null).HasLength(23, GREATER_EQUAL))
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(418)
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
            .HasFileLineNumber(436)
            .HasMessage("Custom failure message");

    [TestCase]
    public void AppendFailureMessage()
        => AssertThrown(() => AssertString("")
                .AppendFailureMessage("custom data")
                .IsNotEmpty())
            .IsInstanceOf<TestFailedException>()
            .HasFileLineNumber(443)
            .HasMessage("""
                        Expecting being NOT empty:
                         but is empty

                        Additional info:
                        custom data
                        """);

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
