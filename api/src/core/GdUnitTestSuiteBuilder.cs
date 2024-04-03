namespace GdUnit4.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using GdUnit4.Executions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


public class GdUnitTestSuiteBuilder
{
    private const string DEFAULT_TEMP_TS_CS = """
        // GdUnit generated TestSuite

        using Godot;
        using GdUnit4;

        namespace ${name_space}
        {
            using static Assertions;
            using static Utils;

            [TestSuite]
            public class ${suite_class_name}
            {
                // TestSuite generated from
                private const string sourceClazzPath = "${source_resource_path}";

            }
        }
    """;

    private static readonly Dictionary<string, Type> ClazzCache = new();

    public static Dictionary<string, object> Build(string sourcePath, int lineNumber, string testSuitePath)
    {
        var result = new Dictionary<string, object>
        {
            { "path", testSuitePath }
        };
        try
        {
            var classDefinition = ParseFullqualifiedClassName(sourcePath);
            if (classDefinition == null)
            {
                result.Add("error", $"Can't parse class type from {sourcePath}:{lineNumber}.");
                return result;
            }
            var methodToTest = FindMethod(sourcePath, lineNumber) ?? "";
            if (string.IsNullOrEmpty(methodToTest))
            {
                result.Add("error", $"Can't parse method name from {sourcePath}:{lineNumber}.");
                return result;
            }

            // create directory if not exists
            var dir = Path.GetDirectoryName(testSuitePath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir!);

            if (!File.Exists(testSuitePath))
            {
                var template = FillFromTemplate(LoadTestSuiteTemplate(), classDefinition, sourcePath);
                var syntaxTree = CSharpSyntaxTree.ParseText(template);
                //var toWrite = syntaxTree.WithFilePath(testSuitePath).GetCompilationUnitRoot();
                var toWrite = AddTestCase(syntaxTree, methodToTest);

                using (var streamWriter = File.CreateText(testSuitePath))
                {
                    toWrite.WriteTo(streamWriter);
                }
                result.Add("line", TestCaseLineNumber(toWrite, methodToTest));
            }
            else if (methodToTest != null)
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(testSuitePath));
                var toWrite = syntaxTree.WithFilePath(testSuitePath).GetCompilationUnitRoot();
                if (TestCaseExists(toWrite, methodToTest))
                {
                    result.Add("line", TestCaseLineNumber(toWrite, methodToTest));
                    return result;
                }
                toWrite = AddTestCase(syntaxTree, methodToTest);
                using (var streamWriter = File.CreateText(testSuitePath))
                {
                    toWrite.WriteTo(streamWriter);
                }
                result.Add("line", TestCaseLineNumber(toWrite, methodToTest));
            }
            return result;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Can't parse method name from {sourcePath}:{lineNumber}. Error: {e.Message}");
            result.Add("error", e.Message);
            return result;
        }
    }

    internal class ClassDefinition : IEquatable<object>
    {
        public ClassDefinition(string? nameSpace, string name)
        {
            Namespace = nameSpace;
            Name = name;
        }

        public string? Namespace { get; }
        public string Name { get; }
        public string ClassName => Namespace == null ? Name : $"{Namespace}.{Name}";
        public override bool Equals(object? obj)
            => obj is ClassDefinition definition &&
                   Namespace == definition.Namespace &&
                   Name == definition.Name;

        public override int GetHashCode() => HashCode.Combine(Namespace, Name, ClassName);
    }

    internal static ClassDefinition? ParseFullqualifiedClassName(string classPath)
    {
        if (string.IsNullOrEmpty(classPath) || !new FileInfo(classPath).Exists)
        {
            Console.Error.WriteLine($"Class `{classPath}` does not exist.");
            return null;
        }
        try
        {
            var code = File.ReadAllText(classPath);
            var root = CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();
            return ParseClassDefinition(root);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Can't parse namespace of {classPath}. Error: {e.Message}");
            return null;
        }
    }

    private static ClassDefinition ParseClassDefinition(CompilationUnitSyntax root)
    {
        var namespaceSyntax = ParseNameSpaceSyntax(root);
        if (namespaceSyntax != null)
        {
            var classSyntax = namespaceSyntax.Members.OfType<ClassDeclarationSyntax>().First();
            return new ClassDefinition(namespaceSyntax.Name.ToString(), classSyntax.Identifier.ValueText);
        }
        return new ClassDefinition(null, root.Members.OfType<ClassDeclarationSyntax>().First().Identifier.ValueText);
    }

    private static Type? FindClassWithTestSuiteAttribute(string classPath, bool isTestSuite)
    {
        var code = File.ReadAllText(classPath);
        var syntaxTree = CSharpSyntaxTree.ParseText(code);
        var root = syntaxTree.GetCompilationUnitRoot();
        var classDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .FirstOrDefault(classDeclaration =>
            {
                var hasTestSuiteAttribute = classDeclaration.AttributeLists
                    .SelectMany(attrList => attrList.Attributes)
                    .Any(attribute => attribute.Name.ToString() == "TestSuite");
                return isTestSuite ? hasTestSuiteAttribute : !hasTestSuiteAttribute;
            });

        if (classDeclaration != null)
        {
            // Construct full class name with namespace
            var namespaceSyntax = classDeclaration.Ancestors().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()
                ?? classDeclaration.Ancestors().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault() as BaseNamespaceDeclarationSyntax;
            var className = namespaceSyntax != null ? namespaceSyntax!.Name + "." + classDeclaration.Identifier : classDeclaration.Identifier.ValueText;
            return FindTypeOnAssembly(className);
        }
        Console.WriteLine($"Warning: No class found in the provided code ({classPath}).");
        return null;
    }

    public static Type? ParseType(string classPath, bool isTestSuite = false)
    {
        if (string.IsNullOrEmpty(classPath) || !new FileInfo(classPath).Exists)
        {
            Console.WriteLine($"Warning: Class `{classPath}` does not exist.");
            return null;
        }
        return FindClassWithTestSuiteAttribute(classPath, isTestSuite);
    }

    private static BaseNamespaceDeclarationSyntax? ParseNameSpaceSyntax(CompilationUnitSyntax root) =>
        root.Members.OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault() as BaseNamespaceDeclarationSyntax ??
        root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

    public static CsNode? Load(string classPath)
    {
        if (string.IsNullOrEmpty(classPath) || !new FileInfo(classPath).Exists)
        {
            Console.Error.WriteLine($"Parse Error: Class `{classPath}` does not exist.");
            return null;
        }
        try
        {
            var code = File.ReadAllText(classPath);
            var syntaxTree = CSharpSyntaxTree.ParseText(code).WithFilePath(classPath).GetCompilationUnitRoot();
            var classDefinition = ParseClassDefinition(syntaxTree);
            var type = FindTypeOnAssembly(classDefinition.ClassName);
            return type!.GetMethods()
                .Where(mi => mi.IsDefined(typeof(TestCaseAttribute)))
                .Select(mi =>
                {
                    var lineNumber = TestCaseLineNumber(syntaxTree, mi.Name);
                    // collect testcase if multiple TestCaseAttribute exists
                    var testCases = mi.GetCustomAttributes(typeof(TestCaseAttribute))
                        .Cast<TestCaseAttribute>()
                        .Where(attr => attr != null && attr.Arguments?.Length != 0)
                        .Select(attr => TestCase.BuildDisplayName(mi.Name, attr))
                        .ToList();
                    // create test
                    return new CsNode(mi.Name, classPath, lineNumber, testCases);
                })
                .Aggregate(new CsNode(classDefinition.Name, classPath), (acc, node) =>
                {
                    acc.AddChild(node);
                    return acc;
                });
        }
#pragma warning disable CS0168
        catch (Exception e)
        {
#pragma warning restore CS0168
            // ignore exception
            return null;
        }
    }

    private static Type? FindTypeOnAssembly(string clazz)
    {
#pragma warning disable CA1854
        if (ClazzCache.ContainsKey(clazz))
            return ClazzCache[clazz];
#pragma warning restore CA1854
        var type = Type.GetType(clazz);
        if (type != null)
            return type;
        // if the class not found on current assembly lookup over all other loaded assemblies
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(clazz);
            if (type != null)
            {
                // Godot.GD.PrintS("found on assemblyName", $"Name={type.Name} Location={assembly.Location}");
                ClazzCache.Add(clazz, type);
                return type;
            }
        }
        return null;
    }

    private static string LoadTestSuiteTemplate()
    {
        if (Godot.ProjectSettings.HasSetting("gdunit4/templates/testsuite/CSharpScript"))
            return (string)Godot.ProjectSettings.GetSetting("gdunit4/templates/testsuite/CSharpScript");
        return DEFAULT_TEMP_TS_CS;
    }

    private const string TAG_TEST_SUITE_NAMESPACE = "${name_space}";
    private const string TAG_TEST_SUITE_CLASS = "${suite_class_name}";
    private const string TAG_SOURCE_CLASS_NAME = "${source_class}";
    private const string TAG_SOURCE_CLASS_VARNAME = "${source_var}";
    private const string TAG_SOURCE_RESOURCE_PATH = "${source_resource_path}";

    private static string FillFromTemplate(string template, ClassDefinition classDefinition, string classPath) =>
        template
            .Replace(TAG_TEST_SUITE_NAMESPACE, string.IsNullOrEmpty(classDefinition.Namespace) ? "GdUnitDefaultTestNamespace" : classDefinition.Namespace)
            .Replace(TAG_TEST_SUITE_CLASS, classDefinition.Name + "Test")
            .Replace(TAG_SOURCE_RESOURCE_PATH, classPath)
            .Replace(TAG_SOURCE_CLASS_NAME, classDefinition.Name)
            .Replace(TAG_SOURCE_CLASS_VARNAME, classDefinition.Name);

    internal static ClassDeclarationSyntax ClassDeclaration(CompilationUnitSyntax root)
    {
        var namespaceSyntax = ParseNameSpaceSyntax(root);
        return namespaceSyntax == null
            ? root.Members.OfType<ClassDeclarationSyntax>().First()
            : namespaceSyntax.Members.OfType<ClassDeclarationSyntax>().First();
    }

    internal static int TestCaseLineNumber(CompilationUnitSyntax root, string testCaseName)
    {
        var classDeclaration = ClassDeclaration(root);
        // lookup on test cases
        var method = classDeclaration.Members.OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(m => m.Identifier.Text.Equals(testCaseName, StringComparison.Ordinal));
        if (method != null && method.Body != null)
            return method.Body.GetLocation().GetLineSpan().StartLinePosition.Line;
        // If method has no body, return the line of the method declaration
        return method?.Identifier.GetLocation().GetLineSpan().StartLinePosition.Line + 1 ?? -1;
    }

    internal static bool TestCaseExists(CompilationUnitSyntax root, string testCaseName) =>
        ClassDeclaration(root).Members.OfType<MethodDeclarationSyntax>()
            .Any(method => method.Identifier.Text.Equals(testCaseName, StringComparison.Ordinal));

    internal static CompilationUnitSyntax AddTestCase(SyntaxTree syntaxTree, string testCaseName)
    {
        var root = syntaxTree.GetCompilationUnitRoot();
        var programClassSyntax = ClassDeclaration(root);
        var insertAt = programClassSyntax.ChildNodes().Last()!;

        var testCaseAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("TestCase"));
        var attributes = SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(testCaseAttribute));

        var method = SyntaxFactory.MethodDeclaration(
            SyntaxFactory.List<AttributeListSyntax>().Add(attributes),
            SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
            default,
            SyntaxFactory.Identifier(testCaseName),
            default,
            SyntaxFactory.ParameterList(),
            default,
            SyntaxFactory.Block(),
            default,
            default);

        var newBody = SyntaxFactory.Block(SyntaxFactory.ParseStatement("AssertNotYetImplemented();"));
        method = method.ReplaceNode(method.Body!, newBody);
        return root.InsertNodesAfter(insertAt, new[] { method }).NormalizeWhitespace("\t", "\n");
    }

    internal static string? FindMethod(string sourcePath, int lineNumber)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(sourcePath));
        var programClassSyntax = ClassDeclaration(syntaxTree.GetCompilationUnitRoot());
        if (programClassSyntax == null)
        {
            Console.Error.WriteLine($"Can't parse method name from {sourcePath}:{lineNumber}. Error: no class declaration found.");
            return null;
        }

        var spanToFind = syntaxTree.GetText().Lines[lineNumber - 1].Span;
        // lookup on properties
        foreach (var m in programClassSyntax.Members.OfType<PropertyDeclarationSyntax>())
        {
            if (m.FullSpan.IntersectsWith(spanToFind))
                return m.Identifier.Text;
        }
        // lookup on methods
        foreach (var m in programClassSyntax.Members.OfType<MethodDeclarationSyntax>())
        {
            if (m.FullSpan.IntersectsWith(spanToFind))
                return m.Identifier.Text;
        }
        return null;
    }
}
