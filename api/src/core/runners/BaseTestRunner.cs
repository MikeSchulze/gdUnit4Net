namespace GdUnit4.Core.Runners;

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Events;

using Newtonsoft.Json;

public class BaseTestRunner : ITestRunner
{
    protected BaseTestRunner(ICommandExecutor executor, ITestEngineLogger logger, TestEngineSettings settings)
    {
        Executor = executor;
        Logger = logger;
        Settings = settings;
    }

    private object SyncLock { get; } = new();
    private ICommandExecutor Executor { get; }
    private TestEngineSettings Settings { get; }
    private CancellationTokenSource? RunnerCancellationToken { get; set; }
    protected ITestEngineLogger Logger { get; }

    public async ValueTask DisposeAsync()
    {
        await Executor.DisposeAsync();
        RunnerCancellationToken?.Dispose();
        GC.SuppressFinalize(this);
    }

    public virtual void Cancel()
    {
        Logger.LogInfo("Try cancelling the test run...");
        lock (SyncLock) RunnerCancellationToken?.Cancel();
    }

    public void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        lock (SyncLock) RunnerCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var token = RunnerCancellationToken.Token;
        Task.Run(async () =>
            {
                try
                {
                    await Executor.StartAsync();
                    foreach (var testSuite in testSuiteNodes)
                    {
                        //using (var stdoutHook = testSuiteContext.IsCaptureStdOut ? StdOutHookFactory.CreateStdOutHook() : null)
                        var response = await Executor.ExecuteCommand(new ExecuteTestSuiteCommand(testSuite, Settings.CaptureStdOut, true), eventListener, token);
                        ValidateResponse(response);
                    }
                }
                catch (TimeoutException)
                {
                    Logger.LogError("Failed to connect: Connection timeout");
                }
                catch (OperationCanceledException)
                {
                    //Logger.LogInfo("Running tests are cancelled.");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                }
            }, token)
            .ContinueWith(_ =>
            {
                lock (SyncLock)
                {
                    RunnerCancellationToken?.Dispose();
                    RunnerCancellationToken = null;
                }
            }, token)
            .Wait(token);
    }

    private static void ValidateResponse(Response response)
    {
        if (response.StatusCode != HttpStatusCode.InternalServerError)
            return;
        var exception = JsonConvert.DeserializeObject<Exception>(response.Payload);
        throw new InvalidOperationException("The server returned an unexpected status code.", exception);
    }
}
