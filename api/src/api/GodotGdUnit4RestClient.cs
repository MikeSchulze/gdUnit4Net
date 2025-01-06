namespace GdUnit4.Api;

using System;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Core.Commands;

public sealed class GodotGdUnit4RestClient : InOutPipeProxy<NamedPipeClientStream>, ICommandExecutor
{
    public GodotGdUnit4RestClient(ITestEngineLogger logger)
        : base(new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation), logger)
        => Logger.LogInfo("Starting GodotGdUnit4RestClient.");

    public async Task<Response> ExecuteCommand<T>(T command, CancellationToken cancellationToken) where T : BaseCommand
    {
        if (!IsConnected)
            throw new InvalidOperationException("Client is not connected");

        try
        {
            await WriteCommand(command);
            return await ReadResponse(cancellationToken);
        }
        catch (Exception e)
        {
            if (e is IOException && cancellationToken.IsCancellationRequested)
                return new Response
                {
                    StatusCode = HttpStatusCode.ServiceUnavailable,
                    Payload = "Connection interrupted by user."
                };

            throw;
        }
    }

    public async Task Start()
    {
        try
        {
            await Proxy.ConnectAsync(5000);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task Stop()
    {
        try
        {
            await ExecuteCommand(new TerminateGodotInstanceCommand(), CancellationToken.None);
            await Proxy.DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
