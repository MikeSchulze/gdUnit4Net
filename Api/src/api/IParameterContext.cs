// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Api;

using System.Reflection;

/// <summary>
/// Provides context information about a parameter being resolved, including reflective <see cref="ParameterInfo"/>.
/// </summary>
public interface IParameterContext
{
    /// <summary>
    /// Get the parameter information for the parameter being resolved, including its type, name, and any attributes applied to it.
    /// </summary>
    /// <returns>A <see cref="ParameterInfo"/> instance representing the current parameter.</returns>
    ParameterInfo GetParameterInfo();
}
