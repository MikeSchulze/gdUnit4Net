// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

/// <summary>
/// Callback interface for test extensions to execute code after each individual test has run.
/// This callback is executed after any <see cref="AfterTestAttribute"/> hooks, allowing for cleanup or finalization tasks that
/// need to run after each test method executes.
/// </summary>
public interface IAfterEachCallback : ITestExtension
{
    /// <summary>
    /// This method is called after each individual test has run, allowing for cleanup or finalization tasks that need to
    /// run after each test method executes.
    /// It is invoked *after* any <see cref="AfterTestAttribute"/> hooks, ensuring that it runs after all test-level cleanup
    /// has been performed for each test.
    /// </summary>
    /// <param name="context">The extension context providing information about the test suite and execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AfterEach(IExtensionContext context);
}
