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
    void Add<T>(string key, T value)
        where T : notnull;

    /// <summary>
    /// Retrieve a previously stored value, falling back to parent store to parent store if not found.
    ///
    /// If the key is not found in this store or any parent store, or if the value found is not of type T,
    /// this method throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    /// <typeparam name="T">The expected type of object to retrieve.</typeparam>
    /// <param name="key">The key used when the value was stored.</param>
    /// <returns>The value stored at the specified key, or null if there is no value stored at the key.</returns>
    /// <exception cref="InvalidOperationException">If there is no value of type T stored in this or a parent store.</exception>
    T Value<T>(string key);

    /// <summary>
    /// Removes the value stored at the specified key and return it.
    /// If there is no value stored at the key, returns null.
    ///
    /// Like <see cref="Value"/>, this method will remove entries from the parent store if it is not found in the child store.
    /// </summary>
    /// <param name="key">The key specifying the value to be deleted.</param>
    /// <typeparam name="T">The expected type of value stored at the specified key.</typeparam>
    /// <returns>The deleted value, if it exists; null otherwise.</returns>
    T? Remove<T>(string key);

    /// <summary>
    /// Gets the number of entries in the internal data store for this context.
    /// </summary>
    /// <returns>The number of entries in the internal data store.</returns>
    int Count();
}
