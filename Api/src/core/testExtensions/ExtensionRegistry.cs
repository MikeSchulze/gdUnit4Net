// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

internal class ExtensionRegistry
{
    private readonly List<ITestExtension> extensions = [];

    public List<ITestExtension> FindTestExtensions(Type type)
    {
        if (extensions.Count > 0)
            return extensions;
        extensions.AddRange(CollectTestExtensions(type));
        extensions.AddRange(CollectFieldLevelRegisterExtensionExtensions(type));
        extensions.AddRange(CollectPropertyLevelRegisterExtensionExtensions(type));
        return extensions;
    }

    public async Task RunBeforeAll(IExtensionContext context)
    {
        foreach (var ext in FindTestExtensions(context.GetTestSuiteType()).OfType<IBeforeAllCallback>())
            await ext.BeforeAll(context).ConfigureAwait(true);
    }

    public async Task RunAfterAll(IExtensionContext context)
    {
        foreach (var ext in FindTestExtensions(context.GetTestSuiteType()).OfType<IAfterAllCallback>().Reverse())
            await ext.AfterAll(context).ConfigureAwait(true);
    }

    public async Task RunBeforeEach(IExtensionContext context)
    {
        foreach (var ext in FindTestExtensions(context.GetTestSuiteType()).OfType<IBeforeEachCallback>())
            await ext.BeforeEach(context).ConfigureAwait(true);
    }

    public async Task RunAfterEach(IExtensionContext context)
    {
        foreach (var ext in FindTestExtensions(context.GetTestSuiteType()).OfType<IAfterEachCallback>().Reverse())
            await ext.AfterEach(context).ConfigureAwait(true);
    }

    private static List<ITestExtension> CollectPropertyLevelRegisterExtensionExtensions(Type type)
    {
        // [RegisterExtension] properties
        var propertyLevelRegisterExtensionExtensions = new List<ITestExtension>();
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!prop.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (prop.GetValue(null) is ITestExtension ext)
                propertyLevelRegisterExtensionExtensions.Add(ext);
        }

        return propertyLevelRegisterExtensionExtensions;
    }

    private static List<ITestExtension> CollectFieldLevelRegisterExtensionExtensions(Type type)
    {
        // [RegisterExtension] fields — supports constructor arguments; same instance reused for all tests
        var fieldLevelRegisterExtensionExtensions = new List<ITestExtension>();
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!field.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;
            if (field.GetValue(null) is ITestExtension ext)
                fieldLevelRegisterExtensionExtensions.Add(ext);
        }

        return fieldLevelRegisterExtensionExtensions;
    }

    // Collects [ExtendWith<T>] extensions base-first, preserving declaration order within each level.
    //
    // Example:
    //   [ExtendWith<XXX>]
    //   [ExtendWith<YYY>]
    //   class BaseSuite { }
    //
    //   [ExtendWith<Foo>]
    //   [ExtendWith<Bar>]
    //   class TestSuiteA : BaseSuite { }
    //
    // Collected order: XXX → YYY → Foo → Bar
    private static List<ITestExtension> CollectTestExtensions(Type type) =>
        [
            .. GetTypeHierarchy(type)
                .SelectMany(t => t.GetCustomAttributes<ExtendWithBaseAttribute>(inherit: false))
                .Select(attr => attr.CreateExtension())
        ];

    // Walks the inheritance chain from the root base type down to the given type
    private static Stack<Type> GetTypeHierarchy(Type type)
    {
        var hierarchy = new Stack<Type>();
        var current = type;
        while (current != null && current != typeof(object))
        {
            hierarchy.Push(current);
            current = current.BaseType;
        }

        return hierarchy;
    }
}
