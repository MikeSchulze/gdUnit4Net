namespace GdUnit4.Analyzers;

using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using static DiagnosticRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataPointAttributeAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DataPoint.MultipleTestCaseAttributes);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Get full attribute names
        var dataPointAttr = context.Compilation.GetTypeByMetadataName("GdUnit4.DataPointAttribute");
        var testCaseAttr = context.Compilation.GetTypeByMetadataName("GdUnit4.TestCaseAttribute");

        if (dataPointAttr == null || testCaseAttr == null)
            return;

        // Check for DataPoint attribute
        var hasDataPoint = methodSymbol.GetAttributes()
            .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, dataPointAttr));

        if (!hasDataPoint) return;

        // Get all TestCase attributes with their ApplicationSyntaxReference
        var testCaseAttributes = methodSymbol.GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseAttr))
            .Skip(1); // Skip the first TestCase attribute, one test case attribute is required

        // Report on all TestCase attributes after the first one
        foreach (var testCaseAttribute in testCaseAttributes)
            if (testCaseAttribute.ApplicationSyntaxReference?.GetSyntax() is { } syntaxNode)
            {
                var diagnostic = Diagnostic.Create(
                    DataPoint.MultipleTestCaseAttributes,
                    syntaxNode.GetLocation(),
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
    }
}
