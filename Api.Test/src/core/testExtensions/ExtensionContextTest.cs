using System;
using System.Collections.Generic;
using System.Reflection;
using GdUnit4.Api;
using GdUnit4.Core.TestExtensions;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
public class ExtensionContextTest
{
    private static readonly TestSuiteNode SuiteNode = new()
    {
        ManagedType = typeof(ExtensionContextTest).FullName ?? "",
        Tests = [],
        AssemblyPath = "test.dll",
        SourceFile = "ExtensionContextTest.cs",
        Id = Guid.NewGuid(),
        ParentId = Guid.NewGuid()
    };

    private static MethodInfo GetMethod(string name) =>
        typeof(ExtensionContextTest).GetMethod(name)
            ?? throw new InvalidOperationException($"Method '{name}' not found");

    private static IExtensionContext MakeSuiteContext() =>
        new ExtensionContext(typeof(ExtensionContextTest), SuiteNode);

    private static IExtensionContext MakeTestContext(IExtensionContext parent, string methodName, List<object?> args) =>
        new ExtensionContext(parent, GetMethod(methodName), methodName, args);

    #region SuiteLevelConstructor

    [TestCase]
    public void TestSuiteConstructorReturnsCorrectTestSuiteType()
    {
        var context = MakeSuiteContext();
        AssertThat(context.GetTestSuiteType()).IsEqual(typeof(ExtensionContextTest));
    }

    [TestCase]
    public void TestSuiteConstructorReturnsCorrectTestSuiteInstance()
    {
        var context = MakeSuiteContext();
        AssertThat(context.GetTestSuiteInstance()).IsEqual(SuiteNode);
    }

    [TestCase]
    public void TestSuiteConstructorReturnsNullTestMethod()
    {
        var context = MakeSuiteContext();
        AssertObject(context.GetTestMethod()).IsNull();
    }

    [TestCase]
    public void TestSuiteConstructorReturnsNullTestCaseName()
    {
        var context = MakeSuiteContext();
        AssertObject(context.GetTestCaseName()).IsNull();
    }

    [TestCase]
    public void TestSuiteConstructorReturnsNullTestCaseArguments()
    {
        var context = MakeSuiteContext();
        AssertObject(context.GetTestCaseArguments()).IsNull();
    }

    [TestCase]
    public void TestSuiteConstructorReturnsNonNullStore()
    {
        var context = MakeSuiteContext();
        AssertObject(context.GetStore()).IsNotNull();
    }

    [TestCase]
    public void TestSuiteConstructorStoreIsInitiallyEmpty()
    {
        var context = MakeSuiteContext();
        AssertInt(context.GetStore().Count()).IsEqual(0);
    }

    #endregion

    #region TestCaseConstructor


    [TestCase]
    public void TestTestConstructorInheritsTestSuiteTypeFromParent()
    {
        var suite = MakeSuiteContext();
        var test = MakeTestContext(suite, nameof(TestTestConstructorInheritsTestSuiteTypeFromParent), []);
        AssertThat(test.GetTestSuiteType()).IsEqual(typeof(ExtensionContextTest));
    }

    [TestCase]
    public void TestTestConstructorInheritsTestSuiteInstanceFromParent()
    {
        var suite = MakeSuiteContext();
        var test = MakeTestContext(suite, nameof(TestTestConstructorInheritsTestSuiteInstanceFromParent), []);
        AssertThat(test.GetTestSuiteInstance()).IsEqual(SuiteNode);
    }

    [TestCase]
    public void TestTestConstructorReturnsCorrectTestMethod()
    {
        var suite = MakeSuiteContext();
        var test = MakeTestContext(suite, nameof(TestTestConstructorReturnsCorrectTestMethod), []);
        AssertThat(test.GetTestMethod()).IsEqual(GetMethod(nameof(TestTestConstructorReturnsCorrectTestMethod)));
    }

    [TestCase]
    public void TestTestConstructorReturnsCorrectTestCaseName()
    {
        var suite = MakeSuiteContext();
        var test = MakeTestContext(suite, nameof(TestTestConstructorReturnsCorrectTestCaseName), []);
        AssertString(test.GetTestCaseName()).IsEqual(nameof(TestTestConstructorReturnsCorrectTestCaseName));
    }

    [TestCase]
    public void TestTestConstructorReturnsEmptyArguments()
    {
        var suite = MakeSuiteContext();
        var test = MakeTestContext(suite, nameof(TestTestConstructorReturnsEmptyArguments), []);
        AssertThat(test.GetTestCaseArguments()).IsNotNull();
        AssertInt(test.GetTestCaseArguments()!.Count).IsEqual(0);
    }

    [TestCase]
    public void TestTestConstructorReturnsCorrectArguments()
    {
        var suite = MakeSuiteContext();
        List<object?> args = ["hello", 42, null];
        var test = MakeTestContext(suite, nameof(TestTestConstructorReturnsCorrectArguments), args);
        AssertThat(test.GetTestCaseArguments()).IsNotNull();
        AssertThat(test.GetTestCaseArguments()!).ContainsExactly("hello", 42, null);
    }

    [TestCase]
    public void TestTestConstructorStoreIsLinkedToParentStore()
    {
        var suite = MakeSuiteContext();
        suite.GetStore().Add("key", "suite-value");
        var test = MakeTestContext(suite, nameof(TestTestConstructorStoreIsLinkedToParentStore), []);
        AssertString(test.GetStore().Value<string>("key")).IsEqual("suite-value");
    }

    #endregion

}
