// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using Api;

internal class ParameterStore(IParameterStore? parentStore = null) : IParameterStore
{
    private Dictionary<string, object?> DataStore { get; } = [];

    public void Add<T>(string key, T value)
        where T : notnull => DataStore[key] = value;

    public T? Value<T>(string key)
    {
        if (DataStore.TryGetValue(key, out var value) && value is T typedValue)
            return typedValue;

        return parentStore != null ? parentStore.Value<T>(key) : default;
    }

    public T? Remove<T>(string key)
    {
        if (!DataStore.TryGetValue(key, out var value))
            return default;

        if (value is not T typedValue)
            return default;

        return DataStore.Remove(key) ? typedValue : default;
    }

    public int Count() => DataStore.Count + (parentStore?.Count() ?? 0);
}
