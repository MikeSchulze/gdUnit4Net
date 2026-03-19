// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

/// <summary>
/// Discovers and manages test extensions registered via <see cref="ExtendWithAttribute{T}"/> and
/// <see cref="RegisterExtensionAttribute"/>, and orchestrates their lifecycle callbacks and parameter resolution.
/// </summary>
internal class ExtensionRegistry : IExtensionRegistry
{
    private readonly List<ITestExtension> suiteExtensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionRegistry"/> class.
    /// Collects all suite-level extensions from class-level <see cref="ExtendWithAttribute{T}"/>
    /// attributes and <see cref="RegisterExtensionAttribute"/> fields/properties.
    /// </summary>
    /// <param name="testSuiteType">The test suite class type.</param>
    /// <param name="testSuiteInstance">The live test suite instance.</param>
    public ExtensionRegistry(Type testSuiteType, object testSuiteInstance)
        => suiteExtensions = CollectSuiteExtensions(testSuiteType, testSuiteInstance);

    /// <summary>
    /// Runs <see cref="IBeforeAllCallback.BeforeAll"/> for all suite-level extensions in registration order.
    /// </summary>
    public async Task RunBeforeAll(IExtensionContext context)
    {
        var beforeAllExtensions = suiteExtensions.OfType<IBeforeAllCallback>().ToList();
        foreach (var ext in beforeAllExtensions)
            await ext.BeforeAll(context).ConfigureAwait(true);
    }

    /// <summary>
    /// Runs <see cref="IAfterAllCallback.AfterAll"/> for all suite-level extensions in reverse registration order.
    /// </summary>
    public async Task RunAfterAll(IExtensionContext context)
    {
        foreach (var ext in suiteExtensions.OfType<IAfterAllCallback>().Reverse())
            await ext.AfterAll(context).ConfigureAwait(true);
    }

    /// <summary>
    /// Runs <see cref="IBeforeEachCallback.BeforeEach"/> for suite-level and method-level extensions in registration order.
    /// </summary>
    public async Task RunBeforeEach(IExtensionContext context, MethodInfo testMethod)
    {
        foreach (var ext in GetExtensionsForMethod(suiteExtensions, testMethod).OfType<IBeforeEachCallback>())
            await ext.BeforeEach(context).ConfigureAwait(true);
    }

    /// <summary>
    /// Runs <see cref="IAfterEachCallback.AfterEach"/> for suite-level and method-level extensions in reverse registration order.
    /// </summary>
    public async Task RunAfterEach(IExtensionContext context, MethodInfo testMethod)
    {
        foreach (var ext in GetExtensionsForMethod(suiteExtensions, testMethod).OfType<IAfterEachCallback>().Reverse())
            await ext.AfterEach(context).ConfigureAwait(true);
    }

    /// <summary>
    /// Resolves method arguments by applying <see cref="IParameterResolver"/> extensions first,
    /// then type-matching remaining <paramref name="testCaseArguments"/> to unresolved parameters.
    /// </summary>
    /// <param name="method">The test method whose parameters need to be resolved.</param>
    /// <param name="context">The extension context, which has already had <see cref="RunBeforeEach"/> called.</param>
    /// <param name="testCaseArguments">The raw arguments from <c>[TestCase(...)]</c>.</param>
    /// <returns>An array of resolved argument values matching the method's parameter list.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a parameter cannot be resolved by any extension or by type-matching the remaining
    /// <paramref name="testCaseArguments"/>.
    /// </exception>
    public object?[] ResolveArguments(MethodInfo method, IExtensionContext context, object?[] testCaseArguments)
    {
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
            return [];

        var allExtensions = GetExtensionsForMethod(suiteExtensions, method);
        var resolvers = allExtensions.OfType<IParameterResolver>().ToList();

        // No resolvers — fall back to raw TestCase arguments as-is
        if (resolvers.Count == 0)
            return testCaseArguments;

        var resolvedArgs = new object?[parameters.Length];
        var remainingArgs = testCaseArguments.ToList();

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];

            // Priority 1: first IParameterResolver that supports this parameter
            var resolver = resolvers.FirstOrDefault(r => r.SupportsParameter(param, context));
            if (resolver != null)
            {
                resolvedArgs[i] = resolver.ResolveParameter(param, context);
                continue;
            }

            // Priority 2: type-compatible match from remaining TestCase arguments
            var matchIndex = remainingArgs.FindIndex(a => a != null && param.ParameterType.IsInstanceOfType(a));
            if (matchIndex >= 0)
            {
                resolvedArgs[i] = remainingArgs[matchIndex];
                remainingArgs.RemoveAt(matchIndex);
                continue;
            }

            throw new InvalidOperationException(
                $"No value could be resolved for parameter '{param.Name}' of type '{param.ParameterType.Name}' " +
                $"in test '{method.DeclaringType?.Name}.{method.Name}'. " +
                "Provide a value via [TestCase(...)] or register an IParameterResolver extension.");
        }

        return resolvedArgs;
    }

    /// <summary>
    /// Returns the combined list of suite-level extensions and any method-level extensions from
    /// <see cref="ExtendWithAttribute{T}"/> on <paramref name="method"/>.
    /// Method-level extensions receive a fresh instance per resolution to match JUnit 5 semantics.
    /// </summary>
    private static List<ITestExtension> GetExtensionsForMethod(List<ITestExtension> suite, MethodInfo method)
    {
        var methodExtensions = new List<ITestExtension>();

        foreach (var attr in method.GetCustomAttributes())
        {
            var attrType = attr.GetType();
            if (!attrType.IsGenericType || attrType.GetGenericTypeDefinition() != typeof(ExtendWithAttribute<>))
                continue;
            var extensionType = attrType.GetGenericArguments()[0];
            methodExtensions.Add((ITestExtension)Activator.CreateInstance(extensionType)!);
        }

        if (methodExtensions.Count == 0)
            return suite;

        var combined = new List<ITestExtension>(suite.Count + methodExtensions.Count);
        combined.AddRange(suite);
        combined.AddRange(methodExtensions);
        return combined;
    }

    private static List<ITestExtension> CollectSuiteExtensions(Type type, object instance)
    {
        var result = new List<ITestExtension>();

        // Class-level [ExtendWith<T>] — one instance per suite, collected in declaration order
        foreach (var attr in type.GetCustomAttributes(inherit: true))
        {
            var attrType = attr.GetType();
            if (!attrType.IsGenericType || attrType.GetGenericTypeDefinition() != typeof(ExtendWithAttribute<>))
                continue;
            var extensionType = attrType.GetGenericArguments()[0];
            result.Add((ITestExtension)Activator.CreateInstance(extensionType)!);
        }

        // [RegisterExtension] fields — supports constructor arguments; same instance reused for all tests
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!field.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (field.GetValue(field.IsStatic ? null : instance) is ITestExtension ext)
                result.Add(ext);
        }

        // [RegisterExtension] properties
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
        {
            if (!prop.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (prop.GetValue(prop.GetMethod?.IsStatic == true ? null : instance) is ITestExtension ext)
                result.Add(ext);
        }

        return result;
    }
}
