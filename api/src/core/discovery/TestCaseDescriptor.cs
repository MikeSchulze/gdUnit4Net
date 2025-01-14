namespace GdUnit4.Core.Discovery;

using System;

using Newtonsoft.Json;

public sealed class TestCaseDescriptor : IEquatable<TestCaseDescriptor>
{
    public required string SimpleName { get; init; }
    public required string FullyQualifiedName { get; init; }
    public required string AssemblyPath { get; init; }
    public required string ManagedType { get; init; }
    public required string ManagedMethod { get; init; }
    public required Guid Id { get; init; }
    public required int LineNumber { get; init; }
    public required string? CodeFilePath { get; init; }
    public required int AttributeIndex { get; init; }
    public required bool RequireRunningGodotEngine { get; init; }

    public bool Equals(TestCaseDescriptor? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return SimpleName == other.SimpleName &&
               FullyQualifiedName == other.FullyQualifiedName &&
               AssemblyPath == other.AssemblyPath &&
               ManagedType == other.ManagedType &&
               ManagedMethod == other.ManagedMethod &&
               Id == other.Id &&
               LineNumber == other.LineNumber &&
               AttributeIndex == other.AttributeIndex &&
               RequireRunningGodotEngine == other.RequireRunningGodotEngine;
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
        hashCode.Add(SimpleName);
        hashCode.Add(FullyQualifiedName);
        hashCode.Add(AssemblyPath);
        hashCode.Add(ManagedType);
        hashCode.Add(ManagedMethod);
        hashCode.Add(Id);
        hashCode.Add(LineNumber);
        hashCode.Add(AttributeIndex);
        hashCode.Add(RequireRunningGodotEngine);
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
