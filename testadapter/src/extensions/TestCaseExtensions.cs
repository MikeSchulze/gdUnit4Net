namespace GdUnit4.TestAdapter.Extensions;

using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;


/// <summary>
/// Extension Methods for TestCase Class.
/// </summary>
internal static class TestCaseExtensions
{
    internal static readonly TestProperty TestCaseNameProperty = TestProperty.Register(
           id: "TestCase.Name",
           label: "Name",
           category: string.Empty,
           description: string.Empty,
           valueType: typeof(string),
           validateValueCallback: o => !string.IsNullOrWhiteSpace(o as string),
           attributes: TestPropertyAttributes.Hidden,
           owner: typeof(TestCase));

    internal static readonly TestProperty ManagedTypeProperty = TestProperty.Register(
           id: ManagedNameConstants.ManagedTypePropertyId,
           label: ManagedNameConstants.ManagedTypeLabel,
           category: string.Empty,
           description: string.Empty,
           valueType: typeof(string),
           validateValueCallback: o => !string.IsNullOrWhiteSpace(o as string),
           attributes: TestPropertyAttributes.Hidden,
           owner: typeof(TestCase));

    internal static readonly TestProperty ManagedMethodProperty = TestProperty.Register(
        id: ManagedNameConstants.ManagedMethodPropertyId,
        label: ManagedNameConstants.ManagedMethodLabel,
        category: string.Empty,
        description: string.Empty,
        valueType: typeof(string),
        validateValueCallback: o => !string.IsNullOrWhiteSpace(o as string),
        attributes: TestPropertyAttributes.Hidden,
        owner: typeof(TestCase));

    internal static readonly TestProperty HierarchyProperty = TestProperty.Register(
        id: HierarchyConstants.HierarchyPropertyId,
        label: HierarchyConstants.HierarchyLabel,
        category: string.Empty,
        description: string.Empty,
        valueType: typeof(string[]),
        validateValueCallback: null,
        attributes: TestPropertyAttributes.Immutable,
        owner: typeof(TestCase));

    internal sealed class TestCaseDescriptor
    {
        internal string? ManagedType { get; set; }
        internal string? ManagedMethod { get; set; }
        internal ReadOnlyCollection<string?>? HierarchyValues { get; set; }
        internal string DisplayName { get; set; } = "";
        internal string FullyQualifiedName { get; set; } = "";
        internal List<Trait> Traits { get; set; } = new List<Trait>();
    }
}
