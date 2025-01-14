namespace GdUnit4.Tests.Core.Discovery;

using System;
using System.Reflection;
using System.Threading.Tasks;

using GdUnit4.Core.Discovery;

using static Assertions;

public class ExampleGdUnitTestSuite
{
    [TestCase]
    public async Task Waiting()
        => await Task.Run(() => Console.WriteLine("TestFooBar"));

    [TestCase(TestName = "Customized")]
    public void TestFooBar()
        => Console.WriteLine("TestFooBar");
}

[TestSuite]
public class CodeNavigationDataProviderTest
{
    private static CodeNavigationDataProvider? NavigationDataProvider { get; set; }

    [Before]
    public static void ClassSetup()
        => NavigationDataProvider = new CodeNavigationDataProvider(Assembly.GetExecutingAssembly().Location);

    [After]
    public static void ClassCleanup()
        => NavigationDataProvider?.Dispose();

    [TestCase(15, "Waiting")]
    [TestCase(19, "TestFooBar")]
    public void DiscoverTests(int expectedLine, string testName)
    {
        var source = CodeNavPath.GetSourceFilePath("src/core/discovery/CodeNavigationDataProviderTest.cs");
        var clazzType = typeof(ExampleGdUnitTestSuite);
        var mi = clazzType.GetMethod(testName)!;

        var navData = NavigationDataProvider?.GetNavigationData(mi);
        AssertThat(navData).IsNotNull();
        AssertThat(navData).IsEqual(new CodeNavigationDataProvider.CodeNavigation
        {
            Method = mi,
            Line = expectedLine,
            Source = source
        });
    }
}
