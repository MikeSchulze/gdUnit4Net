namespace GdUnit4.Core.Runners;

using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Commands;

using Events;

using Extensions;

using Newtonsoft.Json;

internal sealed class GodotGdUnit4RestServer : InOutPipeProxy<NamedPipeServerStream>, ITestEventListener
{
    private readonly SemaphoreSlim processLock = new(1, 1);

    public GodotGdUnit4RestServer(ITestEngineLogger logger)
        : base(new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous), logger)
        => Logger.LogInfo("GodotGdUnit4RestApi:: Starting GdUnit4 RestApi Server.");

    public void Dispose()
    {
    }


    public bool IsFailed { get; set; }

    public int CompletedTests { get; set; }

    public void PublishEvent(TestEvent testEvent)
        => Task.Run(async () => await WriteAsync(testEvent)).Wait();

    public new async ValueTask DisposeAsync()
    {
        Logger.LogInfo("Closing GdUnit4 RestApi.");
        processLock.Dispose();
        if (IsConnected) Proxy.Disconnect();
        await base.DisposeAsync();
    }

    internal async Task Start()
    {
        Logger.LogInfo("GodotGdUnit4RestApi:: Waiting for client connecting.");
        await Proxy.WaitForConnectionAsync();
        Logger.LogInfo($"GodotGdUnit4RestApi:: Client connected. User:{Proxy.GetImpersonationUserName()}");
    }

    public void Stop() => Task.Run(async () =>
    {
        if (await processLock.WaitAsync(TimeSpan.FromMilliseconds(100)))
            try
            {
                await DisposeAsync();
            }
            finally
            {
                processLock.Release();
            }
        else
            Logger.LogWarning("GodotGdUnit4RestApi:: Stop requested but processing is in progress.");
    });

    public async Task Process()
    {
        if (!IsConnected)
            return;

        await GodotObjectExtensions.SyncProcessFrame;
        if (!await processLock.WaitAsync(TimeSpan.FromSeconds(1))) return;

        try
        {
            using CancellationTokenSource tokenSource = new(TimeSpan.FromMinutes(10));
            if (tokenSource.Token.IsCancellationRequested)
            {
                Logger.LogWarning("GodotGdUnit4RestApi:: Operation timed out.");
                return;
            }

            var command = await ReadCommand<BaseCommand>(tokenSource.Token);
            var response = await ProcessCommand(command, this);
            await WriteResponse(response);
        }
        catch (IOException e)
        {
            Logger.LogError($"GodotGdUnit4RestApi:: Client has disconnected by '{e.Message}'");
        }
        catch (Exception ex)
        {
            Logger.LogError($"GodotGdUnit4RestApi:: {ex.Message} \n{ex.StackTrace}");
        }
        finally
        {
            processLock.Release();
        }
    }

    private async Task<Response> ProcessCommand(BaseCommand command, ITestEventListener testEventListener)
    {
        try
        {
            //Logger.LogInfo($"GodotGdUnit4RestApi:: Processing command {command}.");
            return await command.Execute(testEventListener);
        }
        catch (Exception ex)
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
