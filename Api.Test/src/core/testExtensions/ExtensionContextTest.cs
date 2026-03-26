using System;
using System.Collections.Generic;
using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionContextTest
{
    private static IExtensionContext MakeContext(IExtensionContext? parentContext = null, string? methodName = null)
    {
        if (parentContext == null)
            return new ExtensionContext(
                typeof(ExtensionContextTest),
                new TestSuiteNode
                {
                    ManagedType = "",
                    Tests = [],
                    AssemblyPath = "",
                    SourceFile = "",
                    Id = Guid.NewGuid(),
                    ParentId = Guid.NewGuid()
                });

        var method = typeof(ExtensionContextTest).GetMethod(methodName ?? nameof(MakeContext));
        return new ExtensionContext(
            parentContext,
            method,
            method?.Name,
            []);
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestStorageThrowsWhenNoSuiteOrTestLevelDataFound()
    {
        var context = MakeContext();
        context.GetStore().Value<object>("notARealKey");
    }


    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestStorageThrowsWhenTypeDoesNotMatch()
    {
        var context = MakeContext();
        
        // Store a string value at the test level
        context.GetStore().Add("stringKey", "This is a string");

        // Attempt to retrieve it as an int, which should throw
        context.GetStore().Value<List<string>>("stringKey");
    }


    [TestCase]
    public void TestStorageUsesParentContextWhenNotFound()
    {
        var suiteContext = MakeContext();
        var testContext = MakeContext(suiteContext, nameof(TestStorageUsesParentContextWhenNotFound));
        
        suiteContext.GetStore().Add("suiteKey", "suiteValue");
        
        var result = testContext.GetStore().Value<string>("suiteKey");
        
        AssertString(result)
            .IsEqual("suiteValue");
    }


    [TestCase]
    public void TestStorageUsesTestLevelValueWhenFound()
    {
        var suiteContext = MakeContext();
        var testContext = MakeContext(suiteContext, nameof(TestStorageUsesTestLevelValueWhenFound));
        
        suiteContext.GetStore().Add("key", "suite-level value");
        testContext.GetStore().Add("key", "test-level value");

        AssertThat(suiteContext.GetStore().Value<string>("key"))
            .IsEqual("suite-level value");

        AssertThat(testContext.GetStore().Value<string>("key"))
            .IsEqual("test-level value");
        
        testContext.GetStore().Remove<string>("key");
        
        AssertThat(testContext.GetStore().Value<string>("key"))
            .IsEqual("suite-level value");
    }

    [TestCase]
    public void TestDeleteReturnsStoredValue()
    {
        var context = MakeContext();
        
        context.GetStore().Add("key", "value");
        
        var countAfterAdding = context.GetStore().Count();

        var result = context.GetStore().Remove<string>("key");
        
        var countAfterDeleting = context.GetStore().Count();

        AssertString(result)
            .IsEqual("value");

        AssertInt(countAfterAdding - countAfterDeleting)
            .IsEqual(1);
    }

    [TestCase]
    public void TestDeleteReturnsDefaultWhenKeyNotFound()
    {
        var context = MakeContext();
        
        var result = context.GetStore().Remove<string>("nonexistent");

        AssertObject(result)
            .IsNull();
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException))]
    public void TestRemoveCascadesToSuiteLevel()
    {
        // Populate suite-level storage by using a suite-level context (TestCaseName == null)
        var suiteContext = MakeContext();
        suiteContext.GetStore().Add("shared", "suite-value");

        // Test-level context initialised from suite data
        var testContext = MakeContext(suiteContext, nameof(TestRemoveCascadesToSuiteLevel));

        // Remove at test level
        testContext.GetStore().Remove<string>("shared");

        // This should throw as the parent entry will have been removed
        testContext.GetStore().Value<string>("shared");
    }
}
