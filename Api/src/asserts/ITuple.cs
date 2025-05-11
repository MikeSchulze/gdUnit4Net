// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System;
using System.Collections.Generic;

/// <summary>
///     A tuple implementation to hold two or many values.
/// </summary>
public interface ITuple : IEquatable<object?>
{
    public IEnumerable<object?> Values { get; set; }
}
