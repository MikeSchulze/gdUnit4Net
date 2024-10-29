namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

using Core.Extensions;

using Discovery;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.TestPlatform.AdapterUtilities.ManagedNameUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Settings;

using static Discovery.CodeNavigationDataProvider;

using static Settings.GdUnit4Settings;

using static Extensions.TestCaseExtensions;

using static Utilities.Utils;

[DefaultExecutorUri(GdUnit4TestExecutor.ExecutorUri)]
[ExtensionUri(GdUnit4TestExecutor.ExecutorUri)]
[FileExtension(".dll")]
[FileExtension(".exe")]
public sealed class GdUnit4TestDiscoverer : ITestDiscoverer
{
    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        if (!CheckGdUnit4ApiMinimumRequiredVersion(logger, new Version("4.4.0")))
        {
            logger.SendMessage(TestMessageLevel.Error, "Abort the test discovery.");
            return;
        }

        var gdUnitSettings = GdUnit4Settings(discoveryContext);
        var filteredAssembles = FilterWithoutTestAdapter(sources);

        logger.SendMessage(TestMessageLevel.Informational, $"GdUnit4Settings: {gdUnitSettings.DisplayName}");

        foreach (var assemblyPath in filteredAssembles)
            try
            {
                DiscoverTestSuitesFromAssembly(logger, discoverySink, assemblyPath, gdUnitSettings);
            }
            catch (Exception e)
            {
                logger.SendMessage(TestMessageLevel.Error, $"Error discovering tests in {assemblyPath}: {e}");
            }
    }

    private void DiscoverTestSuitesFromAssembly(IMessageLogger logger, ITestCaseDiscoverySink discoverySink, string assemblyPath, GdUnit4Settings gdUnitSettings)
    {
        logger.SendMessage(TestMessageLevel.Informational, $"Discover tests for assembly: {assemblyPath}");

        using var codeNavigationProvider = new CodeNavigationDataProvider(assemblyPath, logger);
        if (codeNavigationProvider.GetAssembly() == null)
            return;
        var assembly = codeNavigationProvider.GetAssembly()!;

        // discover GdUnit4 test suites
        var testSuites = assembly.GetTypes().Where(IsTestSuite).ToList();
        var testsTotalDiscovered = 0;
        testSuites.ForEach(type =>
        {
            var className = type.FullName!;
            var testCases = type.GetMethods()
                .Where(m => m.IsDefined(typeof(TestCaseAttribute)))
                .ToList()
                .AsParallel()
                // ReSharper disable once AccessToDisposedClosure
                .SelectMany(mi => DiscoverTestCasesFromMethod(logger, assemblyPath, gdUnitSettings, codeNavigationProvider, className, mi))
                .OrderBy(t => t.DisplayName)
                .ToList();

            testCases.ForEach(t =>
            {
                testsTotalDiscovered++;
                discoverySink.SendTestCase(t);
            });

            logger.SendMessage(TestMessageLevel.Informational, $"Discover:  TestSuite {className} with {testCases.Count} TestCases found.");
        });

        logger.SendMessage(TestMessageLevel.Informational, $"Discover tests done, {testSuites.Count} TestSuites and total {testsTotalDiscovered} Tests found.");
    }

    private List<TestCase> DiscoverTestCasesFromMethod(IMessageLogger logger, string assemblyPath, GdUnit4Settings gdUnitSettings,
        CodeNavigationDataProvider codeNavigationProvider, string className, MethodInfo mi)
    {
        var navData = codeNavigationProvider.GetNavigationData(className, mi);
        if (!navData.IsValid)
            logger.SendMessage(TestMessageLevel.Informational,
                $"Can't collect code navigation data for {className}:{mi.Name}    GetNavigationData -> {navData.Source}:{navData.Line}");

        ManagedNameHelper.GetManagedName(mi, out var managedType, out var managedMethod, out var hierarchyValues);
        ManagedNameParser.ParseManagedMethodName(managedMethod, out var methodName, out _, out _);
        hierarchyValues[HierarchyConstants.Levels.ContainerIndex] = null;
        hierarchyValues[HierarchyConstants.Levels.TestGroupIndex] = methodName;
        //var isAsync = MatchReturnType(mi, typeof(Task));
        // Collect test cases or build a single test
        return mi.GetCustomAttributes(typeof(TestCaseAttribute))
            .Cast<TestCaseAttribute>()
            .Select((attr, index) => new TestCaseDescriptor
            {
                ManagedType = managedType,
                ManagedMethod = managedMethod,
                HierarchyValues = new ReadOnlyCollection<string?>(hierarchyValues),
                DisplayName = BuildDisplayName(managedType, mi.Name, index, attr, gdUnitSettings),
                FullyQualifiedName = Core.Execution.TestCase.BuildFullyQualifiedName(managedType, mi.Name, attr),
                Traits = TestCasePropertiesAsTraits(attr)
            })
            .Select(testDescriptor => BuildTestCase(testDescriptor, assemblyPath, navData))
            .OrderBy(t => t.DisplayName)
            .ToList();
    }

    private static GdUnit4Settings GdUnit4Settings(IDiscoveryContext discoveryContext)
    {
        //var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(discoveryContext.RunSettings?.SettingsXml);
        var gdUnitSettingsProvider = discoveryContext.RunSettings?.GetSettings(RunSettingsXmlNode) as GdUnit4SettingsProvider;
        var gdUnitSettings = gdUnitSettingsProvider?.Settings ?? new GdUnit4Settings();
        return gdUnitSettings;
    }

    /*
    private static bool MatchReturnType(MethodInfo method, Type returnType)
        => method == null
            ? throw new ArgumentNullException(nameof(method))
            : returnType == null
                ? throw new ArgumentNullException(nameof(returnType))
                : method.ReturnType == returnType;
    */

    private static List<Trait> TestCasePropertiesAsTraits(TestCaseAttribute attr)
        => attr.Arguments
            .Select(arg => new Trait(attr.Name, arg.Formatted())
            )
            .ToList();

    private static TestCase BuildTestCase(TestCaseDescriptor descriptor, string assemblyPath, CodeNavigation navData)
    {
        TestCase testCase = new(descriptor.FullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), assemblyPath)
        {
            Id = GenerateTestId(descriptor, assemblyPath, navData),
            DisplayName = descriptor.DisplayName,
            FullyQualifiedName = descriptor.FullyQualifiedName,
            CodeFilePath = navData.Source,
            LineNumber = navData.Line
        };
        testCase.SetPropertyValue(TestCaseNameProperty, descriptor.FullyQualifiedName);
        testCase.SetPropertyValue(ManagedTypeProperty, descriptor.ManagedType);
        testCase.SetPropertyValue(ManagedMethodProperty, descriptor.ManagedMethod);
        if (descriptor.HierarchyValues?.Count > 0)
            testCase.SetPropertyValue(HierarchyProperty, descriptor.HierarchyValues?.ToArray());
        return testCase;
    }

    private static Guid GenerateTestId(TestCaseDescriptor descriptor, string assemblyPath, CodeNavigation navData)
    {
        var idProvider = new TestIdProvider();
        idProvider.AppendString(assemblyPath);
        idProvider.AppendString(descriptor.FullyQualifiedName);
        idProvider.AppendString(GdUnit4TestExecutor.ExecutorUri);
        idProvider.AppendString(navData.Source ?? "");
        return idProvider.GetId();
    }

    private static string BuildDisplayName(string managedTypeName, string testName, long index, TestCaseAttribute attr, GdUnit4Settings gdUnitSettings)
        => gdUnitSettings.DisplayName switch
        {
            DisplayNameOptions.SimpleName => BuildSimpleDisplayName(testName, index, attr),
            DisplayNameOptions.FullyQualifiedName => Core.Execution.TestCase.BuildFullyQualifiedName(managedTypeName, testName, attr),
            _ => testName
        };

    private static string BuildSimpleDisplayName(string testName, long index, TestCaseAttribute attribute)
    {
        var displayName = attribute.TestName ?? testName;
        if (index == -1 || attribute.Arguments.Length == 0)
            return displayName;
        return $"{displayName}'{index}";
    }

    private static IEnumerable<string> FilterWithoutTestAdapter(IEnumerable<string> assemblyPaths) =>
        assemblyPaths.Where(assembly =>
            !assembly.Contains(".TestAdapter.")
            && !assembly.Contains("Microsoft.VisualStudio.TestPlatform")
            && !assembly.Contains("MSTest.TestAdapter"));

    private static bool IsTestSuite(Type type) =>
        type is { IsClass: true, IsAbstract: false }
        && Attribute.IsDefined(type, typeof(TestSuiteAttribute))
        && type.FullName != null
        && !type.FullName.StartsWith("Microsoft.VisualStudio.TestTools");
}
