namespace GdUnit4.Tests.Core;

using System.Threading.Tasks;

using static Assertions;
using static Utils;

[TestSuite]
public class ExampleTestSuite
{
    [Before]
    public void Before()
    {
        // GD.PrintS("calling Before");
    }

    [After]
    public void After()
    {
        //GD.PrintS("calling After");
    }

    [BeforeTest]
    public void BeforeTest()
    {
        //GD.PrintS("calling BeforeTest");
    }

    [AfterTest]
    public void AfterTest()
    {
        // GD.PrintS("calling AfterTest");
    }

    [TestCase]
    public void TestFoo()
    {
#pragma warning disable IDE0022 // Use expression body for method
        AssertBool(true).IsEqual(true);
#pragma warning restore IDE0022 // Use expression body for method
    }

    [TestCase]
    public void TestBar()
        => AssertBool(true).IsEqual(true);

    [TestCase]
    public async Task Waiting()
        => await DoWait(200);

    [TestCase(TestName = "Customized")]
    public void TestFooBar()
        => AssertBool(true).IsEqual(true);

    [TestCase(1, 2, 3, 6)]
    [TestCase(3, 4, 5, 12)]
    [TestCase(6, 7, 8, 21)]
    public void TestCaseArguments(int a, int b, int c, int expect)
        => AssertThat(a + b + c).IsEqual(expect);

    [TestCase(1, 2, 3, 6, TestName = "TestCaseA")]
    [TestCase(3, 4, 5, 12, TestName = "TestCaseB")]
    [TestCase(6, 7, 8, 21, TestName = "TestCaseC")]
    public void TestCasesWithCustomTestName(int a, double b, int c, int expect)
    {
#pragma warning disable IDE0022 // Use expression body for method
        AssertThat(a + b + c).IsEqual(expect);
#pragma warning restore IDE0022 // Use expression body for method
    }

    [TestCase(true)]
    public void ParameterizedSingleTest(bool value)
        => AssertThat(value).IsTrue();
}
