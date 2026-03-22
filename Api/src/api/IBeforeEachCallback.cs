// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Callback interface for test extensions to execute code before each individual test is run.
/// This callback is executed before any <see cref="BeforeTestAttribute"/> hooks, allowing for setup or
/// initialization tasks that need to run before each test method executes.
/// </summary>
public interface IBeforeEachCallback : ITestExtension
{
    /// <summary>
    /// This method is called before each individual test is run, allowing for setup or initialization tasks that need to
    /// run before each test method executes.
    /// It is invoked *before* any <see cref="BeforeTestAttribute"/> hooks, ensuring that it runs before all test-level setup
    /// is performed for each test.
    /// </summary>
    /// <param name="context">The extension context providing information about the test suite and execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeforeEach(IExtensionContext context);
}
