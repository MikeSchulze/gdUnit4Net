// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;

[AttributeUsage(AttributeTargets.Delegate)]
public class TestStageAttribute : Attribute
{
    protected TestStageAttribute(string name, int line)
    {
        Name = name;
        Line = line;
    }

    /// <summary>
    ///     Gets or sets describes the intention of the test, will be shown as a tool tip on the inspector node.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the timeout in ms to interrupt the test if the test execution takes longer as the given value.
    /// </summary>
    public long Timeout { get; set; } = -1;

    /// <summary>
    ///     Gets the test name.
    /// </summary>
    internal string Name { get; private set; }

    /// <summary>
    ///     Gets the line of the annotated method.
    /// </summary>
    internal int Line { get; private set; }
}
