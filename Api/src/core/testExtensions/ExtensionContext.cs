// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Collections.Generic;

using Api;

internal class ExtensionContext : IExtensionContext
{
    private readonly IParameterStore parameterStore;

    public ExtensionContext() => parameterStore = new ParameterStore();

    public ExtensionContext(IExtensionContext parentContext)
        : this(parentContext, [])
    {
    }

    public ExtensionContext(IExtensionContext parentContext, object?[] testCaseArguments)
    {
        parameterStore = new ParameterStore(parentContext.GetStore());
        TestCaseArguments = testCaseArguments;
    }

    public IReadOnlyList<object?> TestCaseArguments { get; } = [];

    public IParameterStore GetStore() => parameterStore;
}
