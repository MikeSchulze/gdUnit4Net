// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

internal class ParameterContext(ParameterInfo parameterInfo) : IParameterContext
{
    public ParameterInfo GetParameterInfo() => parameterInfo;
}
