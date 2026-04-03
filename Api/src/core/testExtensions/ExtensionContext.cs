// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using Api;

internal class ExtensionContext : IExtensionContext
{
    private readonly IParameterStore parameterStore;

    public ExtensionContext(IExtensionContext parentContext) => parameterStore = new ParameterStore(parentContext.GetStore());

    public ExtensionContext() => parameterStore = new ParameterStore();

    public IParameterStore GetStore() => parameterStore;
}
