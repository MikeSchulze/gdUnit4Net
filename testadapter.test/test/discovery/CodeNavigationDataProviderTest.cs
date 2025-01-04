namespace GdUnit4.TestAdapter.Test.Discovery;

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestAdapter.Discovery;

public class ExampleGdUnitTestSuite
{
    [TestCase]
    public async Task Waiting()
        => await Task.Run(() => Console.WriteLine("TestFooBar"));

    [TestCase(TestName = "Customized")]
    public void TestFooBar()
        => Console.WriteLine("TestFooBar");
}

[TestClass]
public class CodeNavigationDataProviderTest
{
    private static CodeNavigationDataProvider NavigationDataProvider { get; set; }

    [ClassInitialize]
    public static void ClassSetup(TestContext context)
        => NavigationDataProvider = new CodeNavigationDataProvider(Assembly.GetExecutingAssembly().Location);

    [ClassCleanup]
    public static void ClassCleanup()
        => NavigationDataProvider.Dispose();

    [TestMethod]
    [DataRow(16, "Waiting")]
    [DataRow(20, "TestFooBar")]
    public void DiscoverTests(int expectedLine, string testName)
    {
        var source = GetSourceFilePath("test/discovery/CodeNavigationDataProviderTest.cs");
        var clazzType = typeof(ExampleGdUnitTestSuite);
        var mi = clazzType.GetMethod(testName)!;

        var navData = NavigationDataProvider.GetNavigationData(clazzType.FullName!, testName);
        Assert.IsNotNull(navData);
        Assert.AreEqual(new CodeNavigationDataProvider.CodeNavigation
        {
            Method = mi,
            Line = expectedLine,
            Source = source
        }, navData);
    }

    private static string GetSourceFilePath(string relativeSourcePath)
    {
        // Get the directory of the executing assembly
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var projectDir = Path.GetDirectoryName(assemblyLocation)!;

        // Navigate up to find the test file
        // Note: Adjust the path based on your project structure
        while (Directory.GetFiles(projectDir, "*.csproj").Length == 0 && Directory.GetParent(projectDir) != null)
            projectDir = Directory.GetParent(projectDir)!.FullName;

        // Find the test file in the project directory
        var sourceFile = Path.Combine(projectDir.Replace('\\', Path.DirectorySeparatorChar), relativeSourcePath.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetFullPath(sourceFile);
    }
}
