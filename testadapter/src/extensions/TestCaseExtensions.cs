namespace GdUnit4.TestAdapter.Extensions;

using System.Collections.ObjectModel;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

/// <summary>
///     Extension Methods for TestCase Class.
/// </summary>
internal static class TestCaseExtensions
{
    internal static readonly TestProperty TestCaseNameProperty = TestProperty.Register(
        "TestCase.Name",
        "Name",
        string.Empty,
        string.Empty,
        typeof(string),
        o => !string.IsNullOrWhiteSpace(o as string),
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty ManagedTypeProperty = TestProperty.Register(
        ManagedNameConstants.ManagedTypePropertyId,
        ManagedNameConstants.ManagedTypeLabel,
        string.Empty,
        string.Empty,
        typeof(string),
        o => !string.IsNullOrWhiteSpace(o as string),
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty ManagedMethodProperty = TestProperty.Register(
        ManagedNameConstants.ManagedMethodPropertyId,
        ManagedNameConstants.ManagedMethodLabel,
        string.Empty,
        string.Empty,
        typeof(string),
        o => !string.IsNullOrWhiteSpace(o as string),
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty HierarchyProperty = TestProperty.Register(
        HierarchyConstants.HierarchyPropertyId,
        HierarchyConstants.HierarchyLabel,
        string.Empty,
        string.Empty,
        typeof(string[]),
        null,
        TestPropertyAttributes.Immutable,
        typeof(TestCase));

    internal sealed class TestCaseDescriptor
    {
        internal string? ManagedType { get; init; }
        internal string? ManagedMethod { get; init; }
        internal ReadOnlyCollection<string?>? HierarchyValues { get; init; }
        internal string DisplayName { get; init; } = "";
        internal string FullyQualifiedName { get; init; } = "";
    }
}
