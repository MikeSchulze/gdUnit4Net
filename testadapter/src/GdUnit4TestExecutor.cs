namespace GdUnit4.TestAdapter;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Api;

using Discovery;

using Execution;

using Extensions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using Settings;

using Utilities;

[ExtensionUri(ExecutorUri)]
// ReSharper disable once ClassNeverInstantiated.Global
public class GdUnit4TestExecutor : ITestExecutor2, IDisposable
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

    private IFrameworkHandle? fh;
    private ITestEngine? testEngine;

#pragma warning disable CA1859
    private ITestEngineLogger? Log { get; set; }
#pragma warning restore CA1859

    public void Dispose() => GC.SuppressFinalize(this);

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

        Log = new Logger(frameworkHandle);
        if (ITestEngine.EngineVersion() < GdUnit4TestDiscoverer.MinRequiredEngineVersion)
        {
            Log.LogError(
                $"Wrong gdUnit4Api, Version={ITestEngine.EngineVersion()} found, you need to upgrade to minimum version: '{GdUnit4TestDiscoverer.MinRequiredEngineVersion}'");
            Log.LogError("Abort the test discovery.");
            return;
        }

        var runConfiguration = XmlRunSettingsUtilities.GetRunConfigurationNode(runContext.RunSettings?.SettingsXml);
        var settings = GdUnit4SettingsProvider.LoadSettings(runContext);
        var engineSettings = new TestEngineSettings
        {
            CaptureStdOut = settings.CaptureStdOut,
            Parameters = settings.Parameters,
            MaxCpuCount = Math.Max(1, runConfiguration.MaxCpuCount),
            SessionTimeout = (int)(runConfiguration.TestSessionTimeout == 0
                ? 30000
                : runConfiguration.TestSessionTimeout)
        };


        testEngine = ITestEngine.GetInstance(engineSettings, Log);
        Log.LogInfo($"Running on GdUnit4 test engine version: {ITestEngine.EngineVersion()}");

        //var runSettings = XmlRunSettingsUtilities.GetTestRunParameters(runContext.RunSettings?.SettingsXml);
        // ReSharper disable once UnusedVariable
        var filterExpression = runContext.GetTestCaseFilter(supportedProperties.Keys, propertyName =>
        {
            supportedProperties.TryGetValue(propertyName, out var testProperty);
            return testProperty;
        });

        try
        {
            SetupRunnerEnvironment(runContext, frameworkHandle);
            var testsByAssembly = ToGdUnitTestNodes(testCases);
            using var testEventListener = new TestEventReportServer(frameworkHandle, testCases);
            var debuggerFramework = GetDebuggerFramework(frameworkHandle);
            testEngine.Execute(testsByAssembly, testEventListener, debuggerFramework);
            Log.LogInfo("Test execution stopped.");

            // enable just to verify all allocated objects are freed
            /*
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            */
        }
        catch (Exception ex)
        {
            Log.LogError($"Test execution failed: {ex.Message}");
            Log.LogError(ex.StackTrace ?? "No stack trace available");
            throw;
        }
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
        RunTests(discoverySink.TestCases, runContext, frameworkHandle);
    }

    /// <summary>
    ///     Cancel the execution of the tests.
    /// </summary>
    public void Cancel()
    {
        Log?.LogInfo("Cancel pressed  -----");
        testEngine?.Cancel();
    }

    public bool ShouldAttachToTestHost(IEnumerable<string>? sources, IRunContext runContext) => true;

    public bool ShouldAttachToTestHost(IEnumerable<TestCase>? tests, IRunContext runContext) => true;

    private static IDebuggerFramework GetDebuggerFramework(IFrameworkHandle frameworkHandle)
        => IsJetBrainsRider(frameworkHandle)
            ? new RiderDebuggerFramework(frameworkHandle)
            : new DefaultDebuggerFramework(frameworkHandle);

    private static List<TestAssemblyNode> ToGdUnitTestNodes(IEnumerable<TestCase> testCases) =>
        // Group test cases by assembly path
        testCases
            .GroupBy(tc => tc.Source)
            .Select(assemblyGroup =>
            {
                // Create the assembly node
                var assembly = new TestAssemblyNode
                {
                    Id = Guid.NewGuid(),
                    ParentId = Guid.Empty,
                    AssemblyPath = assemblyGroup.Key,
                    Suites = new List<TestSuiteNode>()
                };

                // Group test cases by managed type (suites)
                var suites = assemblyGroup
                    .GroupBy(t => t.CodeFilePath)
                    .Select(tests =>
                    {
                        var t = tests.First();

                        var ManagedType = t.GetPropertyValue<string>(TestCaseExtensions.ManagedTypeProperty, "");
                        var suite = new TestSuiteNode
                        {
                            Id = Guid.NewGuid(),
                            ParentId = assembly.Id,
                            ManagedType = ManagedType,
                            AssemblyPath = assembly.AssemblyPath,
                            Tests = new List<TestCaseNode>()
                        };

                        suite.Tests.AddRange(tests
                            .Select(test => new TestCaseNode
                            {
                                Id = test.Id,
                                ParentId = suite.Id,
                                ManagedMethod = test.GetPropertyValue<string>(TestCaseExtensions.ManagedMethodProperty, ""),
                                AttributeIndex = test.GetPropertyValue(TestCaseExtensions.ManagedMethodAttributeIndexProperty, 0),
                                LineNumber = test.LineNumber,
                                RequireRunningGodotEngine = test.GetPropertyValue(TestCaseExtensions.RequireRunningGodotEngineProperty, false)
                            }).ToList());

                        return suite;
                    });

                assembly.Suites.AddRange(suites);
                return assembly;
            })
            .ToList();

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

    private static bool IsJetBrainsRider(IFrameworkHandle frameworkHandle)
    {
        var version = frameworkHandle.GetType().Assembly.GetName().Version;
        if (frameworkHandle is not IFrameworkHandle2
            || !frameworkHandle.GetType().ToString().Contains("JetBrains")
            || version < new Version("2.16.1.14"))
            return false;

        frameworkHandle.SendMessage(TestMessageLevel.Informational, $"JetBrains Rider detected {version}");
        return true;
    }
}
