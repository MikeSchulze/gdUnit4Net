// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

internal class ExtensionContext : IExtensionContext
{
    public ExtensionContext(
        Type testSuiteType,
        object testSuiteInstance,
        MethodInfo? testMethod,
        string? testCaseName,
        object?[] testCaseArguments,
        Dictionary<Tuple<string?, string?, string>, object?>? initialData = null)
    {
        DataStore = new Dictionary<Tuple<string?, string?, string>, object?>(initialData ?? []);
        TestSuiteType = testSuiteType;
        TestSuiteInstance = testSuiteInstance;
        TestMethod = testMethod;
        TestCaseName = testCaseName;
        TestCaseArguments = testCaseArguments;
    }

    public Type TestSuiteType { get; }

    public object TestSuiteInstance { get; }

    public MethodInfo? TestMethod { get; }

    public string? TestCaseName { get; }

    public object?[] TestCaseArguments { get; }

    internal Dictionary<Tuple<string?, string?, string>, object?> DataStore { get; }

    public void Store<T>(string key, T value)
        where T : notnull
    {
        var composedKey = Tuple.Create(TestSuiteType.FullName, TestCaseName, key);
        DataStore[composedKey] = value;
    }

    public T? Retrieve<T>(string key)
    {
        var testLevelKey = Tuple.Create(TestSuiteType.FullName, TestCaseName, key);
        if (DataStore.TryGetValue(testLevelKey, out var value) && value is T typedValue)
            return typedValue;
        var suiteLevelKey = Tuple.Create(TestSuiteType.FullName, (string?)null, key);
        if (DataStore.TryGetValue(suiteLevelKey, out value) && value is T typedSuiteValue)
            return typedSuiteValue;
        return default;
    }

    public T? Delete<T>(string key)
    {
        var composedKey = Tuple.Create(TestSuiteType.FullName, TestCaseName, key);
        if (!DataStore.TryGetValue(composedKey, out var value))
            return default;

        if (value is not T typedValue)
            return default;

        return DataStore.Remove(composedKey) ? typedValue : default;
    }
}
