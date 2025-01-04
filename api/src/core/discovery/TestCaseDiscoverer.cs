namespace GdUnit4.Core.Discovery;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Execution;

internal static class TestCaseDiscoverer
{
    public static List<TestCaseDescriptor> Discover(TestEngineSettings settings, ITestEngineLogger logger, string testAssembly)
    {
        logger.LogInfo($"Discover tests from assembly: {testAssembly}");

        var assembly = Assembly.LoadFrom(testAssembly);
        var testSuites = assembly.GetTypes().Where(IsTestSuite).ToList();

        var testCases = testSuites.SelectMany(type =>
            {
                var testCases = type.GetMethods()
                    .Where(m => m.GetCustomAttributes().Any(attr => attr is TestCaseAttribute))
                    .ToList()
                    .AsParallel()
                    .SelectMany(mi => DiscoverTestCasesFromMethod(mi, testAssembly, type.FullName!))
                    .ToList();

                logger.LogInfo($"Discover:  TestSuite {type.FullName} with {testCases.Count} TestCases found.");
                return testCases;
            })
            .ToList();

        logger.LogInfo(testCases.Count == 0
            ? "Discover tests done, no tests found."
            : $"Discover tests done, {testSuites.Count} TestSuites and total {testCases.Count} Tests found.");

        return testCases;
    }

    internal static List<TestCaseDescriptor> DiscoverTestCasesFromMethod(
        MethodInfo mi,
        string assemblyPath,
        string className)
        => mi.GetCustomAttributes()
            .Where(attr => attr is TestCaseAttribute)
            .Cast<TestCaseAttribute>()
            .Select((attr, index) => new TestCaseDescriptor
            {
                Id = Guid.NewGuid(),
                AssemblyPath = assemblyPath,
                ManagedType = className,
                ManagedMethod = mi.Name,
                FullyQualifiedName = TestCase.BuildFullyQualifiedName(className, mi.Name, attr),
                SimpleName = TestCase.BuildDisplayName(mi.Name, attr, index, mi.GetCustomAttributes().Count() > 1),
                AttributeIndex = index,
                LineNumber = 0,
                RequireRunningGodotEngine = attr is GodotTestCaseAttribute
            })
            .OrderBy(test => test.ManagedMethod)
            .ToList();

    private static bool IsTestSuite(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && Attribute.IsDefined(type, typeof(TestSuiteAttribute))
        && type.FullName != null
        && !type.FullName.StartsWith("Microsoft.VisualStudio.TestTools");
}
