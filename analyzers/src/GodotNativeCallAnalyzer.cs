namespace GdUnit4.Analyzers;

using System;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using static DiagnosticRules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GodotNativeCallAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GodotEngine.GodotNativeCallNotAllowed);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethodSymbol, SymbolKind.Method);
    }

    private static void AnalyzeMethodSymbol(SymbolAnalysisContext context)
    {
        try
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            // Check if this is a test method
            if (!HasTestAttribute(context, methodSymbol))
                return;

            // Check if method body contains Godot type usage
            if (ContainsGodotTypes(methodSymbol, context.Compilation))
            {
                var diagnostic = Diagnostic.Create(
                    GodotEngine.GodotNativeCallNotAllowed,
                    methodSymbol.Locations[0],
                    methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static bool HasTestAttribute(SymbolAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var testCaseAttr = context.Compilation.GetTypeByMetadataName("GdUnit4.TestCaseAttribute");
        if (testCaseAttr == null)
            return false;

        // Check for TestCase attribute
        return methodSymbol.GetAttributes()
            .Any(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, testCaseAttr));
    }

    private static bool ContainsGodotTypes(IMethodSymbol method, Compilation compilation)
    {
        // Get syntax reference for method body
        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return false;


        // Get the semantic model for the syntax tree
        var tree = syntaxRef.SyntaxTree;
        var semanticModel = compilation.GetSemanticModel(tree);

        // Check parameter types
        foreach (var parameter in method.Parameters)
            if (InheritsFromGodotType(parameter.Type))
                return true;


        // Check return type
        if (InheritsFromGodotType(method.ReturnType))
            return true;

        // Get the method body syntax
        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax?.Body == null)
            return false;

        // Analyze method body using semantic model
        var operation = semanticModel.GetOperation(methodSyntax);
        if (operation == null)
            return false;

        foreach (var childOperation in operation.Descendants())
            switch (childOperation)
            {
                case ILocalReferenceOperation localRef:
                    if (InheritsFromGodotType(localRef.Type))
                        return true;
                    break;

                case IObjectCreationOperation creation:
                    if (InheritsFromGodotType(creation.Type))
                        return true;
                    break;

                case IInvocationOperation invocation:
                    if (InheritsFromGodotType(invocation.Type) ||
                        (invocation.Instance != null && InheritsFromGodotType(invocation.Instance.Type)))
                        return true;
                    break;

                case IPropertyReferenceOperation propRef:
                    if (InheritsFromGodotType(propRef.Type))
                        return true;
                    break;

                case IFieldReferenceOperation fieldRef:
                    if (InheritsFromGodotType(fieldRef.Type))
                        return true;
                    break;
            }

        return false;
    }

    private static bool InheritsFromGodotType(ITypeSymbol type)
    {
        if (type == null)
            return false;

        var current = type;
        while (current != null)
        {
            // Check if the current type is from Godot namespace
            if (current.ContainingNamespace?.ToDisplayString().StartsWith("Godot") == true)
                return true;
            current = current.BaseType;
        }

        return false;
    }
}
