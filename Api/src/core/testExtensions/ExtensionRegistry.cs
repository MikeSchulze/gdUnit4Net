// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Collections.ObjectModel;
using System.Reflection;

using Api;

internal class ExtensionRegistry
{
    private readonly List<ITestExtension> extensions = [];

    public ReadOnlyCollection<ITestExtension> FindTestExtensions(Type type)
    {
        extensions.Clear();
        extensions.AddRange(CollectExtendWithExtensions(type));
        extensions.AddRange(CollectRegisterExtensions(type));
        return extensions.AsReadOnly();
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
        .. Enumerable
            .SelectMany<Type, ExtendWithBaseAttribute>(GetTypeHierarchy(type), t => t.GetCustomAttributes<ExtendWithBaseAttribute>(inherit: false))
            .Select(attr => attr.CreateExtension())
    ];

    internal static List<ITestExtension> CollectRegisterExtensions(Type type)
    {
        List<ITestExtension> extensions = [];

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (!field.IsDefined(typeof(RegisterExtensionAttribute)))
                continue;

            if (field.GetValue(null) is ITestExtension ext)
                extensions.Add(ext);
        }

        return extensions;
    }

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
