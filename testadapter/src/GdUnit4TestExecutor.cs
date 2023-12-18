using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using GdUnit4.TestAdapter.Execution;
using ITestExecutor = GdUnit4.TestAdapter.Execution.ITestExecutor;

namespace GdUnit4.TestAdapter;

[ExtensionUri(ExecutorUri)]
public class GdUnit4TestExecutor : Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter.ITestExecutor
{
    ///<summary>
    /// The Uri used to identify the NUnitExecutor
    ///</summary>
    public const string ExecutorUri = "executor://GdUnit4.TestAdapter/v1";

    private ITestExecutor? _executor;

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
        var settings = XmlRunSettingsUtilities.GetTestRunParameters(runContext.RunSettings?.SettingsXml);
        foreach (var key in settings.Keys)
        {
            frameworkHandle.SendMessage(TestMessageLevel.Informational, $"{key} = '{settings[key]}'");
        }

        var filterExpression = runContext.GetTestCaseFilter(SupportedProperties.Keys, (propertyName) =>
        {
            SupportedProperties.TryGetValue(propertyName, out TestProperty? testProperty);
            return testProperty;
        });

        _executor = new TestExecutor(runConfiguration);
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
        frameworkHandle?.SendMessage(TestMessageLevel.Warning, $"RunTests:containers ${containers} NOT implemented!");
    }

    /// <summary>
    /// Cancel the execution of the tests.
    /// </summary>
    public void Cancel()
    {
        _executor?.Cancel();
    }

}
