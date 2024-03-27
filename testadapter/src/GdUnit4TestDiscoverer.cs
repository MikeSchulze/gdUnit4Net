namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using static GdUnit4.TestAdapter.Discovery.CodeNavigationDataProvider;
using static GdUnit4.TestAdapter.Settings.GdUnit4Settings;
using GdUnit4.TestAdapter.Discovery;
using GdUnit4.TestAdapter.Settings;


[DefaultExecutorUri(GdUnit4TestExecutor.ExecutorUri)]
[ExtensionUri(GdUnit4TestExecutor.ExecutorUri)]
[FileExtension(".dll")]
[FileExtension(".exe")]
public sealed class GdUnit4TestDiscoverer : ITestDiscoverer
{

    internal static readonly TestProperty TestMethodNameProperty = TestProperty.Register(
            "GdUnit4.Property.TestMethodName",
            "TestMethodName",
            typeof(string),
            TestPropertyAttributes.Hidden,
            typeof(TestCase));

    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(discoveryContext.RunSettings?.SettingsXml);
        var gdUnitSettingsProvider = discoveryContext.RunSettings?.GetSettings(RunSettingsXmlNode) as GdUnit4SettingsProvider;
        var gdUnitSettings = gdUnitSettingsProvider?.Settings ?? new GdUnit4Settings();
        var filteredAssembles = FilterWithoutTestAdapter(sources);

        foreach (var assemblyPath in filteredAssembles)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"Discover tests for assembly: {assemblyPath}");

            using var codeNavigationProvider = new CodeNavigationDataProvider(assemblyPath, logger);
            if (codeNavigationProvider.GetAssembly() == null)
                continue;
            var assembly = codeNavigationProvider.GetAssembly()!;

            var testsTotalDiscovered = 0;
            var testSuiteDiscovered = 0;
            // discover GdUnit4 testsuites
            foreach (var type in assembly.GetTypes().Where(IsTestSuite))
            {
                // discover test cases
                var testCasesDiscovered = 0;
                testSuiteDiscovered++;
                var className = type.FullName!;
                type.GetMethods()
                    .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
                    .AsParallel()
                    .ForAll(mi =>
                    {
                        var navData = codeNavigationProvider.GetNavigationData(className, mi);
                        if (!navData.IsValid)
                            logger.SendMessage(TestMessageLevel.Informational, $"Can't collect code navigation data for {className}:{mi.Name}    GetNavigationData -> {navData.Source}:{navData.Line}");

                        // Collect parameterized tests or build a single test
                        mi.GetCustomAttributes(typeof(TestCaseAttribute))
                            .Cast<TestCaseAttribute>()
                            .Where(attr => attr != null && attr.Arguments?.Length != 0)
                            .Select(attr =>
                            {
                                var parameterizedTestName = Executions.TestCase.BuildTestCaseName(mi.Name, attr);
                                return new
                                {
                                    TestName = parameterizedTestName,
                                    FullyQualifiedName = $"{mi.DeclaringType}.{parameterizedTestName}"
                                };
                            })
                            .DefaultIfEmpty(new
                            {
                                TestName = $"{mi.Name}",
                                FullyQualifiedName = $"{mi.DeclaringType}.{mi.Name}"
                            })
                            .Select(test => BuildTestCase(test.FullyQualifiedName, test.TestName, assemblyPath, navData, gdUnitSettings))
                            .ToList()
                            .ForEach(t =>
                            {
                                Interlocked.Increment(ref testsTotalDiscovered);
                                Interlocked.Increment(ref testCasesDiscovered);
                                discoverySink.SendTestCase(t);
                            });
                    });
                logger.SendMessage(TestMessageLevel.Informational, $"Discover:  TestSuite {className} with {testCasesDiscovered} TestCases found.");
            };

            logger.SendMessage(TestMessageLevel.Informational, $"Discover tests done, {testSuiteDiscovered} TestSuites and total {testsTotalDiscovered} Tests found.");
        }
    }

    private TestCase BuildTestCase(string fullyQualifiedName, string testName, string assemblyPath, CodeNavigation navData, GdUnit4Settings gdUnitSettings)
    {
        TestCase testCase = new(fullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), assemblyPath)
        {
            DisplayName = BuildDisplayName(testName, fullyQualifiedName, gdUnitSettings),
            FullyQualifiedName = fullyQualifiedName,
            CodeFilePath = navData.Source,
            LineNumber = navData.Line
        };
        testCase.SetPropertyValue(TestMethodNameProperty, testName);
        return testCase;
    }

    private static string BuildDisplayName(string testName, string fullyQualifiedName, GdUnit4Settings gdUnitSettings) =>
        gdUnitSettings.DisplayName switch
        {
            DisplayNameOptions.SimpleName => testName,
            DisplayNameOptions.FullyQualifiedName => fullyQualifiedName,
            _ => testName
        };

    private static IEnumerable<string> FilterWithoutTestAdapter(IEnumerable<string> assemblyPaths) =>
        assemblyPaths.Where(assembly => !assembly.Contains(".TestAdapter."));

    private static bool IsTestSuite(Type type) =>
        type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

}
