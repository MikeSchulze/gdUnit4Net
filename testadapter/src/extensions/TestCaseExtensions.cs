namespace GdUnit4.TestAdapter.Extensions;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

/// <summary>
///     Extension Methods for TestCase Class.
/// </summary>
internal static class TestCaseExtensions
{
    public const string ManagedMethodAttributeIndexLabel = "ManagedAttributeIndexLabel";

    public const string ManagedMethodAttributeIndexId = $"TestCase.{ManagedMethodAttributeIndexLabel}";


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
        "holds the test suite class name",
        typeof(string),
        o => !string.IsNullOrWhiteSpace(o as string),
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty ManagedMethodProperty = TestProperty.Register(
        ManagedNameConstants.ManagedMethodPropertyId,
        ManagedNameConstants.ManagedMethodLabel,
        string.Empty,
        "holds the test method name",
        typeof(string),
        o => !string.IsNullOrWhiteSpace(o as string),
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty ManagedMethodAttributeIndexProperty = TestProperty.Register(
        ManagedMethodAttributeIndexId,
        ManagedMethodAttributeIndexLabel,
        string.Empty,
        "Holds the test method attribute index",
        typeof(int),
        value => value is int and >= 0,
        TestPropertyAttributes.Hidden,
        typeof(TestCase));

    internal static readonly TestProperty RequireRunningGodotEngineProperty = TestProperty.Register(
        "TestCase.RequireRunningGodotEngineLabel",
        "RequireRunningGodotEngineLabel",
        string.Empty,
        "Indicates that the test method must execute inside Godot engine context.",
        typeof(bool),
        value => value is bool,
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
}
