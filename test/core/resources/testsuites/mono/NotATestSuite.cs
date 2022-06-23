namespace GdUnit3.Tests.Resource
{
    using static GdUnit3.Assertions;

    // will be ignored because of missing `[TestSuite]` anotation
    public class NotATestSuite
    {
        [TestCase]
        public void TestFoo()
        {
            AssertBool(true).IsEqual(false);
        }
    }
}
