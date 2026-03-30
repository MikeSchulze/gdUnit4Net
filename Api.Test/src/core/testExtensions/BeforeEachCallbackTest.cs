using System.Threading.Tasks;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class BeforeEachCallbackTest
{
    [RegisterExtension]
    private static readonly BeforeEachExtension Extension = new BeforeEachExtension();

    private bool beforeEachInvokedAfterBefore = false;

    private bool beforeTestInvokedAfterBeforeEach = false;

    [Before] 
    public void Before() => beforeEachInvokedAfterBefore = !Extension.WasInvoked;

    [BeforeTest]
    public void BeforeTest() => beforeTestInvokedAfterBeforeEach = Extension.WasInvoked;

    private class BeforeEachExtension : IBeforeEachCallback
    {
        public bool WasInvoked { get; private set; }
        
        public Task BeforeEach(IExtensionContext context)
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }

    [TestCase]
    public void BeforeRunningTest_BeforeEachCallback_ShouldBeInvoked()
    {
        AssertThat(Extension.WasInvoked)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningBeforeMethod_BeforeEachCallback_ShouldNotBeInvoked()
    {
        AssertThat(beforeEachInvokedAfterBefore)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningBeforeTestMethod_BeforeEachCallback_ShouldBeInvoked()
    {
        AssertThat(beforeTestInvokedAfterBeforeEach)
            .IsTrue();
    }
}
