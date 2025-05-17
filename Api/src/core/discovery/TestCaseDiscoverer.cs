// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Discovery;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Api;

using Attributes;

using Godot;

using Mono.Cecil;
using Mono.Cecil.Cil;

using Runners;

/// <summary>
///     Discovers test cases in assemblies by scanning for test attributes and analyzing debug information.
/// </summary>
internal static class TestCaseDiscoverer
{
#pragma warning disable CA1859
    private static readonly ITestEngineLogger Logger = new GodotLogger();
#pragma warning restore CA1859

    // Static cache for assembly locations and names
    private static readonly Dictionary<string, ISet<string>> AssemblyLocationCache = new();
    private static string? cachedAssemblyName;

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

    internal static IEnumerable<TestCaseDescriptor> DiscoverTests(ITestEngineLogger? logger, string testAssembly, TypeDefinition type)
    {
        var requireEngineMode = type.CustomAttributes.Any(IsAttribute<RequireGodotRuntimeAttribute>);
        var typeCategories = GetCategories(type);
        var typeTraits = GetTraits(type);

        var testCases = type.Methods
            .Where(m => m.CustomAttributes.Any(IsAttribute<TestCaseAttribute>))
            .ToList()
            .AsParallel()
            .SelectMany(mi => DiscoverTestCasesFromMethod(logger, mi, testAssembly, requireEngineMode, type.FullName, typeCategories, typeTraits))
            .ToList();

        logger?.LogInfo($"Discover:  TestSuite {type.FullName} with {testCases.Count} TestCases found.");
        return testCases;
    }

    private static CodeNavigation GetNavigationDataOrDefault(MethodDefinition definition)
    {
        try
        {
            if (definition.DebugInformation?.SequencePoints == null)
            {
                return new CodeNavigation
                {
                    LineNumber = -1,
                    CodeFilePath = "unknown",
                    MethodName = definition.Name
                };
            }

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
        ITestEngineLogger? logger,
        MethodDefinition mi,
        string assemblyPath,
        bool classRequireEngineMode,
        string className,
        List<string> classCategories,
        Dictionary<string, List<string>> classTraits)
    {
        var attributes = mi.CustomAttributes
            .Where(IsAttribute<TestCaseAttribute>)
            .ToList();
        var hasMultipleAttributes = attributes.Count > 1;
        var requireRunningGodotEngine = classRequireEngineMode || mi.CustomAttributes.Any(IsAttribute<RequireGodotRuntimeAttribute>);
        var methodCategories = GetCategories(mi);
        var methodTraits = GetTraits(mi);
        var allCategories = classCategories.Concat(methodCategories).Distinct().ToList();
        var allTraits = CombineTraits(classTraits, methodTraits);

        return attributes
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
                    AttributeIndex = index,
                    LineNumber = navData.LineNumber,
                    CodeFilePath = navData.CodeFilePath,
                    RequireRunningGodotEngine = requireRunningGodotEngine,
                    Categories = allCategories,
                    Traits = allTraits
                }
                    .Build(testCaseAttribute, hasMultipleAttributes);
            })
            .OrderBy(test => $"{test.ManagedMethod}:{test.AttributeIndex}")
            .ToList();
    }

    /// <summary>
    ///     Extracts category information from a type or method definition.
    /// </summary>
    /// <param name="definition">The type or method definition to check for category attributes.</param>
    /// <returns>A list of categories.</returns>
    private static List<string> GetCategories(ICustomAttributeProvider definition)
    {
        var categories = new List<string>();

        var testCategoryAttributes = definition.CustomAttributes
            .Where(IsAttribute<TestCategoryAttribute>)
            .ToList();

        foreach (var attr in testCategoryAttributes)
        {
            if (attr.ConstructorArguments.Count > 0 && attr.ConstructorArguments[0].Value is string category)
                categories.Add(category);
        }

        return categories;
    }

    /// <summary>
    ///     Extracts trait information from a type or method definition.
    /// </summary>
    /// <param name="definition">The type or method definition to check for trait attributes.</param>
    /// <returns>A dictionary of trait names and their values.</returns>
    private static Dictionary<string, List<string>> GetTraits(ICustomAttributeProvider definition)
    {
        var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var traitAttributes = definition.CustomAttributes
            .Where(IsAttribute<TraitAttribute>)
            .ToList();

        foreach (var attr in traitAttributes)
        {
            if (attr.ConstructorArguments.Count >= 2 &&
                attr.ConstructorArguments[0].Value is string traitName &&
                attr.ConstructorArguments[1].Value is string traitValue)
            {
                if (!traits.TryGetValue(traitName, out var values))
                {
                    values = new List<string>();
                    traits[traitName] = values;
                }

                if (!values.Contains(traitValue))
                    values.Add(traitValue);
            }
        }

        return traits;
    }

    /// <summary>
    ///     Combines two trait dictionaries.
    /// </summary>
    /// <param name="first">The first dictionary of traits.</param>
    /// <param name="second">The second dictionary of traits.</param>
    /// <returns>A combined dictionary of traits.</returns>
    private static Dictionary<string, List<string>> CombineTraits(
        Dictionary<string, List<string>> first,
        Dictionary<string, List<string>> second)
    {
        var result = new Dictionary<string, List<string>>(first, StringComparer.OrdinalIgnoreCase);

        foreach (var trait in second)
        {
            if (!result.TryGetValue(trait.Key, out var values))
            {
                values = new List<string>();
                result[trait.Key] = values;
            }

            foreach (var value in trait.Value)
            {
                if (!values.Contains(value))
                    values.Add(value);
            }
        }

        return result;
    }

    private static TestCaseAttribute CreateTestCaseAttribute(CustomAttribute attribute)
    {
        if (attribute.ConstructorArguments.Count == 0)
            return new TestCaseAttribute();

        var args = Array.Empty<object?>();
        var firstArg = attribute.ConstructorArguments[0];
        if (firstArg.Value is CustomAttributeArgument[] constructorArgs)
        {
            args = constructorArgs
                .Select(arg =>
                {
                    if (arg.Value is CustomAttributeArgument customAttribute)
                        return customAttribute.Value;
                    return arg.Value;
                })
                .ToArray();
        }

        // Create the TestCase attribute instance with constructor arguments
        var testCase = new TestCaseAttribute(args);

        // Handle properties
        foreach (var prop in attribute.Properties)
        {
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
                default:
                    break;
            }
        }

        return testCase;
    }

    private static bool IsAttribute<TAttribute>(CustomAttribute attribute)
        where TAttribute : Attribute
        => string.Equals(attribute.AttributeType.FullName, typeof(TAttribute).FullName, StringComparison.Ordinal);

    private static bool IsTestSuite(TypeDefinition type) =>
        type is { IsClass: true, IsAbstract: false, HasCustomAttributes: true }
        && type.CustomAttributes.Any(attr => attr.AttributeType.Name == "TestSuiteAttribute")
        && type.FullName != null
        && !type.FullName.StartsWith("Microsoft.VisualStudio.TestTools");

    public static List<TestCaseDescriptor> DiscoverTestCasesFromScript(CSharpScript sourceScript)
    {
        ArgumentNullException.ThrowIfNull(sourceScript);

        try
        {
            var fullScriptPath = Path.GetFullPath(ProjectSettings.GlobalizePath(sourceScript.ResourcePath));
            var className = Path.GetFileNameWithoutExtension(fullScriptPath);

            foreach (var assemblyPath in TestAssemblyLocations())
            {
                // Create a custom resolver that can handle GodotSharp
                var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath));

                // Create reader parameters with the custom resolver
                var readerParameters = new ReaderParameters
                {
                    ReadSymbols = true,
                    SymbolReaderProvider = new PortablePdbReaderProvider(),
                    AssemblyResolver = resolver,
                    ReadingMode = ReadingMode.Deferred // Use deferred loading to avoid resolving references right away
                };

                // lookup for class in the assembly
                using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath, readerParameters);
                var testSuite = assembly?.MainModule.Types
                    .FirstOrDefault(definition => definition.Name == className && HasSourceFilePath(definition, fullScriptPath));

                // No suite found, try next assembly
                if (assembly == null || testSuite == null)
                    continue;

                return DiscoverTests(null, assembly.MainModule.FileName, testSuite).ToList();
            }

            Logger.LogWarning($"Could not find assembly or test suite for {className} at {fullScriptPath}");
            return new List<TestCaseDescriptor>();
        }
        catch (Exception e)
        {
            Logger.LogError($"Error discovering tests: {e.Message}\n{e.StackTrace}");
            return new List<TestCaseDescriptor>();
        }
    }

    // Initializes and caches potential assembly locations
    private static ISet<string> TestAssemblyLocations()
    {
        lock (AssemblyLocationCache)
        {
            if (AssemblyLocationCache.Count != 0)
            {
                return AssemblyLocationCache.TryGetValue(TestAssemblyName(), out var locations)
                    ? locations
                    : new HashSet<string>();
            }

            var assemblyName = TestAssemblyName();
            var assemblyPaths = FindAllAssemblyPaths(Logger, assemblyName);
            AssemblyLocationCache[assemblyName] = assemblyPaths;

            Logger.LogInfo($"Initialized {assemblyPaths.Count} test assembly locations {string.Join(",", assemblyPaths.ToList())}");
            return assemblyPaths;
        }
    }

    private static HashSet<string> FindAllAssemblyPaths(ITestEngineLogger logger, string assemblyName)
    {
        // Normalize path
        var projectDir = Path.GetFullPath(ProjectSettings.GlobalizePath("res://"));
        var assemblyDirs = new HashSet<string>
        {
            // Godot 4.x build directories
            Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "Debug"),
            Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "Release"),
            Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "ExportDebug"),
            Path.Combine(projectDir, ".godot", "mono", "temp", "bin", "ExportRelease")
        };

        var assemblyLocations = new HashSet<string>();
        foreach (var dir in assemblyDirs.Where(Directory.Exists))
        {
            // Check for assembly with project/specified name
            var specificAssembly = Path.Combine(dir, $"{assemblyName}.dll");
            if (!File.Exists(specificAssembly))
                continue;
            assemblyLocations.Add(specificAssembly);
            logger.LogInfo($"Found named assembly: {specificAssembly}");

            // Check for other assemblies in the directory
            /*
            foreach (var file in Directory.GetFiles(dir, "*.dll"))
                if (!assemblyLocations.Contains(file))
                {
                    assemblyLocations.Add(file);
                    logger.LogInfo($"Found additional assembly: {file}");
                }
                */
        }

        return assemblyLocations;
    }

    // Gets the assembly name from project settings with caching
    private static string TestAssemblyName()
    {
        if (cachedAssemblyName != null)
            return cachedAssemblyName;

        var projectName = ProjectSettings.GetSetting("application/config/name").AsString();
        var assemblyName = ProjectSettings.GetSetting("dotnet/project/assembly_name").AsString();

        cachedAssemblyName = string.IsNullOrEmpty(assemblyName) ? projectName : assemblyName;
        return cachedAssemblyName;
    }

    private static bool HasSourceFilePath(TypeDefinition type, string sourceScriptPath)
        => type.Methods.Any(method =>
        {
            var navData = GetNavigationDataOrDefault(method);
            return navData.CodeFilePath == sourceScriptPath;
        });
}
