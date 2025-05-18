// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System;
using System.IO.Pipes;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Newtonsoft.Json;

using Reporting;

using Runners;

using static Api.ITestReport.ReportType;

/// <summary>
///     Implements a command executor that communicates with the Godot runtime through named pipes.
///     Handles test command execution, event processing, and interprocess communication with the Godot engine.
/// </summary>
/// <remarks>
///     This executor uses a named pipe for bidirectional communication with the Godot process.
///     It manages the connection lifecycle and handles command execution with event propagation.
/// </remarks>
internal sealed class GodotRuntimeExecutor : InOutPipeProxy<NamedPipeClientStream>, ICommandExecutor
{
    public GodotRuntimeExecutor(ITestEngineLogger logger)
        : base(new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation), logger)
        => Logger.LogInfo("Starting GodotGdUnit4RestClient.");

    public async Task StartAsync()
    {
        try
        {
            await Proxy.ConnectAsync(10000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            await ExecuteCommand(new TerminateGodotInstanceCommand(), new NoInteractTestEventListener(), CancellationToken.None);

            // Give server time to process shutdown
            await Task.Delay(100);
            await DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Response> ExecuteCommand<T>(T command, ITestEventListener testEventListener, CancellationToken cancellationToken)
        where T : BaseCommand
    {
        if (!IsConnected)
            throw new InvalidOperationException("Client is not connected");

        // do not run the command if cancellation requested
        cancellationToken.ThrowIfCancellationRequested();

        // commit command
        await WriteCommand(command);

        // read incoming data until is command response or canceled
        TestEvent? lastTestEvent = null;
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var data = await ReadInData(cancellationToken);
                switch (data)
                {
                    case TestEvent testEvent:
                        // save last event to be used for test cancellation report
                        lastTestEvent = testEvent;
                        testEventListener.PublishEvent(testEvent);
                        break;
                    case Response response:
                        if (response.StatusCode != HttpStatusCode.Gone || lastTestEvent == null)
                            return response;

                        // if connection gone we report at interrupted to the actual test
                        var testCanceledEvent = TestEvent
                            .AfterTest(lastTestEvent.Id, lastTestEvent.ResourcePath, lastTestEvent.SuiteName, lastTestEvent.TestName)
                            .WithStatistic(TestEvent.StatisticKey.Errors, 1)
                            .WithReport(new TestReport(INTERRUPTED, 0, response.Payload));
                        testEventListener.PublishEvent(testCanceledEvent);
                        return response;
                    default:
                        continue;
                }
            }
            catch (Exception ex)
            {
                return new Response
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Payload = JsonConvert.SerializeObject(ex)
                };
            }
        }

        return new Response
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Payload = string.Empty
        };
    }
}

internal class NoInteractTestEventListener : ITestEventListener
{
    public bool IsFailed { get; set; }

    public int CompletedTests { get; set; }

    public void PublishEvent(ITestEvent testEvent)
    {
    }
}
