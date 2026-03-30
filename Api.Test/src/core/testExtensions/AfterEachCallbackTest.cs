using System.Threading.Tasks;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class AfterEachCallbackTest
{
    [RegisterExtension]
    private static readonly AfterEachExtension Extension = new AfterEachExtension();

    private static bool _testRunBeforeAfterEach = false;

    private static bool _afterTestRunBeforeAfterEach = false;

    private static bool _afterEachRunBeforeAfterAll = false;

    private static bool _afterEachRunBeforeAfter = false;

    private class AfterEachExtension : IAfterEachCallback, IAfterAllCallback
    {
        public bool WasInvoked { get; set; }
        
        public Task AfterEach(IExtensionContext context)
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }


        public Task AfterAll(IExtensionContext context)
        {
            _afterEachRunBeforeAfterAll = Extension.WasInvoked;
            AssertThat(_afterEachRunBeforeAfterAll)
                .IsTrue();
            return Task.CompletedTask;
        }
    }


    [BeforeTest]
    public void ClearFlags()
    {
        _testRunBeforeAfterEach = false;
        _afterEachRunBeforeAfterAll = false;
        _afterTestRunBeforeAfterEach = false;
        Extension.WasInvoked = false;
    }


    [AfterTest]
    public void AfterTest()
    {
        _afterTestRunBeforeAfterEach = !Extension.WasInvoked;
        AssertThat(_afterTestRunBeforeAfterEach)
            .IsTrue();
    }


    [After]
    public void BeforeRunningAfter_AfterEachCallback_ShouldBeInvoked()
    {
        _afterEachRunBeforeAfter = Extension.WasInvoked;
        AssertThat(_afterEachRunBeforeAfter)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningTest_AfterEachCallback_ShouldNotBeInvoked()
    {
        _testRunBeforeAfterEach = !Extension.WasInvoked;
        AssertThat(_testRunBeforeAfterEach)
            .IsTrue();
    }
}
