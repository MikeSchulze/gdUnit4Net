using System;
using System.Collections.Generic;
using GdUnit4.Api;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[ExtendWith<ContextParameterResolver>]
[ExtendWith<CompletionTrackerExtension>]
public class ExtensionContextStateTest
{
    private const string CUSTOM_TEST_NAME = "TestContextHonorsCustomTestName";

    // Random number used to verify test context suite instance.
    private int randomNumber;


    [BeforeTest]
    public void SetRandomNumber()
    {
        randomNumber = Random.Shared.Next();
    }


    [TestCase]
    public void TestStorageReturnsNullWhenNoSuiteOrTestLevelDataFound(IExtensionContext context)
    {
        var result = context.Retrieve<object>("notARealKey");
        AssertObject(result)
            .IsNull();
    }


    [TestCase]
    public void TestStorageReturnsNullWhenTypeDoesNotMatch(IExtensionContext context)
    {
        // Store a string value at the test level
        context.Store("stringKey", "This is a string");

        // Attempt to retrieve it as an int, which should return null
        var result = context.Retrieve<List<string>>("stringKey");
        AssertObject(result)
            .IsNull();
    }


    [TestCase]
    public void TestStorageUsesSuiteLeveWhenNotFound(IExtensionContext context)
    {
        // CompletionTrackerExtension sets this to true in the BeforeAll callback,
        // so it should be available here at the suite level
        var beforeAllExecuted = context.Retrieve<bool>(CompletionTrackerExtension.BeforeAllExecuted);
        AssertBool(beforeAllExecuted)
            .IsTrue();
    }


    [TestCase]
    public void TestStorageUsesTestLevelValueWhenFound(IExtensionContext context)
    {
        var old = context.Retrieve<bool>(CompletionTrackerExtension.BeforeAllExecuted);

        // Store a value at the test level with the same key as the
        // BeforeAllExecuted value set by CompletionTrackerExtension
        context.Store(CompletionTrackerExtension.BeforeAllExecuted, "foobar");

        // Retrieve the value again, it should return the test level value instead of the suite level value
        var preDelete = context.Retrieve<object?>(CompletionTrackerExtension.BeforeAllExecuted);

        // Delete the test-level value, to ensure that the suite level is used on the next retrieval
        context.Delete<object>(CompletionTrackerExtension.BeforeAllExecuted);

        var postDelete = context.Retrieve<bool?>(CompletionTrackerExtension.BeforeAllExecuted);

        AssertThat(old)
            .IsEqual(true);

        AssertThat(preDelete)
            .IsEqual("foobar");

        AssertThat(postDelete)
            .IsEqual(old);
    }


    [TestCase("hello", 42)]
    public void TestContextArgsMatchTestCaseArgs(IExtensionContext context, string arg1, int arg2)
    {
        AssertBool(context.TestCaseArguments.Length >= 2)
            .IsTrue();

        AssertString(context.TestCaseArguments[0]?.ToString())
            .IsEqual(arg1);

        AssertInt((int)context.TestCaseArguments[1]!)
            .IsEqual(arg2);
    }


    [TestCase]
    public void TestContextSuiteTypeShouldBeCorrect(IExtensionContext context)
    {
        AssertThat(context.TestSuiteType)
            .IsEqual(typeof(ExtensionContextStateTest));
    }


    [TestCase]
    public void TestContextSuiteInstanceShouldBeCorrect(IExtensionContext context)
    {
        AssertThat(context.TestSuiteInstance)
            .IsInstanceOf<ExtensionContextStateTest>();

        var instance = (ExtensionContextStateTest)context.TestSuiteInstance;

        AssertThat(instance.randomNumber)
            .IsEqual(randomNumber);
    }


    [TestCase]
    public void TestContextTestMethodIsCorrect(IExtensionContext context)
    {
        AssertThat(context.TestMethod)
            .IsNotNull();

        AssertThat(context.TestMethod)
            .IsEqual(GetType().GetMethod(nameof(TestContextTestMethodIsCorrect))!);
    }


    [TestCase]
    public void TestContextTestMethodShouldMatchTestName(IExtensionContext context)
    {
        AssertThat(context.TestMethod?.Name)
            .IsEqual(nameof(TestContextTestMethodShouldMatchTestName));
    }


    [TestCase(TestName = CUSTOM_TEST_NAME)]
    public void TestContextTestMethodShouldRespectTestNameParameter(IExtensionContext context)
    {
        AssertThat(context.TestCaseName)
            .IsEqual(CUSTOM_TEST_NAME);
    }


    [TestCase("leftover", 99)]
    public void TestContextTestCaseArgumentsShouldHaveLeftoverArgs(IExtensionContext context, string arg1, int arg2)
    {
        // This test is meant to verify that arguments not consumed by parameter resolvers are
        // available in the context and passed into the test method.
        AssertBool(context.TestCaseArguments.Length >= 2)
            .IsTrue();

        AssertString(context.TestCaseArguments[0]?.ToString())
            .IsEqual(arg1);

        AssertInt((int)context.TestCaseArguments[1]!)
            .IsEqual(arg2);
    }
}
