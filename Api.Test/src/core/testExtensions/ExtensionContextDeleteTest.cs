// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Tests.Core.TestExtensions;

using GdUnit4.Core.TestExtensions;

using static GdUnit4.Assertions;

[TestSuite]
public class ExtensionContextDeleteTest
{
    private static ExtensionContext MakeContext(string testCaseName) =>
        new(typeof(ExtensionContextDeleteTest), new ExtensionContextDeleteTest(), null, testCaseName, []);

    [TestCase]
    public void TestDeleteReturnsStoredValue()
    {
        var context = MakeContext(nameof(TestDeleteReturnsStoredValue));
        context.Store("key", "value");

        var result = context.Delete<string>("key");

        AssertString(result)
            .IsEqual("value");

        AssertBool(context.DataStore.Count == 0)
            .IsTrue();
    }

    [TestCase]
    public void TestDeleteReturnsDefaultWhenKeyNotFound()
    {
        var context = MakeContext(nameof(TestDeleteReturnsDefaultWhenKeyNotFound));

        var result = context.Delete<string>("nonexistent");

        AssertObject(result)
            .IsNull();
    }

    [TestCase]
    public void TestDeleteDoesNotCascadeToSuiteLevel()
    {
        // Populate suite-level storage by using a suite-level context (TestCaseName == null)
        var suiteContext = new ExtensionContext(
            typeof(ExtensionContextDeleteTest),
            new ExtensionContextDeleteTest(),
            null,
            null,
            []);
        suiteContext.Store("shared", "suite-value");

        // Test-level context initialised from suite data
        var testContext = new ExtensionContext(
            typeof(ExtensionContextDeleteTest),
            new ExtensionContextDeleteTest(),
            null,
            nameof(TestDeleteDoesNotCascadeToSuiteLevel),
            [],
            suiteContext.DataStore);

        // Delete must not touch the suite-level entry
        testContext.Delete<string>("shared");

        AssertString(testContext.Retrieve<string>("shared"))
            .IsEqual("suite-value");
    }
}
