// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Interface for storing and retrieving arbitrary values within the context of test extensions.
/// </summary>
public interface IParameterStore
{
    /// <summary>
    /// Store a value scoped to this extension context (keyed by string).
    /// </summary>
    ///
    /// <typeparam name="T">The type of object to store.</typeparam>
    /// <param name="key">The key to use when retrieving the value.</param>
    /// <param name="value">The value to store.</param>
    void Put<T>(string key, T value)
        where T : notnull;

    /// <summary>
    /// Retrieve a previously stored value, falls back to suite context if not found.
    /// </summary>
    /// <typeparam name="T">The expected type of object to retrieve.</typeparam>
    /// <param name="key">The key used when the value was stored.</param>
    /// <returns>The value stored at the specified key, or null if there is no value stored at the key.</returns>
    T? Retrieve<T>(string key);

    /// <summary>
    /// Delete the value stored at the specified key and return it.
    /// If there is no value stored at the key, returns null.
    /// Does **not** cascade to suite context like <see cref="Retrieve{T}"/>. This means that if there is not a value stored
    /// at the specified key in a test case context, this method will return null even if there is a value stored at the
    /// key in the suite context. This ensures that test-level callbacks cannot accidentally delete suite-level values.
    /// </summary>
    /// <param name="key">The key specifying the value to be deleted.</param>
    /// <typeparam name="T">The expected type of value stored at the specified key.</typeparam>
    /// <returns>The deleted value, if it exists; null otherwise.</returns>
    T? Delete<T>(string key);

    /// <summary>
    /// Gets the number of entries in the internal data store for this context.
    /// </summary>
    /// <returns>The number of entries in the internal data store.</returns>
    int Count();
}
