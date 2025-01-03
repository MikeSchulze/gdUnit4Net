namespace GdUnit4.Core;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Discovery;

using Events;

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

    public void Dispose() => cancellationSource?.Dispose();

    public void Cancel()
    {
        lock (taskLock) cancellationSource?.Cancel();
    }

    public List<TestCaseDescriptor> Discover(string testAssembly) => TestCaseDiscoverer.Discover(Settings, Logger, testAssembly);

    public void Execute(List<TestAssemblyNode> testAssemblyNodes, ITestEventListener eventListener)
    {
        lock (taskLock) cancellationSource = new CancellationTokenSource();

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(Settings.MaxCpuCount);

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
            Logger.LogInfo("Test execution cancelled");
            // Cancel any remaining tasks
            cancellationSource.Cancel();
            try
            {
                // Wait for tasks to complete cancellation
                Task.WaitAll(tasks.ToArray(), TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during cancellation cleanup: {ex.Message}");
            }

            throw;
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

    private Task ExecuteTestsInAssembly(TestAssemblyNode testAssemblyNode, ITestEventListener eventListener, CancellationToken cancellationToken)
        => Task.Run(() =>
        {
            Logger.LogInfo($"Starting tests for assembly: {testAssemblyNode.AssemblyPath}");

            ExecuteEngineTests(testAssemblyNode.Suites, eventListener, cancellationToken);

            Logger.LogInfo($"Completed tests for assembly: {testAssemblyNode.AssemblyPath}");
        }, cancellationToken);

    private void ExecuteEngineTests(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        var (directExecutorTestSuites, godotExecutorTestSuites) = SplitTestSuitesByRequiredRuntime(testSuiteNodes);

        // Run tests that don't require Godot runtime
        if (directExecutorTestSuites.Count > 0)
        {
            var directRunner = new DirectTestRunner(Logger);
            directRunner.RunAndWait(directExecutorTestSuites, eventListener, cancellationToken);
        }

        // Run tests that require Godot runtime
        if (godotExecutorTestSuites.Count > 0)
        {
            var godotRunner = new GodotProcessTestRunner(Logger);
            godotRunner.RunAndWait(godotExecutorTestSuites, eventListener, cancellationToken);
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
}
