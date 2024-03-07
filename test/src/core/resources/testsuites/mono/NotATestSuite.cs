namespace GdUnit4.Tests.Resources;

using static GdUnit4.Assertions;

// will be ignored because of missing `[TestSuite]` annotation
public class NotATestSuite
{
    [TestCase]
    public void TestFoo()
        => AssertBool(true).IsEqual(false);
}
