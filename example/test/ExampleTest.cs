using GdUnit4;

namespace Spike;

[TestSuite]
public class Tests
{
    [TestCase]
    public static void TestFoo()
    {
        Assertions.AssertThat(5).IsEqual(5);
    }
}
