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
    protected BaseTestRunner(ICommandExecutor executor, ITestEngineLogger logger)
    {
        Executor = executor;
        Logger = logger;
    }

    private object SyncLock { get; } = new();
    private ICommandExecutor Executor { get; }
    protected ITestEngineLogger Logger { get; }
    private CancellationTokenSource? RunnerCancellationToken { get; set; }

    public async ValueTask DisposeAsync()
    {
        await Executor.DisposeAsync();
        RunnerCancellationToken?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void Cancel()
    {
        Logger.LogInfo("Cancelling... test run.");
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
                    await Executor.Start();
                    foreach (var testSuite in testSuiteNodes)
                    {
                        //using (var stdoutHook = testSuiteContext.IsCaptureStdOut ? StdOutHookFactory.CreateStdOutHook() : null)
                        var response = await Executor.ExecuteCommand(new ExecuteSetupTestSuiteCommand(testSuite), token);
                        ValidateResponse(response);
                        try
                        {
                            foreach (var test in testSuite.Tests)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                eventListener.PublishEvent(TestEvent.SetupTest(test.Id));
                                await Executor.ExecuteCommand(new ExecuteTestSetupCommand(test), token);
                                await Executor.ExecuteCommand(new ExecuteTestCommand(test), token)
                                    .ContinueWith(result => { });
                                await Executor.ExecuteCommand(new ExecuteTestTeardownCommand(test), token)
                                    .ContinueWith(result =>
                                    {
                                        try
                                        {
                                            ValidateResponse(result.Result);
                                            var testEvent = JsonConvert.DeserializeObject<TestEvent>(result.Result.Payload,
                                                new JsonSerializerSettings
                                                {
                                                    TypeNameHandling = TypeNameHandling.All,
                                                    Formatting = Formatting.Indented
                                                })!;
                                            eventListener.PublishEvent(testEvent);
                                        }
                                        catch (Exception e)
                                        {
                                            Logger.LogError($"{e.Message}\n{e.StackTrace}");
                                        }

                                        return Task.CompletedTask;
                                    });
                            }
                        }
                        finally
                        {
                            response = await Executor.ExecuteCommand(new ExecuteTeardownTestSuiteCommand(testSuite), token);
                            ValidateResponse(response);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    Logger.LogError("Failed to connect: Connection timeout");
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInfo("Test execution was cancelled");
                }
                catch (Exception ex)
                {
                    Logger.LogError($"{ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    Console.WriteLine("Test execution was stopped");
                    //  await Executor.Stop();
                }
            }, token)
            .ContinueWith(t =>
            {
                lock (SyncLock)
                {
                    RunnerCancellationToken?.Dispose();
                    RunnerCancellationToken = null;
                }
            }, token)
            .Wait(token);
    }

    private void ValidateResponse(Response response)
    {
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var exception = JsonConvert.DeserializeObject<Exception>(response.Payload);
            throw new InvalidOperationException("The server returned an unexpected status code.", exception);
        }
    }
}
