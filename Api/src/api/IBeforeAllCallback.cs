// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Callback interface for test extensions to execute code once before any tests in a suite are run.
/// This callback is executed before any <see cref="BeforeAttribute"/> hooks.
/// </summary>
public interface IBeforeAllCallback : ITestExtension
{
    /// <summary>
    /// This method is called once before any tests in the suite are run, allowing for setup or initialization tasks.
    /// It is invoked *before* any <see cref="BeforeAttribute"/> hooks, ensuring that it runs before all suite-level setup is performed.
    /// </summary>
    /// <param name="context">The extension context providing information about the test suite and execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeforeAll(IExtensionContext context);
}
