// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using System.Threading.Tasks;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class AfterAllCallbackTest
{
    [RegisterExtension]
    private static readonly AfterAllExtension Extension = new AfterAllExtension();

    private static bool _testRunBeforeAfterAll = false;

    private static bool _afterTestRunBeforeAfterAll = false;

    private static bool _afterRunBeforeAfterAll = false;

    private class AfterAllExtension : IAfterAllCallback, IAfterEachCallback
    {
        public bool EachInvoked { get; private set; }
        
        public bool WasInvoked { get; set; }
        
        public Task AfterEach(IExtensionContext context)
        {
            EachInvoked = true;
            return Task.CompletedTask;
        }
        
        public Task AfterAll(IExtensionContext context)
        {
            AssertThat(EachInvoked)
                .IsTrue();
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }


    [BeforeTest]
    public void ClearFlags()
    {
        _testRunBeforeAfterAll = false;
        _afterTestRunBeforeAfterAll = false;
        _afterRunBeforeAfterAll = false;
        Extension.WasInvoked = false;
    }


    [AfterTest]
    public void BeforeRunningAfterTest_AfterAllCallback_ShouldNotBeInvoked()
    {
        _afterTestRunBeforeAfterAll = !Extension.WasInvoked;
        AssertThat(_afterTestRunBeforeAfterAll)
            .IsTrue();
    }


    [After]
    public void BeforeRunningAfter_AfterAllCallback_ShouldNotBeInvoked()
    {
        _afterRunBeforeAfterAll = !Extension.WasInvoked;
        AssertThat(_afterRunBeforeAfterAll)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningTest_AfterAllCallback_ShouldNotBeInvoked()
    {
        _testRunBeforeAfterAll = !Extension.WasInvoked;
        AssertThat(_testRunBeforeAfterAll)
            .IsTrue();
    }
}
