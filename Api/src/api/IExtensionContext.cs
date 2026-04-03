// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Provides context information to test extensions during their lifecycle callbacks.
/// </summary>
public interface IExtensionContext
{
    /// <summary>
    /// Get the parameter store for this extension context, which allows extensions to store and retrieve arbitrary
    /// values within the context of test execution. This is useful for sharing state between different lifecycle
    /// callbacks of the same extension, or for storing values that should be accessible to parameter resolvers.
    /// </summary>
    /// <returns>The <see cref="IParameterStore"/> containing data for this and any parent contexts.</returns>
    IParameterStore GetStore();
}
