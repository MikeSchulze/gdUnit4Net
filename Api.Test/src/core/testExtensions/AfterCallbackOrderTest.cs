using System.Collections.Generic;
using System.Threading.Tasks;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<AfterCallbackOrderTest.ExtA>]
[ExtendWith<AfterCallbackOrderTest.ExtB>]
public class AfterCallbackOrderTest
{
    private static bool extBAfterAllHasRun = false;
    
    // Reset by ExtA.BeforeEach before each test; written by AfterEach callbacks afterwards.
    private static readonly List<string> AfterEachLog = [];

    // Written only by AfterAll callbacks — should be empty when [After] runs.
    private static readonly List<string> AfterAllLog = [];

    [AfterTest]
    public void TestAfterEachHasNotRunBeforeAfterTest()
    {
        // [AfterTest] fires before AfterEach callbacks, so the log must still be empty.
        AssertBool(AfterEachLog.Count == 0)
            .IsTrue();
    }

    [After]
    public void TestAfterAllHasNotRunBeforeAfter()
    {
        // AfterEach:B (second registration) must appear before AfterEach:A (first registration)
        // because After* callbacks run in reversed registration order.
        var bIdx = AfterEachLog.IndexOf("B");
        var aIdx = AfterEachLog.IndexOf("A");

        AssertBool(bIdx >= 0)
            .IsTrue();
        AssertBool(aIdx >= 0)
            .IsTrue();
        AssertBool(bIdx < aIdx)
            .IsTrue();

        // AfterAll callbacks run after the [After] hook, so they must not have fired yet.
        AssertBool(AfterAllLog.Count == 0)
            .IsTrue();
    }

    [TestCase]
    public void TestCaseToTriggerCallbacks()
    {
        // This case mostly exists so that the callbacks run
        AssertBool(AfterEachLog.Count == 0)
            .IsTrue();
    }


    private class ExtA : IBeforeEachCallback, IAfterEachCallback, IAfterAllCallback
    {
        public Task BeforeEach(IExtensionContext context)
        {
            // Reset the per-test log so [AfterTest] and the test body see a clean state.
            AfterEachLog.Clear();
            return Task.CompletedTask;
        }

        public Task AfterEach(IExtensionContext context)
        {
            AfterEachLog.Add("A");
            return Task.CompletedTask;
        }

        public Task AfterAll(IExtensionContext context)
        {
            AfterAllLog.Add("A");
            AssertBool(extBAfterAllHasRun)
                .IsTrue();
            extBAfterAllHasRun = false; // Reset for potential subsequent tests
            return Task.CompletedTask;
        }
    }
    
    private class ExtB : IAfterEachCallback, IAfterAllCallback
    {
        // AfterEach for ExtB runs BEFORE ExtA (reversed order), so "B" appears first in the log.
        public Task AfterEach(IExtensionContext context)
        {
            AfterEachLog.Add("B");
            return Task.CompletedTask;
        }

        public Task AfterAll(IExtensionContext context)
        {
            AfterAllLog.Add("B");
            extBAfterAllHasRun = true;
            return Task.CompletedTask;
        }
    }
}
