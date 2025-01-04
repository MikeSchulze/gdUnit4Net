namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Discovery;

using Microsoft.TestPlatform.AdapterUtilities;
using Microsoft.TestPlatform.AdapterUtilities.ManagedNameUtilities;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

using Settings;

using Utilities;

using static Settings.GdUnit4Settings;

using static Extensions.TestCaseExtensions;

using TestCaseDescriptor = Core.Discovery.TestCaseDescriptor;

[DefaultExecutorUri(GdUnit4TestExecutor.ExecutorUri)]
[ExtensionUri(GdUnit4TestExecutor.ExecutorUri)]
[FileExtension(".dll")]
[FileExtension(".exe")]
[Category("managed")]
public sealed class GdUnit4TestDiscoverer : ITestDiscoverer
{
#pragma warning disable CA1859
    private ITestEngineLogger Log { get; set; } = null!;
#pragma warning restore CA1859
    internal static Version MinRequiredEngineVersion { get; } = new("4.4.0");


    public void DiscoverTests(
        IEnumerable<string> sources,
        IDiscoveryContext discoveryContext,
        IMessageLogger logger,
        ITestCaseDiscoverySink discoverySink)
    {
        try
        {
            Log = new Logger(logger);
            if (ITestEngine.EngineVersion() < MinRequiredEngineVersion)
            {
                Log.LogError($"Wrong gdUnit4Api, Version={ITestEngine.EngineVersion()} found, you need to upgrade to minimum version: '{MinRequiredEngineVersion}'");
                Log.LogError("Abort the test discovery.");
                return;
            }

            var settings = GdUnit4SettingsProvider.LoadSettings(discoveryContext);
            var engineSettings = new TestEngineSettings
            {
                CaptureStdOut = settings.CaptureStdOut,
                Parameters = settings.Parameters
            };
            var testEngine = ITestEngine.GetInstance(engineSettings, Log);
            Log.LogInfo($"Running on GdUnit4 test engine version: {ITestEngine.EngineVersion()}");

            var filteredAssembles = FilterWithoutTestAdapter(sources);

            foreach (var assemblyPath in filteredAssembles)
                try
                {
                    using var codeNavigationProvider = new CodeNavigationDataProvider(assemblyPath);

                    testEngine.Discover(assemblyPath)
                        .ConvertAll(testCase => BuildTestCase(testCase, codeNavigationProvider, settings))
                        .OrderBy(t => t.FullyQualifiedName)
                        .ToList()
                        .ForEach(discoverySink.SendTestCase);
                }
                catch (Exception e)
                {
                    Log.LogError($"Error discovering tests in {assemblyPath}: {e}");
                }
        }
        catch (Exception e)
        {
            logger.SendMessage(TestMessageLevel.Error, $"Error loading test settings: {e}");
        }
    }

    private TestCase BuildTestCase(TestCaseDescriptor descriptor, CodeNavigationDataProvider navDataProvider, GdUnit4Settings settings)
    {
        var navData = navDataProvider.GetNavigationData(descriptor.ManagedType, descriptor.ManagedMethod);
        if (!navData.IsValid)
            Log.LogInfo(
                $"Can't collect code navigation data for {descriptor.ManagedType}:{descriptor.ManagedMethod}    GetNavigationData -> {navData.Source}:{navData.Line}");

        TestCase testCase = new(descriptor.FullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), descriptor.AssemblyPath)
        {
            Id = descriptor.Id,
            DisplayName = GetDisplayName(descriptor, settings),
            CodeFilePath = navData.Source,
            LineNumber = navData.Line
        };
        testCase.SetPropertyValue(TestCaseNameProperty, descriptor.FullyQualifiedName);
        testCase.SetPropertyValue(ManagedTypeProperty, descriptor.ManagedType);
        testCase.SetPropertyValue(ManagedMethodProperty, descriptor.ManagedMethod);
        testCase.SetPropertyValue(ManagedMethodAttributeIndexProperty, descriptor.AttributeIndex);
        testCase.SetPropertyValue(RequireRunningGodotEngineProperty, descriptor.RequireRunningGodotEngine);

        ManagedNameHelper.GetManagedName(navData.Method, out _, out _, out var hierarchyValues);
        ManagedNameParser.ParseManagedMethodName(descriptor.ManagedMethod, out var methodName, out _, out _);
        if (hierarchyValues.Length > 0)
        {
            hierarchyValues[HierarchyConstants.Levels.ContainerIndex] = null;
            hierarchyValues[HierarchyConstants.Levels.TestGroupIndex] = descriptor.ManagedMethod;
            testCase.SetPropertyValue(HierarchyProperty, hierarchyValues);
        }

        return testCase;
    }

    private static string GetDisplayName(TestCaseDescriptor input, GdUnit4Settings gdUnitSettings)
        => gdUnitSettings.DisplayName switch
        {
            DisplayNameOptions.SimpleName => input.SimpleName,
            DisplayNameOptions.FullyQualifiedName => input.FullyQualifiedName.Substring(input.FullyQualifiedName.LastIndexOf('.') + 1),
            _ => input.ManagedMethod
        };

    private static IEnumerable<string> FilterWithoutTestAdapter(IEnumerable<string> assemblyPaths) =>
        assemblyPaths.Where(assembly => !assembly.Contains("MSTest") && !assembly.Contains("Microsoft"));
}
