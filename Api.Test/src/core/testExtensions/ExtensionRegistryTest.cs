using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

using System.Threading.Tasks;

using Api;

using GdUnit4.Core.TestExtensions;

[TestSuite]
public class ExtensionRegistryTest
{
    #region Mock extension classes for testing

    private abstract class MockExtension : IBeforeEachCallback
    {
        public Task BeforeEach(IExtensionContext context) =>
            // No-op
            Task.CompletedTask;
    }

    private class ExtensionA : MockExtension;

    private class ExtensionB : MockExtension;

    private class ExtensionC : MockExtension;

    private class ExtensionD : MockExtension;

    #endregion

    #region Mock suite classes for testing

    [ExtendWith<ExtensionA>]
    private class SuiteWithExtension;

    private class SuiteWithRegisterExtension
    {
        // ReSharper disable once UnusedMember.Local
        [RegisterExtension] private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    private class SuiteWithExtensionsAndRegistration
    {
        // ReSharper disable once UnusedMember.Local
        [RegisterExtension] private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    [ExtendWith<ExtensionB>]
    private class AbstractSuite;

    [ExtendWith<ExtensionC>]
    [ExtendWith<ExtensionD>]
    private class SuiteWithExtensionsInherits : AbstractSuite;

    private class SuiteWithNoExtensions;

    #endregion

    #region Test setup

    // ReSharper disable once NullableWarningSuppressionIsUsed
    private ExtensionRegistry extensionRegistry = null!;

    [BeforeTest]
    public void CreateExtensionRegistry() => extensionRegistry = new ExtensionRegistry();

    #endregion

    #region Test FindTestExtensions

    [TestCase]
    public void FindTestExtensions_OnSuiteWithNoExtensions()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithNoExtensions));

        AssertThat(result).IsEmpty();
    }


    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtension));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtensionsAndRegistration()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtensionsAndRegistration));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithExtensionsInherits()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtensionsInherits));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionA), typeof(ExtensionB), typeof(ExtensionC), typeof(ExtensionD));
    }

    [TestCase]
    public void FindTestExtensions_OnSuiteWithRegisterExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithRegisterExtension));

        AssertThat(result)
            .Extract("GetType")
            .ContainsExactly(typeof(ExtensionB));
    }

    #endregion
}
