namespace GdUnit4.Core.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Api;

using Execution;

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

        var assembly = Assembly.LoadFrom(testAssembly);
        var testSuites = assembly.GetTypes().Where(IsTestSuite).ToList();

        using var codeNavigationProvider = new CodeNavigationDataProvider(testAssembly);

        var testCases = testSuites
            .SelectMany(type => DiscoverTests(logger, codeNavigationProvider, testAssembly, type))
            .ToList();

        logger.LogInfo(testCases.Count == 0
            ? "Discover tests done, no tests found."
            : $"Discover tests done, {testSuites.Count} TestSuites and total {testCases.Count} Tests found.");

        return testCases;
    }

    internal static IEnumerable<TestCaseDescriptor> DiscoverTests(ITestEngineLogger logger, CodeNavigationDataProvider codeNavigationProvider, string testAssembly, Type type)
    {
        var requireEngineMode = type.GetCustomAttributes().Any(attr => attr is RequireGodotRuntimeAttribute);
        var testCases = type.GetMethods()
            .Where(m => m.GetCustomAttributes().Any(attr => attr is TestCaseAttribute))
            .ToList()
            .AsParallel()
            .SelectMany(mi => DiscoverTestCasesFromMethod(mi, codeNavigationProvider, requireEngineMode, testAssembly, type.FullName!))
            .ToList();

        logger.LogInfo($"Discover:  TestSuite {type.FullName} with {testCases.Count} TestCases found.");
        return testCases;
    }

    internal static List<TestCaseDescriptor> DiscoverTestCasesFromMethod(MethodInfo mi,
        CodeNavigationDataProvider codeNavigationProvider,
        bool requireEngineMode,
        string assemblyPath,
        string className)
        => mi.GetCustomAttributes()
            .Where(attr => attr is TestCaseAttribute)
            .Cast<TestCaseAttribute>()
            .Select((attr, index) =>
            {
                var navData = codeNavigationProvider.GetNavigationData(mi);

                return new TestCaseDescriptor
                {
                    Id = Guid.NewGuid(),
                    AssemblyPath = assemblyPath,
                    ManagedType = className,
                    ManagedMethod = mi.Name,
                    FullyQualifiedName = TestCase.BuildFullyQualifiedName(className, mi.Name, attr),
                    SimpleName = TestCase.BuildDisplayName(mi.Name, attr, index, mi.GetCustomAttributes().Count() > 1),
                    AttributeIndex = index,
                    LineNumber = navData.LineNumber,
                    CodeFilePath = navData.CodeFilePath,
                    RequireRunningGodotEngine = requireEngineMode || mi.GetCustomAttributes().Any(attribute => attribute is RequireGodotRuntimeAttribute)
                };
            })
            .OrderBy(test => $"{test.ManagedMethod}:{test.AttributeIndex}")
            .ToList();

    private static bool IsTestSuite(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && Attribute.IsDefined(type, typeof(TestSuiteAttribute))
        && type.FullName != null
        && !type.FullName.StartsWith("Microsoft.VisualStudio.TestTools");
}
