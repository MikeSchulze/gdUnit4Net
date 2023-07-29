namespace GdUnit4.Tests.Asserts
{
    using System.Collections.Generic;
    using static Assertions;
    using static Utils;

    [TestSuite]
    public class TestEventTest
    {

        [TestCase]
        public void AsDictionary()
        {
            var statistics = TestEvent.BuildStatistics(
                0,
                false, 0,
                false, 0,
                false,
                false, 0,
                1000);
            List<TestReport> reports = new List<TestReport> {
                new TestReport(TestReport.TYPE.SUCCESS, 12, "success")
            };
            AssertObject(TestEvent
                .Before("res://foo/TestSuite.cs", "TestSuite", 42).AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
            AssertObject(TestEvent
                .After("res://foo/TestSuite.cs", "TestSuite", new Dictionary<string, object>() { }, new List<TestReport> { }).AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
            AssertObject(TestEvent
                .After("res://foo/TestSuite.cs", "TestSuite", statistics, reports).AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
            AssertObject(TestEvent
                .BeforeTest("res://foo/TestSuite.cs", "TestSuite", "TestA").AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
            AssertObject(TestEvent
                .AfterTest("res://foo/TestSuite.cs", "TestSuite", "TestA").AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
            AssertObject(TestEvent
                .AfterTest("res://foo/TestSuite.cs", "TestSuite", "TestA", statistics, reports).AsDictionary())
                .IsInstanceOf<Godot.Collections.Dictionary>()
                .IsNotNull();
        }
    }
}
