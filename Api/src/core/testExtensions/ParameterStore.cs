// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using Api;

internal class ParameterStore(IParameterStore? parentStore = null) : IParameterStore
{
    private Dictionary<string, object?> DataStore { get; } = [];

    public void Add<T>(string key, T value)
        where T : notnull => DataStore[key] = value;

    public T Value<T>(string key)
    {
        if (!DataStore.TryGetValue(key, out var value))
        {
            return parentStore is not null
                ? parentStore.Value<T>(key)
                : throw new InvalidOperationException($"No value stored at {key}");
        }

        return value is T typedValue
            ? typedValue
            : throw new InvalidOperationException($"Value stored at {key} is not of type {typeof(T)}");
    }

    public T? Remove<T>(string key)
    {
        if (!DataStore.TryGetValue(key, out var value))
            return parentStore != null ? parentStore.Remove<T>(key) : default;

        if (value is not T typedValue)
            return default;

        return DataStore.Remove(key) ? typedValue : default;
    }

    public int Count() => DataStore.Count + (parentStore?.Count() ?? 0);
}
