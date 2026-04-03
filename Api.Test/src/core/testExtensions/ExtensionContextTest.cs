using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionContextTest
{
    private static IExtensionContext CreateSuiteContext() => new ExtensionContext();

    private static IExtensionContext CreateTestContext(IExtensionContext parent) => new ExtensionContext(parent);

    [TestCase]
    public void TestConstructorAtSuiteLevel()
    {
        var context = CreateSuiteContext();
        AssertObject(context.GetStore()).IsNotNull();
        AssertInt(context.GetStore().Count()).IsEqual(0);
    }

    [TestCase]
    public void TestConstructorAtTestLevel()
    {
        var suite = CreateSuiteContext();
        var test = CreateTestContext(suite);
        
        suite.GetStore().Add("key", "suite-value");
        
        AssertString(test.GetStore().Value<string>("key")).IsEqual("suite-value");
    }
}
