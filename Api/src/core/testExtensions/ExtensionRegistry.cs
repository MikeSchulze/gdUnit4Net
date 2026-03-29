// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Collections.ObjectModel;
using System.Reflection;

using Api;

internal sealed class ExtensionRegistry
{
    private readonly List<ITestExtension> extensions = [];

    public ReadOnlyCollection<ITestExtension> FindTestExtensions(Type type)
    {
        extensions.Clear();
        extensions.AddRange(CollectExtendWithExtensions(type));
        extensions.AddRange(CollectRegisterExtensions(type));
        return extensions.AsReadOnly();
    }

    public async Task RunBeforeAll(IExtensionContext extensionContext)
    {
        foreach (var extension in extensions.OfType<IBeforeAllCallback>())
            await extension.BeforeAll(extensionContext).ConfigureAwait(true);
    }

    public async Task RunBeforeEach(IExtensionContext extensionContext)
    {
        foreach (var extension in extensions.OfType<IBeforeEachCallback>())
            await extension.BeforeEach(extensionContext).ConfigureAwait(true);
    }

    public async Task RunAfterEach(IExtensionContext extensionContext)
    {
        foreach (var extension in extensions.OfType<IAfterEachCallback>().Reverse())
            await extension.AfterEach(extensionContext).ConfigureAwait(true);
    }

    public async Task RunAfterAll(IExtensionContext extensionContext)
    {
        foreach (var extension in extensions.OfType<IAfterAllCallback>().Reverse())
            await extension.AfterAll(extensionContext).ConfigureAwait(true);
    }

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
    internal static List<ITestExtension> CollectExtendWithExtensions(Type type) =>
    [
        .. GetTypeHierarchy(type)
            .SelectMany<Type, ExtendWithBaseAttribute>(t => t.GetCustomAttributes<ExtendWithBaseAttribute>(false))
            .Select(attr => attr.CreateExtension())
    ];

    internal static List<ITestExtension> CollectRegisterExtensions(Type type) =>
    [
        .. type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(field => field.IsDefined(typeof(RegisterExtensionAttribute)))
            .Select(field => field.GetValue(null))
            .OfType<ITestExtension>()
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
