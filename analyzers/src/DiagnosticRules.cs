namespace GdUnit4.Analyzers;

using Microsoft.CodeAnalysis;

internal static class DiagnosticRules
{
    public const string HELP_LINK = "https://github.com/MikeSchulze/gdUnit4Net/tree/master/analyzers/documentation";

    // Rule identifiers - makes it easier to track and maintain rule IDs
    internal static class RuleIds
    {
        // Rule IDs (GdUnit01xx)
        // public const string TestCaseName = "GdUnit0101";

        // Datapoint rules starting at 0200
        public const string DataPointWithMultipleTestCase = "GdUnit0201";

        // Godot runtime detection rules at 500
        public const string GodotEngineDiagnosticId = "GdUnit0501";
    }


    private static class Categories
    {
        public const string AttributeUsage = "Attribute Usage";
    }

    internal static class DataPoint
    {
        public static readonly DiagnosticDescriptor MultipleTestCaseAttributes = new(
            RuleIds.DataPointWithMultipleTestCase,
            "Multiple TestCase attributes not allowed with DataPoint",
            "Method '{0}' cannot have multiple TestCase attributes when DataPoint attribute is present",
            Categories.AttributeUsage,
            DiagnosticSeverity.Error,
            true,
            "Methods decorated with DataPoint attribute can only have one TestCase attribute. Multiple TestCase attributes on a method that uses DataPoint will result in undefined behavior.",
            $"{HELP_LINK}/{RuleIds.DataPointWithMultipleTestCase}.md",
            WellKnownDiagnosticTags.Compiler);

        // Future TestCase rules can be added here
    }

    internal static class GodotEngine
    {
        public static readonly DiagnosticDescriptor GodotNativeCallNotAllowed = new(
            RuleIds.GodotEngineDiagnosticId,
            "Godot Engine Required for Test",
            "Test method '{0}' uses Godot native types or calls but is marked with [TestCase]. Use [GodotTestCase] instead when testing Godot functionality.",
            Categories.AttributeUsage,
            DiagnosticSeverity.Error,
            true,
            "Test methods that interact with Godot types (such as Node, Scene, or other Godot-derived classes) require a running Godot engine. " +
            "These tests must be marked with the [GodotTestCase] attribute to ensure proper test execution in the Godot environment.",
            $"{HELP_LINK}/{RuleIds.GodotEngineDiagnosticId}.md",
            WellKnownDiagnosticTags.Compiler);
    }
}
