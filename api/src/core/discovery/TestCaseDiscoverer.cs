namespace GdUnit4.Core.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Api;

using Execution;

using Mono.Cecil;
using Mono.Cecil.Cil;

/// <summary>
///     Discovers test cases in assemblies by scanning for test attributes and analyzing debug information.
/// </summary>
internal static class TestCaseDiscoverer
{
    /// <summary>
    ///     Discovers all test cases in a test assembly.
    /// </summary>
    /// <param name="settings">Test engine settings.</param>
    /// <param name="logger">Logger for discovery process information.</param>
    /// <param name="testAssembly">Path to the test assembly to scan.</param>
    /// <returns>List of discovered test case descriptors.</returns>
    public static List<TestCaseDescriptor> Discover(TestEngineSettings settings, ITestEngineLogger logger, string testAssembly)
    {
        logger.LogInfo($"Discover tests from assembly: {testAssembly}");

        var readerParameters = new ReaderParameters
        {
            ReadSymbols = true,
            SymbolReaderProvider = new PortablePdbReaderProvider()
        };
        using var assemblyDefinition = AssemblyDefinition.ReadAssembly(testAssembly, readerParameters);
        var testSuites = assemblyDefinition.MainModule.Types
            .Where(IsTestSuite)
            .ToList();

        var testCases = testSuites
            .SelectMany(type => DiscoverTests(logger, testAssembly, type))
            .ToList();

        logger.LogInfo(testCases.Count == 0
            ? "Discover tests done, no tests found."
            : $"Discover tests done, {testSuites.Count} TestSuites and total {testCases.Count} Tests found.");

        return testCases;
    }

    internal static IEnumerable<TestCaseDescriptor> DiscoverTests(ITestEngineLogger logger, string testAssembly, TypeDefinition type)
    {
        var requireEngineMode = type.CustomAttributes.Any(IsAttribute<RequireGodotRuntimeAttribute>);
        var testCases = type.Methods
            .Where(m => m.CustomAttributes.Any(IsAttribute<TestCaseAttribute>))
            .ToList()
            .AsParallel()
            .SelectMany(mi => DiscoverTestCasesFromMethod(mi, requireEngineMode, testAssembly, type.FullName))
            .ToList();

        logger.LogInfo($"Discover:  TestSuite {type.FullName} with {testCases.Count} TestCases found.");
        return testCases;
    }

    private static CodeNavigation GetNavigationDataOrDefault(MethodDefinition definition)
    {
        try
        {
            var debugInfo = GetDebugInfo(definition);

            return new CodeNavigation
            {
                LineNumber = debugInfo?.StartLine ?? -1,
                CodeFilePath = debugInfo?.Document.Url ?? "unknown",
                MethodName = definition.Name
            };
        }
        catch (Exception)
        {
            return new CodeNavigation
            {
                LineNumber = -1,
                CodeFilePath = "unknown",
                MethodName = definition.Name
            };
        }
    }

    private static SequencePoint? GetDebugInfo(MethodDefinition definition)
    {
        // Check if this is an async method
        var stateMachineAttribute = definition.CustomAttributes.FirstOrDefault(IsAttribute<AsyncStateMachineAttribute>);
        if (stateMachineAttribute?.ConstructorArguments[0].Value is TypeDefinition stateMachineType)
        {
            // Get the MoveNext method which contains the actual implementation
            var moveNextMethod = stateMachineType.Methods.FirstOrDefault(m => m.Name == "MoveNext");
            if (moveNextMethod != null)
            {
                var debugInfo = moveNextMethod.DebugInformation;
                // Try to find the first sequence point that's not compiler-generated
                return debugInfo?.SequencePoints
                    .FirstOrDefault(sp =>
                        // Compiler-generated code often has special column values or hidden lines
                        !sp.IsHidden &&
                        sp.StartColumn > 0 &&
                        sp.Document.Url.EndsWith(".cs"));
            }
        }

        // Default handling for non-async methods
        return definition.DebugInformation?.SequencePoints.FirstOrDefault();
    }

    internal static List<TestCaseDescriptor> DiscoverTestCasesFromMethod(
        MethodDefinition mi,
        bool requireEngineMode,
        string assemblyPath,
        string className)
        => mi.CustomAttributes
            .Where(IsAttribute<TestCaseAttribute>)
            .Select((attr, index) =>
            {
                var navData = GetNavigationDataOrDefault(mi);
                var testCaseAttribute = CreateTestCaseAttribute(attr);

                return new TestCaseDescriptor
                {
                    Id = Guid.NewGuid(),
                    AssemblyPath = assemblyPath,
                    ManagedType = className,
                    ManagedMethod = mi.Name,
                    FullyQualifiedName = TestCase.BuildFullyQualifiedName(className, mi.Name, testCaseAttribute),
                    SimpleName = TestCase.BuildDisplayName(mi.Name, testCaseAttribute, index, mi.CustomAttributes.Count > 1),
                    AttributeIndex = index,
                    LineNumber = navData.LineNumber,
                    CodeFilePath = navData.CodeFilePath,
                    RequireRunningGodotEngine = requireEngineMode || mi.CustomAttributes.Any(IsAttribute<RequireGodotRuntimeAttribute>)
                };
            })
            .OrderBy(test => $"{test.ManagedMethod}:{test.AttributeIndex}")
            .ToList();

    private static TestCaseAttribute CreateTestCaseAttribute(CustomAttribute attribute)
    {
        if (attribute.ConstructorArguments.Count == 0)
            return new TestCaseAttribute();

        var args = Array.Empty<object?>();
        var firstArg = attribute.ConstructorArguments[0];
        if (firstArg.Value is CustomAttributeArgument[] constructorArgs)
            args = constructorArgs
                .Select(arg =>
                {
                    if (arg.Value is CustomAttributeArgument customAttribute)
                        return customAttribute.Value;
                    return arg.Value;
                })
                .ToArray();

        // Create the TestCase attribute instance with constructor arguments
        var testCase = new TestCaseAttribute(args);

        // Handle properties
        foreach (var prop in attribute.Properties)
            switch (prop.Name)
            {
                case "TestName":
                    testCase.TestName = prop.Argument.Value?.ToString();
                    break;
                case "Seed":
                    if (prop.Argument.Value is double seed)
                        testCase.Seed = seed;
                    break;
                case "Iterations":
                    if (prop.Argument.Value is int iterations)
                        testCase.Iterations = iterations;
                    break;
                case "Description":
                    if (prop.Argument.Value is string description)
                        testCase.Description = description;
                    break;
            }

        return testCase;
    }

    private static bool IsAttribute<TAttribute>(CustomAttribute attribute) where TAttribute : Attribute
        => string.Equals(attribute.AttributeType.FullName, typeof(TAttribute).FullName, StringComparison.Ordinal);

    private static bool IsTestSuite(TypeDefinition type) =>
        type is { IsClass: true, IsAbstract: false, HasCustomAttributes: true }
        && type.CustomAttributes.Any(attr => attr.AttributeType.Name == "TestSuiteAttribute")
        && type.FullName != null
        && !type.FullName.StartsWith("Microsoft.VisualStudio.TestTools");
}
