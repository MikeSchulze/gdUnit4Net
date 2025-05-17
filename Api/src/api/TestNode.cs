// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

using System;

public record TestNode
{
    /// <summary>
    ///     Gets the unique identifier for this test case.
    /// </summary>
    public required Guid Id { get; init; }

    public required Guid ParentId { get; set; }
}
