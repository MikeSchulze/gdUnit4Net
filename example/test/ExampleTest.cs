namespace Spike;

using GdUnit4;

[TestSuite]
public class Tests
{
    [TestCase]
    public static void TestFoo() => Assertions.AssertThat(5).IsEqual(5);
}
