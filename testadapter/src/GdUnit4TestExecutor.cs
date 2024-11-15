namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Discovery;

using Execution;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using Settings;

using static Utilities.Utils;

using ITestExecutor = Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter.ITestExecutor;

[ExtensionUri(ExecutorUri)]
// ReSharper disable once ClassNeverInstantiated.Global
public class GdUnit4TestExecutor : ITestExecutor, IDisposable
{
    /// <summary>
    ///     The Uri used to identify the GdUnit4 Executor
    /// </summary>
    public const string ExecutorUri = "executor://GdUnit4.TestAdapter";

    // Test properties supported for filtering
    private readonly Dictionary<string, TestProperty> supportedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FullyQualifiedName"] = TestCaseProperties.FullyQualifiedName,
        ["Name"] = TestCaseProperties.DisplayName
    };

    private TestExecutor? executor;

    private IFrameworkHandle? fh;

    public void Dispose()
    {
        executor?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Runs only the tests specified by parameter 'tests'.
    /// </summary>
    /// <param name="tests">Tests to be run.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        _ = tests ?? throw new ArgumentNullException(nameof(tests), "Argument 'tests' is null, abort!");
        _ = runContext ?? throw new ArgumentNullException(nameof(runContext), "Argument 'runContext' is null, abort!");
        fh = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle), "Argument 'frameworkHandle' is null, abort!");

        var testCases = tests.ToList();
        if (testCases.Count == 0)
            return;

        if (!CheckGdUnit4ApiMinimumRequiredVersion(fh, new Version("4.4.0")))
        {
            fh.SendMessage(TestMessageLevel.Error, "Abort the test execution.");
            return;
        }

        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        //var runSettings = XmlRunSettingsUtilities.GetTestRunParameters(runContext.RunSettings?.SettingsXml);
        var gdUnitSettings = runContext.RunSettings?.GetSettings(GdUnit4Settings.RunSettingsXmlNode) as GdUnit4SettingsProvider;
        // ReSharper disable once UnusedVariable
        var filterExpression = runContext.GetTestCaseFilter(supportedProperties.Keys, propertyName =>
        {
            supportedProperties.TryGetValue(propertyName, out var testProperty);
            return testProperty;
        });

        SetupRunnerEnvironment(runContext, frameworkHandle);
        executor = new TestExecutor(runConfiguration, gdUnitSettings?.Settings ?? new GdUnit4Settings());
        executor.Run(fh, runContext, testCases);
    }

    /// <summary>
    ///     Runs 'all' the tests present in the specified 'containers'.
    /// </summary>
    /// <param name="sources">Path to test container files to look for tests in.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<string>? sources, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        _ = sources ?? throw new ArgumentNullException(nameof(sources), "Argument 'containers' is null, abort!");
        _ = runContext ?? throw new ArgumentNullException(nameof(runContext), "Argument 'runContext' is null, abort!");
        fh = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle), "Argument 'frameworkHandle' is null, abort!");

        TestCaseDiscoverySink discoverySink = new();
        new GdUnit4TestDiscoverer().DiscoverTests(sources, runContext, fh, discoverySink);
        if (discoverySink.TestCases.Count == 0) return;

        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        var gdUnitSettings = runContext.RunSettings?.GetSettings(GdUnit4Settings.RunSettingsXmlNode) as GdUnit4SettingsProvider;

        SetupRunnerEnvironment(runContext, frameworkHandle);
        executor = new TestExecutor(runConfiguration, gdUnitSettings?.Settings ?? new GdUnit4Settings());
        executor.Run(fh, runContext, discoverySink.TestCases);
    }

    /// <summary>
    ///     Cancel the execution of the tests.
    /// </summary>
    public void Cancel()
    {
        fh?.SendMessage(TestMessageLevel.Informational, "Cancel pressed  -----");
        executor?.Cancel();
    }

    internal static void SetupRunnerEnvironment(IRunContext runContext, IFrameworkHandle frameworkHandle)
    {
        try
        {
            foreach (var variable in RunSettingsProvider.GetEnvironmentVariables(runContext.RunSettings?.SettingsXml))
                Environment.SetEnvironmentVariable(variable.Key, variable.Value);
        }
        catch (XmlException ex)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Error, "Error while setting environment variables: " + ex.Message);
        }
    }
}
