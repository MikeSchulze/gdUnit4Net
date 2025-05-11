// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Attributes;

using System;

/// <summary>
///     Specifies a category for a test method or class.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class TestCategoryAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TestCategoryAttribute" /> class.
    /// </summary>
    /// <param name="category">The name of the category.</param>
    public TestCategoryAttribute(string category) => Category = category;

    /// <summary>
    ///     Gets the name of the category.
    /// </summary>
    public string Category { get; }
}
