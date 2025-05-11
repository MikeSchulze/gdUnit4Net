// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

using System.Collections.Generic;

using Newtonsoft.Json;

public record TestAssemblyNode : TestNode
{
    /// <summary>
    ///     Gets the file path to the assembly containing this test case.
    /// </summary>
    public required string AssemblyPath { get; init; }

    [JsonIgnore]
    public List<TestSuiteNode> Suites { get; set; } = new();
}
