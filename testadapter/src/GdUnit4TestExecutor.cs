using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;


using GdUnit4.TestAdapter.Discovery;
using GdUnit4.TestAdapter.Settings;

namespace GdUnit4.TestAdapter;

[ExtensionUri(ExecutorUri)]
public class GdUnit4TestExecutor : ITestExecutor
{
    ///<summary>
    /// The Uri used to identify the GdUnit4 Executor
    ///</summary>
    public const string ExecutorUri = "executor://GdUnit4.TestAdapter";

    private Execution.ITestExecutor? _executor;

    private IFrameworkHandle? _frameworkHandle;

    // Test properties supported for filtering
    private Dictionary<string, TestProperty> SupportedProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        ["FullyQualifiedName"] = TestCaseProperties.FullyQualifiedName,
        ["Name"] = TestCaseProperties.DisplayName,
    };

    /// <summary>
    /// Runs only the tests specified by parameter 'tests'. 
    /// </summary>
    /// <param name="tests">Tests to be run.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<TestCase>? tests, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        _ = tests ?? throw new ArgumentNullException(nameof(tests), "Argument 'tests' is null, abort!");
        _ = runContext ?? throw new ArgumentNullException(nameof(runContext), "Argument 'runContext' is null, abort!");
        _ = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle), "Argument 'frameworkHandle' is null, abort!");

        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        var runSettings = XmlRunSettingsUtilities.GetTestRunParameters(runContext.RunSettings?.SettingsXml);
        var gdUnitSettings = runContext.RunSettings?.GetSettings(GdUnit4Settings.RunSettingsXmlNode) as GdUnit4SettingsProvider;
        var filterExpression = runContext.GetTestCaseFilter(SupportedProperties.Keys, (propertyName) =>
        {
            SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
            return testProperty;
        });

        _frameworkHandle = frameworkHandle;

        _executor = new Execution.TestExecutor(runConfiguration, gdUnitSettings?.Settings ?? new GdUnit4Settings());
        _executor.Run(frameworkHandle, runContext, tests);
    }

    /// <summary>
    /// Runs 'all' the tests present in the specified 'containers'. 
    /// </summary>
    /// <param name="containers">Path to test container files to look for tests in.</param>
    /// <param name="runContext">Context to use when executing the tests.</param>
    /// <param param name="frameworkHandle">Handle to the framework to record results and to do framework operations.</param>
    public void RunTests(IEnumerable<string>? containers, IRunContext? runContext, IFrameworkHandle? frameworkHandle)
    {
        _ = containers ?? throw new ArgumentNullException(nameof(containers), "Argument 'containers' is null, abort!");
        _ = runContext ?? throw new ArgumentNullException(nameof(runContext), "Argument 'runContext' is null, abort!");
        _ = frameworkHandle ?? throw new ArgumentNullException(nameof(frameworkHandle), "Argument 'frameworkHandle' is null, abort!");

        TestCaseDiscoverySink discoverySink = new();
        new GdUnit4TestDiscoverer().DiscoverTests(containers, runContext, frameworkHandle, discoverySink);
        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        var gdUnitSettings = runContext.RunSettings?.GetSettings(GdUnit4Settings.RunSettingsXmlNode) as GdUnit4SettingsProvider;

        _frameworkHandle = frameworkHandle;
        _executor = new Execution.TestExecutor(runConfiguration, gdUnitSettings?.Settings ?? new GdUnit4Settings());
        _executor.Run(frameworkHandle, runContext, discoverySink.TestCases);
    }

    /// <summary>
    /// Cancel the execution of the tests.
    /// </summary>
    public void Cancel()
    {
        _frameworkHandle?.SendMessage(TestMessageLevel.Informational, "Cancel pressed  -----");
        _executor?.Cancel();
    }

}
