// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Collections.ObjectModel;
using System.Reflection;

using Api;

internal class ExtensionRegistry(Type testSuiteType, object testSuiteInstance)
{
    private readonly List<ITestExtension> suiteExtensions = CollectSuiteExtensions(testSuiteType, testSuiteInstance);

    public async Task RunBeforeAll(IExtensionContext context)
    {
        var beforeAllExtensions = suiteExtensions
            .OfType<IBeforeAllCallback>();
        foreach (var ext in beforeAllExtensions)
            await ext.BeforeAll(context).ConfigureAwait(true);
    }

    public async Task RunAfterAll(IExtensionContext context)
    {
        var afterAllCallbacks = suiteExtensions
            .OfType<IAfterAllCallback>()
            .Reverse();
        foreach (var ext in afterAllCallbacks)
            await ext.AfterAll(context).ConfigureAwait(true);
    }

    public async Task RunBeforeEach(IExtensionContext context)
    {
        var testMethod = context.GetTestMethod()
                         ?? throw new InvalidOperationException("Test method information is required in the extension context for BeforeEach callbacks.");
        var beforeEachCallbacks = GetExtensionsForMethod(suiteExtensions, testMethod)
            .OfType<IBeforeEachCallback>();
        foreach (var ext in beforeEachCallbacks)
            await ext.BeforeEach(context).ConfigureAwait(true);
    }

    public async Task RunAfterEach(IExtensionContext context)
    {
        var testMethod = context.GetTestMethod()
            ?? throw new InvalidOperationException("Test method information is required in the extension context for AfterEach callbacks.");
        var afterEachCallbacks = GetExtensionsForMethod(suiteExtensions, testMethod)
            .OfType<IAfterEachCallback>()
            .Reverse();
        foreach (var ext in afterEachCallbacks)
            await ext.AfterEach(context).ConfigureAwait(true);
    }

    public ReadOnlyCollection<object?>? ResolveArguments(IExtensionContext testContext)
    {
        var testMethod = testContext.GetTestMethod()
            ?? throw new InvalidOperationException("Test method information is required in the extension context to resolve arguments.");

        var parameters = testMethod.GetParameters();
        if (parameters.Length == 0)
            return new ReadOnlyCollection<object?>([]);

        var resolvers = GetExtensionsForMethod(suiteExtensions, testMethod)
            .OfType<IParameterResolver>()
            .ToList();

        return resolvers.Count == 0 ? testContext.GetTestCaseArguments() : ResolveArguments(testContext, resolvers, parameters);
    }

    private static ReadOnlyCollection<object?> ResolveArguments(
        IExtensionContext testContext,
        List<IParameterResolver> resolvers,
        ParameterInfo[] parameters)
    {
        var testCaseArguments = testContext.GetTestCaseArguments()
                                ?? new ReadOnlyCollection<object?>([]);

        var resolvedArgs = new List<object?>(parameters.Length);
        var remainingArgs = testCaseArguments;

        foreach (var param in parameters)
        {
            var parameterContext = new ParameterContext(param);
            var (resolved, remaining) = ResolveParameter(testContext, parameterContext, resolvers, remainingArgs);
            resolvedArgs.Add(resolved);
            remainingArgs = remaining;
        }

        return new ReadOnlyCollection<object?>(resolvedArgs);
    }

    private static Tuple<object?, ReadOnlyCollection<object?>> ResolveParameter(
        IExtensionContext extensionContext,
        ParameterContext parameterContext,
        List<IParameterResolver> resolvers,
        ReadOnlyCollection<object?> remainingTestCaseArgs)
    {
        var remainingArgs = remainingTestCaseArgs.ToList();
        var testMethod = extensionContext.GetTestMethod()!;
        var param = parameterContext.GetParameterInfo();

        // Priority 1: first IParameterResolver that supports this parameter
        var resolver = resolvers.FirstOrDefault(r => r.SupportsParameter(parameterContext, extensionContext));
        if (resolver != null)
        {
            var result = resolver.ResolveParameter(parameterContext, extensionContext);
            return new Tuple<object?, ReadOnlyCollection<object?>>(result, new ReadOnlyCollection<object?>(remainingArgs));
        }

        // Priority 2: type-compatible match from remaining TestCase arguments
        var matchIndex = remainingArgs.FindIndex(a => a != null && param.ParameterType.IsInstanceOfType(a));
        if (matchIndex >= 0)
        {
            var result = remainingArgs[matchIndex];
            remainingArgs.RemoveAt(matchIndex);
            return new Tuple<object?, ReadOnlyCollection<object?>>(result, new ReadOnlyCollection<object?>(remainingArgs));
        }

        throw new InvalidOperationException(
            $"No value could be resolved for parameter '{param.Name}' of type '{param.ParameterType.Name}' " +
            $"in test '{testMethod.DeclaringType?.Name}.{testMethod.Name}'. " +
            "Provide a value via [TestCase(...)] or register an IParameterResolver extension.");
    }

    private static List<ITestExtension> GetExtensionsForMethod(List<ITestExtension> suiteExtensions, MethodInfo method)
    {
        var methodExtensions = new List<ITestExtension>();
        methodExtensions.AddRange(CollectMethodLevelExtendWithExtensions(method));

        var combined = new List<ITestExtension>(suiteExtensions.Count + methodExtensions.Count);
        combined.AddRange(suiteExtensions);
        combined.AddRange(methodExtensions);
        return combined;
    }

    private static List<ITestExtension> CollectSuiteExtensions(Type type, object instance)
    {
        var result = new List<ITestExtension>();
        result.AddRange(CollectClassLevelExtendWithExtensions(type));
        result.AddRange(CollectFieldLevelRegisterExtensionExtensions(type, instance));
        result.AddRange(CollectPropertyLevelRegisterExtensionExtensions(type, instance));
        return result;
    }

    private static List<ITestExtension> CollectPropertyLevelRegisterExtensionExtensions(Type type, object instance)
    {
        // [RegisterExtension] properties
        var propertyLevelRegisterExtensionExtensions = new List<ITestExtension>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!prop.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (prop.GetValue(prop.GetMethod?.IsStatic == true ? null : instance) is ITestExtension ext)
                propertyLevelRegisterExtensionExtensions.Add(ext);
        }

        return propertyLevelRegisterExtensionExtensions;
    }

    private static List<ITestExtension> CollectFieldLevelRegisterExtensionExtensions(Type type, object instance)
    {
        // [RegisterExtension] fields — supports constructor arguments; same instance reused for all tests
        var fieldLevelRegisterExtensionExtensions = new List<ITestExtension>();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!field.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (field.GetValue(field.IsStatic ? null : instance) is ITestExtension ext)
                fieldLevelRegisterExtensionExtensions.Add(ext);
        }

        return fieldLevelRegisterExtensionExtensions;
    }

    private static List<ITestExtension> CollectClassLevelExtendWithExtensions(Type type)
    {
        // Class-level [ExtendWith<T>] — one instance per suite, collected in declaration order
        var classLevelExtendWithAttributes = new List<ITestExtension>();
        foreach (var attr in type.GetCustomAttributes(inherit: true))
        {
            var attrType = attr.GetType();
            if (!attrType.IsGenericType || attrType.GetGenericTypeDefinition() != typeof(ExtendWithAttribute<>))
                continue;
            var extensionType = attrType.GetGenericArguments()[0];
            classLevelExtendWithAttributes.Add((ITestExtension)Activator.CreateInstance(extensionType)!);
        }

        return classLevelExtendWithAttributes;
    }

    private static List<ITestExtension> CollectMethodLevelExtendWithExtensions(MethodInfo method)
    {
        var methodLevelExtendWithExtensions = new List<ITestExtension>();
        foreach (var attr in method.GetCustomAttributes())
        {
            var attrType = attr.GetType();
            if (!attrType.IsGenericType || attrType.GetGenericTypeDefinition() != typeof(ExtendWithAttribute<>))
                continue;
            var extensionType = attrType.GetGenericArguments()[0];
            methodLevelExtendWithExtensions.Add((ITestExtension)Activator.CreateInstance(extensionType)!);
        }

        return methodLevelExtendWithExtensions;
    }
}
