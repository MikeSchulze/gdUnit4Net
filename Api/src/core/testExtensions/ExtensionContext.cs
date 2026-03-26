// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Collections.ObjectModel;
using System.Reflection;

using Api;

internal class ExtensionContext : IExtensionContext
{
    private readonly Type testSuiteType;

    private readonly TestSuiteNode testSuiteInstance;

    private readonly MethodInfo? testMethod;

    private readonly IParameterStore parameterStore;

    private readonly string? testCaseName;

    private readonly ReadOnlyCollection<object?>? testCaseArguments;

    public ExtensionContext(IExtensionContext parentContext, MethodInfo testMethod, string testCaseName, List<object?> testCaseArguments)
    {
        parameterStore = new ParameterStore(parentContext.GetStore());
        testSuiteType = parentContext.GetTestSuiteType();
        testSuiteInstance = parentContext.GetTestSuiteInstance();
        this.testMethod = testMethod;
        this.testCaseName = testCaseName;
        this.testCaseArguments = new ReadOnlyCollection<object?>(testCaseArguments);
    }

    public ExtensionContext(Type testSuiteType, TestSuiteNode testSuiteInstance)
    {
        parameterStore = new ParameterStore();
        this.testSuiteType = testSuiteType;
        this.testSuiteInstance = testSuiteInstance;
        testMethod = null;
        testCaseName = null;
        testCaseArguments = null;
    }

    public Type GetTestSuiteType() => testSuiteType;

    public TestSuiteNode GetTestSuiteInstance() => testSuiteInstance;

    public MethodInfo? GetTestMethod() => testMethod;

    public string? GetTestCaseName() => testCaseName;

    public ReadOnlyCollection<object?>? GetTestCaseArguments() => testCaseArguments;

    public IParameterStore GetStore() => parameterStore;
}
