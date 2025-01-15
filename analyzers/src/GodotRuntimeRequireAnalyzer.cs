#nullable enable
namespace GdUnit4.Analyzers;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

using static DiagnosticRules;

/// <summary>
///     Analyzer that enforces proper Godot runtime requirements for test classes and methods.
/// </summary>
/// <remarks>
///     This analyzer checks for two scenarios:
///     <list type="bullet">
///         <item>Test methods using Godot types must use [GodotTestCase] instead of [TestCase]</item>
///         <item>Test classes with Godot dependencies in hooks must use [RequireGodotRuntime]</item>
///     </list>
/// </remarks>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GodotRuntimeRequireAnalyzer : DiagnosticAnalyzer
{
    private static readonly string RequireGodotRuntimeAttribute = "GdUnit4.RequireGodotRuntimeAttribute";
    private static readonly string TestCaseAttribute = "GdUnit4.TestCaseAttribute";
    private static readonly string[] TestHookAttributes = { "GdUnit4.BeforeAttribute", "GdUnit4.AfterAttribute", "GdUnit4.BeforeTestAttribute", "GdUnit4.AfterTestAttribute" };

    private static readonly ConcurrentDictionary<string, bool> GodotTypeCache = new();

    private static readonly HashSet<string> GodotDependentTypes = new()
    {
        "GdUnit4.ISceneRunner"
        // Add any other GdUnit4 types that require Godot runtime
    };


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            GodotEngine.RequiresGodotRuntime,
            GodotEngine.GodotNativeCallNotAllowed
        );

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeTestMethod, OperationKind.MethodBody);
        context.RegisterOperationAction(AnalyzeHookMethod, OperationKind.MethodBody);
    }

    /// <summary>
    ///     Analyzes test methods for Godot dependencies and verifies they use [GodotTestCase].
    /// </summary>
    private static void AnalyzeTestMethod(OperationAnalysisContext context)
    {
        try
        {
            if (context.Operation is not IMethodBodyOperation methodBody)
                return;
            var methodSymbol = methodBody.SemanticModel?.GetDeclaredSymbol(methodBody.Syntax) as IMethodSymbol;
            if (methodSymbol == null)
                return;

            // Skip analysis if class has RequireGodotRuntime attribute
            if (HasGodotRuntimeAttribute(methodSymbol.ContainingType))
                return;

            if (!HasTestCaseAttribute(methodSymbol))
                return;

            if (!ContainsGodotTypes(methodSymbol, context.Compilation, methodBody.SemanticModel!))
                return;

            ReportMethodDiagnostic(context, methodSymbol);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    /// <summary>
    ///     Analyzes test hooks ([Before], [After], etc.) for Godot dependencies
    ///     and verifies the containing class uses [RequireGodotRuntime].
    /// </summary>
    private static void AnalyzeHookMethod(OperationAnalysisContext context)
    {
        try
        {
            if (context.Operation is not IMethodBodyOperation methodBody)
                return;
            var methodSymbol = methodBody.SemanticModel?.GetDeclaredSymbol(methodBody.Syntax) as IMethodSymbol;
            if (methodSymbol == null)
                return;

            // Skip analysis if class has RequireGodotRuntime attribute
            if (HasGodotRuntimeAttribute(methodSymbol.ContainingType))
                return;

            if (!IsTestHook(methodSymbol))
                return;

            if (!ContainsGodotTypes(methodSymbol, context.Compilation, methodBody.SemanticModel!))
                return;

            ReportClassDiagnostic(context, methodSymbol.ContainingType);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private static void ReportClassDiagnostic(in OperationAnalysisContext context, INamedTypeSymbol classSymbol)
    {
        var diagnostic = Diagnostic.Create(
            GodotEngine.RequiresGodotRuntime,
            classSymbol.Locations[0],
            classSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static void ReportMethodDiagnostic(in OperationAnalysisContext context, IMethodSymbol methodSymbol)
    {
        var diagnostic = Diagnostic.Create(
            GodotEngine.GodotNativeCallNotAllowed,
            methodSymbol.Locations[0],
            methodSymbol.Name);
        context.ReportDiagnostic(diagnostic);
    }


    private static bool IsTestHook(IMethodSymbol method)
        => method.GetAttributes()
            .Any(attr => TestHookAttributes.Contains(attr.AttributeClass?.ToDisplayString()));

    private static bool HasGodotRuntimeAttribute(INamedTypeSymbol classSymbol)
        => classSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == RequireGodotRuntimeAttribute);

    private static bool HasTestCaseAttribute(IMethodSymbol methodSymbol)
        => methodSymbol.GetAttributes()
            .Any(attr => attr.AttributeClass?.ToDisplayString() == TestCaseAttribute);

    private static bool ContainsGodotTypes(IMethodSymbol method, Compilation compilation, SemanticModel semanticModel, HashSet<IMethodSymbol>? visitedMethods = null)
    {
        visitedMethods ??= new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        if (!visitedMethods.Add(method))
            return false;

        return ContainsGodotTypesInMethod(method, compilation, semanticModel, visitedMethods);
    }

    private static bool ContainsGodotTypesInMethod(IMethodSymbol method, Compilation compilation, SemanticModel semanticModel, HashSet<IMethodSymbol> visitedMethods)
        => AnalyzeMethodImplementation(method, compilation, semanticModel, visitedMethods);

    private static bool AnalyzeMethodImplementation(IMethodSymbol method, Compilation compilation, SemanticModel semanticModel, HashSet<IMethodSymbol> visitedMethods)
    {
        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return false;

        if (ContainsGodotTypesInMethodSignature(method))
            return true;

        if (ContainsGodotTypesInMethodBodyFieldsOrProperties(method, compilation, semanticModel))
            return true;

        var methodOperation = GetMethodOperationTree(syntaxRef, compilation, semanticModel);
        if (methodOperation == null)
            return false;

        return ContainsGodotTypesInOperationTree(methodOperation, compilation, visitedMethods);
    }

    private static bool ContainsGodotTypesInMethodSignature(IMethodSymbol method)
        => InheritsFromGodotType(method.ReturnType) || method.Parameters.Any(param => InheritsFromGodotType(param.Type));

    private static bool ContainsGodotTypesInMethodBodyFieldsOrProperties(IMethodSymbol method, Compilation compilation, SemanticModel semanticModel)
    {
        if (method.ContainingType == null)
            return false;

        var syntaxRef = method.DeclaringSyntaxReferences.FirstOrDefault();
        if (syntaxRef == null)
            return false;

        var methodOperation = GetMethodOperationTree(syntaxRef, compilation, semanticModel);
        if (methodOperation == null)
            return false;

        foreach (var operation in methodOperation.Descendants())
            switch (operation)
            {
                case IFieldReferenceOperation fieldRef:
                    if (ContainsGodotTypesInField(fieldRef.Field, semanticModel))
                        return true;
                    break;

                case IPropertyReferenceOperation propRef:
                    if (InheritsFromGodotType(propRef.Property.Type))
                        return true;
                    break;
            }

        return false;
    }

    private static bool ContainsGodotTypesInField(IFieldSymbol field, SemanticModel semanticModel)
    {
        if (InheritsFromGodotType(field.Type))
            return true;

        if (field.DeclaringSyntaxReferences.Length == 0)
            return false;

        var fieldSyntax = field.DeclaringSyntaxReferences[0].GetSyntax() as VariableDeclaratorSyntax;
        if (fieldSyntax?.Initializer?.Value == null)
            return false;

        var fieldOperation = semanticModel.GetOperation(fieldSyntax.Initializer.Value);
        return fieldOperation != null && ContainsGodotTypesInOperationTree(fieldOperation, semanticModel.Compilation, null);
    }

    private static IOperation? GetMethodOperationTree(SyntaxReference syntaxRef, Compilation compilation, SemanticModel semanticModel)
    {
        var methodSyntax = syntaxRef.GetSyntax() as MethodDeclarationSyntax;
        if (methodSyntax?.Body == null && methodSyntax?.ExpressionBody == null)
            return null;


        return semanticModel.GetOperation(methodSyntax);
        /*
        try
        {
            return semanticModel.GetOperation(methodSyntax);
        }
        catch (Exception)
        {
            var tree = syntaxRef.SyntaxTree;
            return compilation.GetSemanticModel(tree).GetOperation(methodSyntax) ?? null;
        }*/
    }

    private static bool ContainsGodotTypesInOperationTree(IOperation operation, Compilation compilation, HashSet<IMethodSymbol>? visitedMethods)
    {
        // Collect all relevant operations first
        var operations = operation.Descendants()
            .Where(op => op is IInvocationOperation or IObjectCreationOperation or IPropertyReferenceOperation)
            .ToList();

        foreach (var op in operations)
            if (AnalyzeOperationNode(op, compilation, visitedMethods))
                return true;
        return false;
    }


    private static bool AnalyzeOperationNode(IOperation operation, Compilation compilation, HashSet<IMethodSymbol>? visitedMethods)
        => operation switch
        {
            ILocalReferenceOperation local => AnalyzeLocalReference(local),
            IObjectCreationOperation creation => AnalyzeObjectCreation(creation),
            IInvocationOperation invocation => AnalyzeMethodCall(invocation, compilation, visitedMethods),
            IPropertyReferenceOperation property => AnalyzePropertyAccessOperation(property),
            IFieldReferenceOperation field => AnalyzeFieldAccess(field),
            IMemberReferenceOperation member => AnalyzeMemberReference(member),
            IArrayElementReferenceOperation array => AnalyzeArrayAccess(array),
            ITypeOfOperation typeOf => AnalyzeTypeOf(typeOf),
            _ => false
        };

    private static bool AnalyzeLocalReference(ILocalReferenceOperation local) =>
        InheritsFromGodotType(local.Type);

    private static bool AnalyzeObjectCreation(IObjectCreationOperation creation) =>
        InheritsFromGodotType(creation.Type);

    private static bool AnalyzeMethodCall(IInvocationOperation invocation, Compilation compilation, HashSet<IMethodSymbol>? visitedMethods)
    {
        // First check the direct Godot usage
        if (InheritsFromGodotType(invocation.Type))
            return true;

        // Check static Godot methods
        if (invocation.TargetMethod.ContainingType?.ToDisplayString().StartsWith("Godot.") == true)
            return true;

        if (invocation.Instance != null && InheritsFromGodotType(invocation.Instance.Type))
            return true;

        try
        {
            // analysis of external methods
            if (invocation.TargetMethod.Locations.All(loc => loc.IsInMetadata))
                return AnalyzeExternalMethod(invocation.TargetMethod, visitedMethods);

            // analysis of internal methods
            return ContainsGodotTypes(invocation.TargetMethod, compilation, invocation.SemanticModel!);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // For methods without source (from metadata), continue with current analysis
        return false;
    }


    private static bool AnalyzeExternalMethod(IMethodSymbol method, HashSet<IMethodSymbol>? visitedMethods = null)
    {
        // do not analyze standard .net methods
        var ns = method.ToDisplayString();
        if (ns.StartsWith("System") ||
            ns.StartsWith("Microsoft") ||
            ns == "global" ||
            ns == "<global namespace>")
            return false;


        visitedMethods ??= new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
        if (!visitedMethods.Add(method))
            return false;


        // Check method attributes
        if (method.GetAttributes().Any(attr => InheritsFromGodotType(attr.AttributeClass)))
            return true;

        // Check return type
        if (InheritsFromGodotType(method.ReturnType))
            return true;

        // Check parameters
        if (method.Parameters.Any(p => InheritsFromGodotType(p.Type)))
            return true;


        return false;
    }


    private static bool AnalyzePropertyAccessOperation(IPropertyReferenceOperation property)
    {
        if (InheritsFromGodotType(property.Type))
            return true;

        // Check the property containing type (for static members)
        if (property.Property.ContainingType?.ToDisplayString().StartsWith("Godot.") == true)
            return true;

        return InheritsFromGodotType(property.Property.Type);
    }

    private static bool AnalyzeFieldAccess(IFieldReferenceOperation field)
    {
        if (InheritsFromGodotType(field.Type))
            return true;

        return InheritsFromGodotType(field.Field.Type);
    }

    private static bool AnalyzeMemberReference(IMemberReferenceOperation member)
    {
        if (InheritsFromGodotType(member.Type))
            return true;

        return InheritsFromGodotType(member.Member.ContainingType);
    }

    private static bool AnalyzeArrayAccess(IArrayElementReferenceOperation array) =>
        InheritsFromGodotType(array.ArrayReference.Type);

    private static bool AnalyzeTypeOf(ITypeOfOperation typeOf) =>
        InheritsFromGodotType(typeOf.TypeOperand);

    private static bool InheritsFromGodotType(ITypeSymbol? type)
    {
        if (type == null)
            return false;

        var typeName = type.ToDisplayString();
        // Check for GdUnit4 types that require Godot
        if (GodotDependentTypes.Contains(typeName))
            return true;

        return GodotTypeCache.GetOrAdd(typeName, _ =>
        {
            var current = type;
            while (current != null)
            {
                var ns = current.ContainingNamespace?.ToDisplayString();
                // Early exits for common namespace patterns
                if (ns == null)
                    return false;
                // Known non-Godot namespaces
                if (ns.StartsWith("System") ||
                    ns.StartsWith("Microsoft") ||
                    ns == "global" ||
                    ns == "<global namespace>")
                    return false;
                if (ns.StartsWith("Godot"))
                    return true;
                current = current.BaseType;
            }

            return false;
        });
    }
}
