namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Discovery;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.TestPlatform.AdapterUtilities.ManagedNameUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

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

        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(discoveryContext.RunSettings?.SettingsXml);
        var gdUnitSettingsProvider = discoveryContext.RunSettings?.GetSettings(RunSettingsXmlNode) as GdUnit4SettingsProvider;
        var gdUnitSettings = gdUnitSettingsProvider?.Settings ?? new GdUnit4Settings();
        var filteredAssembles = FilterWithoutTestAdapter(sources);

        logger.SendMessage(TestMessageLevel.Informational, $"GdUnit4Settings: {gdUnitSettings.DisplayName}");

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
                            logger.SendMessage(TestMessageLevel.Informational,
                                $"Can't collect code navigation data for {className}:{mi.Name}    GetNavigationData -> {navData.Source}:{navData.Line}");

                        ManagedNameHelper.GetManagedName(mi, out var managedType, out var managedMethod, out var hierarchyValues);
                        ManagedNameParser.ParseManagedMethodName(managedMethod, out var methodName, out var parameterCount, out var parameterTypes);
                        hierarchyValues[HierarchyConstants.Levels.ContainerIndex] = null;
                        hierarchyValues[HierarchyConstants.Levels.TestGroupIndex] = methodName;
                        var isAsync = MatchReturnType(mi, typeof(Task));
                        // Collect test cases or build a single test
                        mi.GetCustomAttributes(typeof(TestCaseAttribute))
                            .Cast<TestCaseAttribute>()
                            .Select((attr, index) => new TestCaseDescriptor
                            {
                                ManagedType = managedType,
                                ManagedMethod = managedMethod,
                                HierarchyValues = new ReadOnlyCollection<string?>(hierarchyValues),
                                DisplayName = BuildDisplayName(managedType, mi.Name, index, attr, gdUnitSettings),
                                FullyQualifiedName = Executions.TestCase.BuildFullyQualifiedName(managedType, mi.Name, attr),
                                Traits = TestCasePropertiesAsTraits(mi)
                            })
                            .Select(testDescriptor => BuildTestCase(testDescriptor, assemblyPath, navData))
                            .OrderBy(t => t.DisplayName)
                            .ToList()
                            .ForEach(t =>
                            {
                                Interlocked.Increment(ref testsTotalDiscovered);
                                Interlocked.Increment(ref testCasesDiscovered);
                                discoverySink.SendTestCase(t);
                            });
                    });
                logger.SendMessage(TestMessageLevel.Informational, $"Discover:  TestSuite {className} with {testCasesDiscovered} TestCases found.");
            }

            logger.SendMessage(TestMessageLevel.Informational, $"Discover tests done, {testSuiteDiscovered} TestSuites and total {testsTotalDiscovered} Tests found.");
        }
    }

    internal static bool MatchReturnType(MethodInfo method, Type returnType)
        => method == null
            ? throw new ArgumentNullException(nameof(method))
            : returnType == null
                ? throw new ArgumentNullException(nameof(returnType))
                : method.ReturnType.Equals(returnType);


    private List<Trait> TestCasePropertiesAsTraits(MethodInfo mi)
        => mi.GetCustomAttributes(typeof(TestCaseAttribute))
            .Cast<TestCaseAttribute>()
            .Where(attr => attr.Arguments?.Length != 0)
            .Select(attr => attr.Name == null
                ? new Trait(string.Empty, attr.Arguments.Formatted())
                : new Trait(attr.Name, attr.Arguments.Formatted())
            )
            .ToList();

    private TestCase BuildTestCase(TestCaseDescriptor descriptor, string assemblyPath, CodeNavigation navData)
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
            DisplayNameOptions.FullyQualifiedName => Executions.TestCase.BuildFullyQualifiedName(managedTypeName, testName, attr),
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
        assemblyPaths.Where(assembly => !assembly.Contains(".TestAdapter."));

    private static bool IsTestSuite(Type type) =>
        type is { IsClass: true, IsAbstract: false } && Attribute.IsDefined(type, typeof(TestSuiteAttribute));
}
