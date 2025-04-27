namespace GdUnit4.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Discovery;

using Extensions;

using Runners;

internal sealed class GdUnit4TestEngine : ITestEngine
{
    private readonly object taskLock = new();
    private CancellationTokenSource? cancellationSource;

    public GdUnit4TestEngine(TestEngineSettings settings, ITestEngineLogger logger)
    {
        Settings = settings;
        Logger = logger;
    }

    private TestEngineSettings Settings { get; }
    private ITestEngineLogger Logger { get; }
    private IDebuggerFramework DebuggerFramework { get; set; } = null!;
    private List<ITestRunner> ActiveTestRunners { get; } = new();

    public void Dispose() => cancellationSource?.Dispose();

    public void Cancel()
    {
        lock (taskLock) cancellationSource?.Cancel();
        foreach (var activeTestRunner in ActiveTestRunners) activeTestRunner.Cancel();
    }

    public List<TestCaseDescriptor> Discover(string testAssembly) => TestCaseDiscoverer.Discover(Settings, Logger, testAssembly);

    public void Execute(List<TestAssemblyNode> testAssemblyNodes, ITestEventListener eventListener, IDebuggerFramework debuggerFramework)
    {
        DebuggerFramework = debuggerFramework;
        var sessionTimeoutCancellationSource = new CancellationTokenSource(Settings.SessionTimeout);
        lock (taskLock)
            cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(sessionTimeoutCancellationSource.Token);

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(Settings.MaxCpuCount);
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            foreach (var assemblyNode in testAssemblyNodes)
            {
                semaphore.Wait(cancellationSource.Token);

                var task = ExecuteTestsInAssembly(assemblyNode, eventListener, cancellationSource.Token)
                    // ReSharper disable once AccessToDisposedClosure
                    .ContinueWith(_ => semaphore.Release(), TaskContinuationOptions.ExecuteSynchronously);
                tasks.Add(task);
            }

            Task.WaitAll(tasks.ToArray(), cancellationSource.Token);
        }
        catch (OperationCanceledException)
        {
            // is running into session timeout we need to manually cancel the current test run
            if (sessionTimeoutCancellationSource.IsCancellationRequested)
            {
                stopwatch.Stop();
                Logger.LogInfo($"Test execution is stopped because of running into session timeout of {TimeSpan.FromMilliseconds(Settings.SessionTimeout)}.!");
                Logger.LogInfo($"""

                                ╔═══════════════════════ TEST SESSION TIMEOUT ═══════════════════════════════════════╗

                                  Test execution exceeded maximum allowed time:
                                    • Timeout: {TimeSpan.FromMilliseconds(Settings.SessionTimeout).Humanize()}
                                    • Total tests: {TotalTests(testAssemblyNodes)}
                                    • Completed tests: {eventListener.CompletedTests}
                                    • Time elapsed: {stopwatch.Elapsed.Humanize()}

                                  ACTION REQUIRED: Please increase 'TestSessionTimeout' in your '.runsettings' file

                                ╚════════════════════════════════════════════════════════════════════════════════════╝
                                """);
                Cancel();
            }

            try
            {
                // Wait for tasks to complete cancellation
                Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during cancellation cleanup: {ex.Message}");
            }
        }
        catch (AggregateException ae)
        {
            foreach (var ex in ae.InnerExceptions) Logger.LogError($"Error executing tests: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error executing tests: {ex.Message}");
            throw;
        }
        finally
        {
            semaphore.Dispose();
            lock (taskLock)
            {
                cancellationSource.Dispose();
                cancellationSource = null;
            }
        }
    }

    private static int TotalTests(List<TestAssemblyNode> testAssemblyNodes)
    {
        var totalTests = 0;
        foreach (var assemblyNode in testAssemblyNodes) totalTests += assemblyNode.Suites.Sum(ts => ts.Tests.Count);
        return totalTests;
    }

    private Task ExecuteTestsInAssembly(TestAssemblyNode testAssemblyNode, ITestEventListener eventListener, CancellationToken cancellationToken)
        => Task.Run(() =>
        {
            Logger.LogInfo($"Starting tests for assembly: {testAssemblyNode.AssemblyPath}");

            var projectWorkingDir = LookupProjectPath(testAssemblyNode.AssemblyPath);
            Directory.SetCurrentDirectory(projectWorkingDir);
            Logger.LogInfo($"Set current working directory to: {projectWorkingDir}");

            ExecuteEngineTests(testAssemblyNode.Suites, eventListener, cancellationToken);

            Logger.LogInfo($"Completed tests for assembly: {testAssemblyNode.AssemblyPath}");
        }, cancellationToken);

    private void ExecuteEngineTests(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        var (directExecutorTestSuites, godotExecutorTestSuites) = SplitTestSuitesByRequiredRuntime(testSuiteNodes);
        // Run tests that require Godot runtime
        if (godotExecutorTestSuites.Count > 0)
        {
            var godotRunner = new GodotRuntimeTestRunner(Logger, DebuggerFramework, Settings);
            ActiveTestRunners.Add(godotRunner);
            godotRunner.RunAndWait(godotExecutorTestSuites, eventListener, cancellationToken);
            ActiveTestRunners.Remove(godotRunner);
        }
        // Run tests that don't require Godot runtime
        if (directExecutorTestSuites.Count > 0)
        {
            var directRunner = new DefaultTestRunner(Logger, Settings);
            ActiveTestRunners.Add(directRunner);
            directRunner.RunAndWait(directExecutorTestSuites, eventListener, cancellationToken);
            ActiveTestRunners.Remove(directRunner);
        }
    }

    private static (List<TestSuiteNode> DirectExecutorTestSuites, List<TestSuiteNode> GodotExecutorTestSuites) SplitTestSuitesByRequiredRuntime(List<TestSuiteNode> testSuiteNodes)
    {
        var directExecutorTestSuites = new List<TestSuiteNode>();
        var godotExecutorTestSuites = new List<TestSuiteNode>();

        foreach (var suite in testSuiteNodes)
        {
            var godotTests = suite.Tests.FindAll(test => test.RequireRunningGodotEngine);
            var directTests = suite.Tests.FindAll(test => !test.RequireRunningGodotEngine);

            if (godotTests.Count > 0)
                godotExecutorTestSuites.Add(suite with { Tests = godotTests });
            if (directTests.Count > 0)
                directExecutorTestSuites.Add(suite with { Tests = directTests });
        }

        return (directExecutorTestSuites, godotExecutorTestSuites);
    }


    private string LookupProjectPath(string assemblyPath)
    {
        try
        {
            Logger.LogInfo($"Search '.csproj' at {assemblyPath}");
            var currentDir = new DirectoryInfo(assemblyPath).Parent;
            while (currentDir != null)
            {
                if (currentDir.EnumerateFiles("*.csproj").Any())
                    return currentDir.FullName;
                currentDir = currentDir.Parent;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Unable to locate .csproj file: {ex.Message}");
        }

        throw new FileNotFoundException("Project file does not exist");
    }
}
