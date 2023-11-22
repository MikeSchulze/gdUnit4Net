using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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
            "GdUnit4.TestAdapter.TestClassName",
            "ClassName",
            typeof(string),
            TestPropertyAttributes.Hidden,
            typeof(TestCase));

    public void DiscoverTests(
        IEnumerable<string> assemblyPaths,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        ISettingsProvider t;
        logger.SendMessage(TestMessageLevel.Informational, $"RunSettings: {discoveryContext.RunSettings}:{discoveryContext.RunSettings?.SettingsXml}");
        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(discoveryContext.RunSettings?.SettingsXml);
        logger.SendMessage(TestMessageLevel.Informational, $"RunConfiguration: {runConfiguration.TestSessionTimeout}");

        var filteredAssemblys = FilterWithoutTestAdapter(assemblyPaths);
        foreach (string assemblyPath in filteredAssemblys)
        {
            logger.SendMessage(TestMessageLevel.Informational, $"Discover tests for assembly: {assemblyPath}");
            Assembly? assembly = LoadAssembly(assemblyPath, logger);
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
                        //logger.SendMessage(TestMessageLevel.Informational, $"Discover test {assemblyPath}, {className}, {testName}");
                        GetCodeNavigationInfos(assemblyPath, className, testName, out var lineNumber, out var classFilePath);
                        //logger.SendMessage(TestMessageLevel.Informational, $"-> {fileName}:{lineNumber}");

                        var fullyQualifiedName = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", className, testName);
                        TestCase testCase = new(fullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), assemblyPath)
                        {
                            DisplayName = testName,
                            FullyQualifiedName = fullyQualifiedName,
                            CodeFilePath = classFilePath,
                            LineNumber = lineNumber
                        };
                        testCase.SetPropertyValue(TestClassNameProperty, testCase.DisplayName);
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


    /// <summary>
    /// Gets the code navigation data for given class and method.
    /// </summary>
    /// <param name="assemblyPath"> The assembly path. </param>
    /// <param name="className"> The class name. </param>
    /// <param name="methodName"> The method name. </param>
    /// <param name="lineNumber"> The line number as output. </param>
    /// <param name="classFilePath"> The class path as output. </param>
    private static void GetCodeNavigationInfos(string assemblyPath, string className, string methodName, out int lineNumber, out string? classFilePath)
    {
        classFilePath = null;
        lineNumber = -1;

        try
        {
            DiaSession diaSession = new(assemblyPath);
            var navigationData = diaSession.GetNavigationData(className, methodName);
            if (navigationData != null)
            {
                lineNumber = navigationData.MinLineNumber;
                classFilePath = navigationData.FileName;
            }
        }
        catch (Exception e)
        {
            throw new Exception($"Could not create diaSession for source: {assemblyPath}. Reason:{e.Message}");
        }
    }

    private static bool IsTestSuite(Type type) =>
        type.IsClass && !type.IsAbstract && Attribute.IsDefined(type, typeof(TestSuiteAttribute));

}