using System.Threading.Tasks;
using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionRegistryTest
{
    #region Mock extension classes for testing

    private abstract class MockExtension : IBeforeEachCallback
    {
        public Task BeforeEach(IExtensionContext context)
        {
            // No-op
            return Task.CompletedTask;
        }
    }

    private class ExtensionA : MockExtension;

    private class ExtensionB : MockExtension;
    
    private class ExtensionC : MockExtension;

    private class ExtensionD : MockExtension;

    #endregion
    
    #region Mock suite classes for testing
    
    [ExtendWith<ExtensionA>]
    private class SuiteWithExtendWith;

    private class SuiteWithRegisterExtensionField
    {
        [RegisterExtension]
        private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    private class SuiteWithBothRegistrationMethods
    {
        [RegisterExtension]
        private static readonly ITestExtension Extension = new ExtensionB();
    }

    [ExtendWith<ExtensionA>]
    [ExtendWith<ExtensionB>]
    private class AbstractSuite;

    [ExtendWith<ExtensionC>]
    [ExtendWith<ExtensionD>]
    private class ConcreteSuite : AbstractSuite;

    private class SuiteWithNoExtensions;
    
    #endregion

    #region Test setup

    private ExtensionRegistry extensionRegistry = null!;

    [BeforeTest]
    public void CreateExtensionRegistry()
    {
        extensionRegistry = new ExtensionRegistry();
    }

    #endregion
    
    #region Test FindTestExtensions

    [TestCase]
    public void FindTestExtensions_WhenCalledOnSuiteWithNoExtensions_ReturnsEmpty()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithNoExtensions));
        
        AssertThat(result).IsEmpty();
    }


    [TestCase]
    public void FindTestExtensions_WhenCalledOnSuiteWithExtendWith_ReturnsExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithExtendWith));
        
        AssertThat(result).IsNotEmpty();
        AssertThat(result[0].GetType())
            .IsEqual(typeof(ExtensionA));
    }
    
    [TestCase]
    public void FindTestExtensions_WhenCalledOnSuiteWithBothExtensions_ReturnsExtensionsInCorrectOrder()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithBothRegistrationMethods));
        
        AssertThat(result).IsNotEmpty();
        AssertThat(result[0].GetType())
            .IsEqual(typeof(ExtensionA));
        AssertThat(result[1].GetType())
            .IsEqual(typeof(ExtensionB));
    }

    [TestCase]
    public void FindTestExtensions_CollectsInheritedExtensionsInCorrectOrder()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(ConcreteSuite));
        
        AssertThat(result).IsNotEmpty();
        AssertInt(result.Count).IsEqual(4);
        AssertThat(result[0].GetType())
            .IsEqual(typeof(ExtensionA));
        AssertThat(result[1].GetType())
            .IsEqual(typeof(ExtensionB));
        AssertThat(result[2].GetType())
            .IsEqual(typeof(ExtensionC));
        AssertThat(result[3].GetType())
            .IsEqual(typeof(ExtensionD));
    }

    [TestCase]
    public void FindTestExtensions_WhenCalledOnSuiteWithRegisterExtension_ReturnsExtension()
    {
        var result = extensionRegistry
            .FindTestExtensions(typeof(SuiteWithRegisterExtensionField));
        
        AssertThat(result).IsNotEmpty();
        
        AssertThat(result[0].GetType())
            .IsEqual(typeof(ExtensionB));
    }

    #endregion
}