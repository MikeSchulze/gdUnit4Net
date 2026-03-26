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

    private static IExtensionContext CreateSuiteContext() =>
        new ExtensionContext(typeof(ExtensionContextTest), SuiteNode);

    private static IExtensionContext CreateTestContext(IExtensionContext parent, string methodName, List<object?> args) =>
        new ExtensionContext(parent, GetMethod(methodName), methodName, args);

    [TestCase]
    public void TestConstructorAtSuiteLevel()
    {
        var context = CreateSuiteContext();
        AssertThat(context.GetTestSuiteType()).IsEqual(typeof(ExtensionContextTest));
        AssertThat(context.GetTestSuiteInstance()).IsEqual(SuiteNode);
        AssertObject(context.GetTestMethod()).IsNull();
        AssertObject(context.GetTestCaseName()).IsNull();
        AssertObject(context.GetTestCaseArguments()).IsNull();
        AssertObject(context.GetStore()).IsNotNull();
        AssertInt(context.GetStore().Count()).IsEqual(0);
    }


    [TestCase]
    public void TestConstructorAtTestLevel()
    {
        var suite = CreateSuiteContext();
        var test = CreateTestContext(suite, nameof(TestConstructorAtTestLevel), []);
        AssertThat(test.GetTestSuiteType()).IsEqual(typeof(ExtensionContextTest));
        AssertThat(test.GetTestSuiteInstance()).IsEqual(SuiteNode);
        AssertThat(test.GetTestMethod()).IsEqual(GetMethod(nameof(TestConstructorAtTestLevel)));
        AssertString(test.GetTestCaseName()).IsEqual(nameof(TestConstructorAtTestLevel));
        AssertThat(test.GetTestCaseArguments()).IsNotNull();
        AssertInt(test.GetTestCaseArguments()!.Count).IsEqual(0);
        
        suite.GetStore().Add("key", "suite-value");
        AssertString(test.GetStore().Value<string>("key")).IsEqual("suite-value");
        
        
        List<object?> args = ["hello", 42, null];
        var argsTest = CreateTestContext(suite, nameof(TestConstructorAtTestLevel), args);
        AssertThat(argsTest.GetTestCaseArguments()).IsNotNull();
        AssertThat(argsTest.GetTestCaseArguments()!).ContainsExactly("hello", 42, null);
    }
}
