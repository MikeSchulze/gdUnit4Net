namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Api;

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
    private ITestEngineLogger Logger { get; set; } = null!;
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
            Logger = new Logger(logger);
            if (ITestEngine.EngineVersion() < MinRequiredEngineVersion)
            {
                Logger.LogError($"Wrong GdUnit4Api, Version={ITestEngine.EngineVersion()} found, you need to upgrade to minimum version: '{MinRequiredEngineVersion}'");
                Logger.LogError("Abort the test discovery.");
                return;
            }

            var settings = GdUnit4SettingsProvider.LoadSettings(discoveryContext);
            var engineSettings = new TestEngineSettings
            {
                CaptureStdOut = settings.CaptureStdOut,
                Parameters = settings.Parameters
            };
            var testEngine = ITestEngine.GetInstance(engineSettings, Logger);
            Logger.LogInfo($"Running on GdUnit4 test engine version: {ITestEngine.EngineVersion()}");

            var filteredAssembles = FilterWithoutTestAdapter(sources);

            foreach (var assemblyPath in filteredAssembles)
                try
                {
                    testEngine.Discover(assemblyPath)
                        .ConvertAll(testCase => BuildTestCase(testCase, settings))
                        .OrderBy(t => t.FullyQualifiedName)
                        .ToList()
                        .ForEach(discoverySink.SendTestCase);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error discovering tests for assembly {assemblyPath}: {e}");
                }
        }
        catch (Exception e)
        {
            logger.SendMessage(TestMessageLevel.Error, $"Error loading test settings: {e}");
        }
    }

    internal static TestCase BuildTestCase(TestCaseDescriptor descriptor, GdUnit4Settings settings)
    {
        TestCase testCase = new(descriptor.FullyQualifiedName, new Uri(GdUnit4TestExecutor.ExecutorUri), descriptor.AssemblyPath)
        {
            Id = descriptor.Id,
            DisplayName = GetDisplayName(descriptor, settings),
            CodeFilePath = descriptor.CodeFilePath,
            LineNumber = descriptor.LineNumber
        };

        testCase.SetPropertyValues(descriptor);

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
