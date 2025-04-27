namespace GdUnit4.Analyzers;

using Microsoft.CodeAnalysis;

internal static class DiagnosticRules
{
    private const string HELP_LINK = "https://github.com/MikeSchulze/gdUnit4Net/tree/master/analyzers/documentation";

    // Rule identifiers - makes it easier to track and maintain rule IDs
    internal static class RuleIds
    {
        // Rule IDs (GdUnit01xx)
        // public const string TestCaseName = "GdUnit0101";

        // Datapoint rules starting at 0200
        public const string DataPointWithMultipleTestCase = "GdUnit0201";

        // Godot runtime detection rules at 500
        public const string RequiresGodotRuntimeOnClassId = "GdUnit0500";
        public const string RequiresGodotRuntimeOnMethodId = "GdUnit0501";
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
        public static readonly DiagnosticDescriptor RequiresGodotRuntimeOnMethod = new(
            RuleIds.RequiresGodotRuntimeOnMethodId,
            "Godot Runtime Required for Test Method",
            "Test method '{0}' uses Godot functionality but is not annotated with `[RequireGodotRuntime]`",
            Categories.AttributeUsage,
            DiagnosticSeverity.Error,
            true,
            """
            Test methods that use Godot functionality (such as Node, Scene, or other Godot types)
            must be annotated with `[RequireGodotRuntime]`. This ensures proper test execution in
            the Godot engine environment.

            Add `[RequireGodotRuntime]` to either test method or class level.
            """,
            $"{HELP_LINK}/{RuleIds.RequiresGodotRuntimeOnMethodId}.md",
            WellKnownDiagnosticTags.Compiler);

        public static readonly DiagnosticDescriptor RequiresGodotRuntimeOnClass = new(
            RuleIds.RequiresGodotRuntimeOnClassId,
            "Godot Runtime Required for Test Class",
            "Test class '{0}' uses Godot native types or calls in test hooks but is not annotated with `[RequireGodotRuntime]`",
            Categories.AttributeUsage,
            DiagnosticSeverity.Error,
            true,
            """
            Test classes with hooks (`[Before]`, `[After]`, `[BeforeTest]`, `[AfterTest]`) that use Godot functionality
            must be annotated with `[RequireGodotRuntime]`. This ensures proper test execution in the
            Godot engine environment.

            Add `[RequireGodotRuntime]` to the test class level.
            """,
            $"{HELP_LINK}/{RuleIds.RequiresGodotRuntimeOnClassId}.md",
            WellKnownDiagnosticTags.Compiler);
    }
}
