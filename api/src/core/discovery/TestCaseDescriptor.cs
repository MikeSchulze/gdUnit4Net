﻿namespace GdUnit4.Core.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;

using Execution;

using Newtonsoft.Json;

/// <summary>
///     Describes a discovered test case with its location and execution requirements.
/// </summary>
public sealed class TestCaseDescriptor : IEquatable<TestCaseDescriptor>
{
    /// <summary>
    ///     Simple display name of the test case.
    /// </summary>
    public string SimpleName { get; set; } = string.Empty;

    /// <summary>
    ///     Fully qualified name including class path and test parameters.
    /// </summary>
    public string FullyQualifiedName { get; set; } = string.Empty;

    /// <summary>
    ///     Path to the assembly containing this test case.
    /// </summary>
    public required string AssemblyPath { get; init; }

    /// <summary>
    ///     Fully qualified name of the class containing this test.
    /// </summary>
    public required string ManagedType { get; init; }

    /// <summary>
    ///     Name of the test method.
    /// </summary>
    public required string ManagedMethod { get; init; }

    /// <summary>
    ///     Unique identifier for this test case.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    ///     Line number in source file where test is defined.
    /// </summary>
    public required int LineNumber { get; init; }

    /// <summary>
    ///     Path to source code file containing this test.
    /// </summary>
    public required string? CodeFilePath { get; init; }

    /// <summary>
    ///     Index if multiple test cases exist for same method.
    /// </summary>
    public required int AttributeIndex { get; init; }

    /// <summary>
    ///     Whether this test requires the Godot engine to be running.
    /// </summary>
    public required bool RequireRunningGodotEngine { get; init; }

    /// <summary>
    ///     Gets or sets the list of categories associated with this test case.
    ///     Categories are set by the TestCategory attributes.
    /// </summary>
    public List<string> Categories { get; init; } = new();

    /// <summary>
    ///     Gets or sets the collection of traits associated with this test case.
    ///     Each trait is a name-value pair that can be used for filtering and reporting.
    /// </summary>
    public Dictionary<string, List<string>> Traits { get; init; } = new();

    // ReSharper disable once UsageOfDefaultStructEquality
    public bool Equals(TestCaseDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return SimpleName == other.SimpleName
               && FullyQualifiedName == other.FullyQualifiedName
               && AssemblyPath == other.AssemblyPath
               && ManagedType == other.ManagedType
               && ManagedMethod == other.ManagedMethod
               && Id == other.Id
               && LineNumber == other.LineNumber
               && AttributeIndex == other.AttributeIndex
               && RequireRunningGodotEngine == other.RequireRunningGodotEngine
               && Categories.SequenceEqual(other.Categories)
               && Traits.Count == other.Traits.Count
               && Traits.Keys.All(key =>
                   other.Traits.ContainsKey(key) && Traits[key].SequenceEqual(other.Traits[key]));
    }

    public TestCaseDescriptor Build(TestCaseAttribute testCaseAttribute, bool hasMultipleAttributes)
    {
        SimpleName = TestCase.BuildDisplayName(ManagedMethod, testCaseAttribute, hasMultipleAttributes ? AttributeIndex : -1);
        FullyQualifiedName = hasMultipleAttributes
            ? $"{ManagedType}.{ManagedMethod}.{SimpleName}"
            : $"{ManagedType}.{SimpleName}";
        return this;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is TestCaseDescriptor descriptor && Equals(descriptor);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(AssemblyPath);
        hashCode.Add(ManagedType);
        hashCode.Add(ManagedMethod);
        hashCode.Add(Id);
        hashCode.Add(LineNumber);
        hashCode.Add(AttributeIndex);
        hashCode.Add(RequireRunningGodotEngine);
        hashCode.Add(Categories.Count);
        hashCode.Add(Traits.Count);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(TestCaseDescriptor? left, TestCaseDescriptor? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(TestCaseDescriptor? left, TestCaseDescriptor? right) => !(left == right);

    public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
}
