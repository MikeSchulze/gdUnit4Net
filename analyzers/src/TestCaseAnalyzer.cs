namespace GdUnit4.Analyzers;

using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class TestCaseAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "GDU0001";
    private const string Category = "GdUnit4";
    private const string HelpLinkUri = "https://github.com/MikeSchulze/gdUnit4Net/wiki/Analyzers";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Invalid TestCase attribute usage",
        "Method '{0}' cannot have multiple TestCase attributes when DataPoint attribute is present",
        Category,
        DiagnosticSeverity.Error,
        true,
        "Methods decorated with DataPoint attribute can only have one TestCase attribute.",
        HelpLinkUri,
        WellKnownDiagnosticTags.Compiler);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

        if (hasDataPoint)
        {
            // Get all TestCase attributes with their ApplicationSyntaxReference
            var testCaseAttributes = methodSymbol.GetAttributes()
                .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseAttr))
                .ToList();
            // Report on all TestCase attributes after the first one
            for (var i = 1; i < testCaseAttributes.Count; i++)
            {
                var attributeLocation = testCaseAttributes[i].ApplicationSyntaxReference?.GetSyntax().GetLocation();
                if (attributeLocation != null)
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        attributeLocation,
                        methodSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
