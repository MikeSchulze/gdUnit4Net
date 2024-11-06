namespace GdUnit4.Analyzers;

using Microsoft.CodeAnalysis;

public static class DiagnosticRules
{
    private const string HELP_LINK = "https://github.com/MikeSchulze/gdUnit4Net/tree/master/analyzers/documentation";

    // Rule identifiers - makes it easier to track and maintain rule IDs
    private static class RuleIds
    {
        // Rule IDs (GdUnit01xx)
        // public const string TestCaseName = "GdUnit0101";

        // Datapoint rules starting at 0200
        public const string DataPointWithMultipleTestCase = "GdUnit0201";
    }


    private static class Categories
    {
        public const string AttributeUsage = "Attribute Usage";
    }

    public static class DataPoint
    {
        internal static readonly DiagnosticDescriptor MultipleTestCaseAttributes = new(
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
}
