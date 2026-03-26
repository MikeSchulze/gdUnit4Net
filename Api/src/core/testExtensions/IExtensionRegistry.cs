// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

/// <summary>
/// Interface for class which discovers and manages test extensions registered via <see cref="ExtendWithAttribute{T}"/> and
/// <see cref="RegisterExtensionAttribute"/>, and orchestrates their lifecycle callbacks and parameter resolution.
/// </summary>
internal interface IExtensionRegistry
{
    /// <summary>
    /// Runs <see cref="IBeforeAllCallback.BeforeAll"/> for all suite-level extensions in registration order.
    /// </summary>
    /// <param name="context">The extension context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RunBeforeAll(IExtensionContext context);

    /// <summary>
    /// Runs <see cref="IAfterAllCallback.AfterAll"/> for all suite-level extensions in reverse registration order.
    /// </summary>
    /// <param name="context">The extension context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RunAfterAll(IExtensionContext context);

    /// <summary>
    /// Runs <see cref="IBeforeEachCallback.BeforeEach"/> for suite-level and method-level extensions in registration order.
    /// </summary>
    /// <param name="context">The extension context.</param>
    /// <param name="testMethod">The test method for which to run method-level extensions.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RunBeforeEach(IExtensionContext context, MethodInfo testMethod);

    /// <summary>
    /// Runs <see cref="IAfterEachCallback.AfterEach"/> for suite-level and method-level extensions in reverse registration order.
    /// </summary>
    /// <param name="context">The extension context.</param>
    /// <param name="testMethod">The test method for which to run method-level extensions.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task RunAfterEach(IExtensionContext context, MethodInfo testMethod);

    /// <summary>
    /// Resolves method arguments by applying <see cref="IParameterResolver"/> extensions first,
    /// then type-matching remaining <paramref name="testCaseArguments"/> to unresolved parameters.
    /// </summary>
    /// <param name="method">The test method whose parameters need to be resolved.</param>
    /// <param name="context">The extension context, which has already had <see cref="IExtensionRegistry.RunBeforeEach"/> called.</param>
    /// <param name="testCaseArguments">The raw arguments from <c>[TestCase(...)]</c>.</param>
    /// <returns>An array of resolved argument values matching the method's parameter list.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a parameter cannot be resolved by any extension or by type-matching the remaining
    /// <paramref name="testCaseArguments"/>.
    /// </exception>
    object?[] ResolveArguments(MethodInfo method, IExtensionContext context, object?[] testCaseArguments);
}
