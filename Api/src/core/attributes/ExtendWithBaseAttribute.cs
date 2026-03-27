// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
// Need to be placed in the root namespace to be accessible by the test runner.
namespace GdUnit4;

using Api;

/// <summary>
/// Non-generic base for <see cref="ExtendWithAttribute{T}"/>, used to discover and instantiate
/// test extensions without knowing the concrete type parameter at the call site.
/// </summary>
public abstract class ExtendWithBaseAttribute : Attribute
{
    /// <summary>Creates and returns a new instance of the registered <see cref="ITestExtension"/>.</summary>
    /// <returns>A new <see cref="ITestExtension"/> instance.</returns>
    public abstract ITestExtension CreateExtension();
}
