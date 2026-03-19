// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Callback interface for test extensions to execute code once after all tests in a suite have run.
/// This callback is executed after any <see cref="AfterAttribute"/> hooks.
/// </summary>
public interface IAfterAllCallback : ITestExtension
{
    /// <summary>
    /// This method is called once after all tests in the suite have run, allowing for cleanup or finalization tasks.
    /// It is invoked *after* any <see cref="AfterAttribute"/> hooks, ensuring that it runs after all suite-level
    /// cleanup has been performed.
    /// </summary>
    /// <param name="context">The extension context providing information about the test suite and execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AfterAll(IExtensionContext context);
}
