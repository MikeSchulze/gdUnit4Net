// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Extensions;

using Newtonsoft.Json;

internal sealed class GodotGdUnit4RestServer : InOutPipeProxy<NamedPipeServerStream>, ITestEventListener
{
    private readonly SemaphoreSlim processLock = new(1, 1);

    public GodotGdUnit4RestServer(ITestEngineLogger logger)
        : base(new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous), logger)
        => Logger.LogInfo("GodotGdUnit4RestApi:: Starting GdUnit4 RestApi Server.");

    public bool IsFailed { get; set; }

    public int CompletedTests { get; set; }

    public void PublishEvent(ITestEvent testEvent)
        => Task.Run(async () => await WriteAsync(testEvent)
                .ConfigureAwait(false))
            .Wait();

    public new async ValueTask DisposeAsync()
    {
        Logger.LogInfo("Closing GdUnit4 RestApi.");
        processLock.Dispose();
        if (IsConnected)
            Proxy.Disconnect();
        await base
            .DisposeAsync()
            .ConfigureAwait(false);
    }

    public async Task Process()
    {
        if (!IsConnected)
            return;

        await GodotObjectExtensions.SyncProcessFrame;
        if (!await processLock
                .WaitAsync(TimeSpan.FromSeconds(1))
                .ConfigureAwait(true))
            return;

        try
        {
            using CancellationTokenSource tokenSource = new(TimeSpan.FromMinutes(10));
            if (tokenSource.Token.IsCancellationRequested)
            {
                Logger.LogWarning("GodotGdUnit4RestApi:: Operation timed out.");
                return;
            }

            var command = await ReadCommand<BaseCommand>(tokenSource.Token)
                .ConfigureAwait(true);
            var response = await ProcessCommand(command, this)
                .ConfigureAwait(true);
            await WriteResponse(response)
                .ConfigureAwait(true);
        }
        catch (IOException e)
        {
            Logger.LogError($"GodotGdUnit4RestApi:: Client has disconnected by '{e.Message}'");
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Logger.LogError($"GodotGdUnit4RestApi:: {ex.Message} \n{ex.StackTrace}");
        }
        finally
        {
            processLock.Release();
        }
    }

    internal async Task Start()
    {
        Logger.LogInfo("GodotGdUnit4RestApi:: Waiting for client connecting.");
        await Proxy
            .WaitForConnectionAsync()
            .ConfigureAwait(false);
        Logger.LogInfo($"GodotGdUnit4RestApi:: Client connected. User:{Proxy.GetImpersonationUserName()}");
    }

    internal void Stop() => Task.Run(async () =>
    {
        if (await processLock.WaitAsync(TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false))
        {
            try
            {
                await DisposeAsync()
                    .ConfigureAwait(false);
            }
            finally
            {
                processLock.Release();
            }
        }
        else
            Logger.LogWarning("GodotGdUnit4RestApi:: Stop requested but processing is in progress.");
    });

    private async Task<Response> ProcessCommand(BaseCommand command, ITestEventListener testEventListener)
    {
        try
        {
            // Logger.LogInfo($"GodotGdUnit4RestApi:: Processing command {command}.");
            return await command
                .Execute(testEventListener)
                .ConfigureAwait(false);
        }
#pragma warning disable CA1031
        catch (Exception ex)
#pragma warning restore CA1031
        {
            Logger.LogError($"GodotGdUnit4RestApi:: Processing command failed {ex.Message}.");
            return new Response
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Payload = JsonConvert.SerializeObject(ex)
            };
        }
    }
}
