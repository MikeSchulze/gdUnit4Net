using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GdUnit4.TestAdapter.Discovery;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

namespace GdUnit4.TestAdapter;

[DefaultExecutorUri(GdUnit4TestExecutor.ExecutorUri)]
[ExtensionUri(GdUnit4TestExecutor.ExecutorUri)]
[FileExtension(".dll")]
[FileExtension(".exe")]
public sealed class GdUnit4TestDiscoverer : ITestDiscoverer
{

    internal static readonly TestProperty TestClassNameProperty = TestProperty.Register(
            "GdUnit4.Test",
            "SuiteName",
            typeof(string),
            TestPropertyAttributes.Hidden,
            typeof(TestCase));

    public void DiscoverTests(
        IEnumerable<string> assemblyPaths,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        //logger.SendMessage(TestMessageLevel.Informational, $"RunSettings: {discoveryContext.RunSettings}:{discoveryContext.RunSettings?.SettingsXml}");
        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(discoveryContext.RunSettings?.SettingsXml);
        //logger.SendMessage(TestMessageLevel.Informational, $"RunConfiguration: {runConfiguration.TestSessionTimeout}");

        var filteredAssemblys = FilterWithoutTestAdapter(assemblyPaths);
        foreach (string assemblyPath in filteredAssemblys)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"Discover tests for assembly: {assemblyPath}");
            Assembly? assembly = LoadAssembly(assemblyPath, logger);

            var codeNavigationProvider = new CodeNavigationDataProvider(assemblyPath);

            if (assembly == null) continue;

            // discover GdUnit4 testsuites
            foreach (var type in assembly.GetTypes().Where(IsTestSuite))
            {
                // discover test cases
                var className = type.FullName!;
                type.GetMethods()
                    .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
                    .AsParallel()
                    .ForAll(mi =>
                    {
                        var testName = mi.Name;
                        var navData = codeNavigationProvider.GetNavigationData(className, testName);
                        if (!navData.IsValid)
                            logger.SendMessage(TestMessageLevel.Informational, $"Error Discover test {className}:{testName}    GetNavigationData -> {navData.Source}:{navData.Line}");

                        var fullyQualifiedName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", className, testName);
                        TestCase testCase = new(fullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), assemblyPath)
                        {
                            DisplayName = testName,
                            FullyQualifiedName = fullyQualifiedName,
                            CodeFilePath = navData.Source,
                            LineNumber = navData.Line

                        };
                        testCase.SetPropertyValue(TestClassNameProperty, testCase.DisplayName);
                        //logger.SendMessage(TestMessageLevel.Informational, $"Added TestCase: {testCase.DisplayName} {testCase}");
                        discoverySink.SendTestCase(testCase);
                    });
            };
        }
    }

    private static IEnumerable<string> FilterWithoutTestAdapter(IEnumerable<string> assemblyPaths) =>
        assemblyPaths.Where(assembly => !assembly.Contains(".TestAdapter."));

    private static Assembly? LoadAssembly(string assemblyPath, IMessageLogger logger)
    {
        try
        {
            return Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception e)
        {
            logger.SendMessage(TestMessageLevel.Error, e.Message);
            return null;
        }
    }

    private static bool IsTestSuite(Type type) =>
        type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

}