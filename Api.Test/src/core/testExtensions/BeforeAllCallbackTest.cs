// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using System.Threading.Tasks;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class BeforeAllCallbackTest
{
    [RegisterExtension]
    private static readonly BeforeAllExtension Extension = new BeforeAllExtension();

    private bool beforeInvokedAfterBeforeAll = false;

    private bool beforeTestInvokedAfterBeforeAll = false;

    [Before]
    public void Before() => beforeInvokedAfterBeforeAll = Extension.WasInvoked;

    [BeforeTest]
    public void BeforeTest() => beforeTestInvokedAfterBeforeAll = Extension.WasInvoked;

    private class BeforeAllExtension : IBeforeAllCallback
    {
        public bool WasInvoked { get; private set; }
        
        public Task BeforeAll(IExtensionContext context)
        {
            WasInvoked = true;
            return Task.CompletedTask;
        }
    }

    [TestCase]
    public void BeforeRunningTest_BeforeAllCallback_ShouldBeInvoked()
    {
        AssertThat(Extension.WasInvoked)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningBeforeMethod_BeforeAllCallback_ShouldBeInvoked()
    {
        AssertThat(beforeInvokedAfterBeforeAll)
            .IsTrue();
    }

    [TestCase]
    public void BeforeRunningBeforeTestMethod_BeforeAllCallback_ShouldBeInvoked()
    {
        AssertThat(beforeTestInvokedAfterBeforeAll)
            .IsTrue();
    }
}
