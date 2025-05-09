// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Analyzers;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using static DiagnosticRules;

/// <summary>
///     Analyzer that checks for improper usage of DataPoint attributes.
///     Specifically, it ensures methods with DataPointAttribute don't have multiple TestCaseAttributes.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class DataPointAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     Gets the set of diagnostic descriptors supported by this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(DataPoint.MultipleTestCaseAttributes);

    /// <summary>
    ///     Initializes the analyzer with the analysis context.
    /// </summary>
    /// <param name="context">The analysis context to initialize.</param>
    public override void Initialize(AnalysisContext context)
    {
        Debug.Assert(context != null, nameof(context) + " != null");
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    /// <summary>
    ///     Analyzes a method symbol to verify proper usage of DataPoint and TestCase attributes.
    /// </summary>
    /// <param name="context">The symbol analysis context containing the method to analyze.</param>
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

        if (!hasDataPoint)
            return;

        // Get all TestCase attributes with their ApplicationSyntaxReference
        var testCaseAttributes = methodSymbol.GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseAttr))
            .Skip(1); // Skip the first TestCase attribute, one test case attribute is required

        // Report on all TestCase attributes after the first one
        foreach (var testCaseAttribute in testCaseAttributes)
        {
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
}
