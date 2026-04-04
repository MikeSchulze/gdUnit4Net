// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Base class for <see cref="IParameterResolver"/> implementations that resolve parameters from the extension context's store.
/// </summary>
public abstract class ContextStoreParameterResolver : IParameterResolver
{
    /// <summary>
    /// Returns true if the specified parameter context can be resolved using the specified context's store.
    /// </summary>
    /// <param name="parameterContext">The parameter context to resolve.</param>
    /// <param name="extensionContext">The extension context.</param>
    /// <returns>true if the parameter can be resolved using the context store.</returns>
    /// <exception cref="ArgumentNullException">If either the parameter or extension context are null.</exception>
    public bool SupportsParameter(IParameterContext parameterContext, IExtensionContext extensionContext)
    {
        ArgumentNullException.ThrowIfNull(parameterContext);
        ArgumentNullException.ThrowIfNull(extensionContext);
        return extensionContext.GetStore().Has(
            parameterContext.GetParameterInfo().Name ?? string.Empty,
            parameterContext.GetParameterInfo().ParameterType);
    }

    /// <summary>
    /// Returns the value for the specified parameter context from the specified context's store.
    ///
    /// This method should only be called if <see cref="SupportsParameter"/> returns `true.
    /// </summary>
    /// <param name="parameterContext">The parameter context to resolve.</param>
    /// <param name="extensionContext">The extension context.</param>
    /// <returns>The resolved parameter.</returns>
    /// <exception cref="ArgumentNullException">If either the parameter or extension context are null.</exception>
    public object ResolveParameter(IParameterContext parameterContext, IExtensionContext extensionContext)
    {
        ArgumentNullException.ThrowIfNull(parameterContext);
        ArgumentNullException.ThrowIfNull(extensionContext);
        return extensionContext.GetStore().Value(
            parameterContext.GetParameterInfo().Name ?? string.Empty,
            parameterContext.GetParameterInfo().ParameterType);
    }
}
