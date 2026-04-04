// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Interface for test extensions that support parameter resolution, allowing them to provide values for parameters in test methods.
/// </summary>
public interface IParameterResolver : ITestExtension
{
    /// <summary>
    /// Returns whether this extension supports resolving the given parameter.
    /// If true, <see cref="ResolveParameter"/> will be called to obtain the value for the parameter.
    /// </summary>
    /// <param name="parameterContext">The parameter context to check for resolution eligibility.</param>
    /// <param name="extensionContext">The extension context providing information about the test suite and execution.</param>
    /// <returns>True if the parameter is supported.</returns>
    bool SupportsParameter(IParameterContext parameterContext, IExtensionContext extensionContext);

    /// <summary>
    /// Resolves the value for the given parameter. This method is called only if <see cref="SupportsParameter"/>
    /// returns true for the parameter.
    /// </summary>
    /// <param name="parameterContext">The parameter context to resolve into an instance.</param>
    /// <param name="extensionContext">The extension context providing information about the test suite and execution.</param>
    /// <returns>The value to be passed into the parameter.</returns>
    object? ResolveParameter(IParameterContext parameterContext, IExtensionContext extensionContext);
}
