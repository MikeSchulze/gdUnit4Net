// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Newtonsoft.Json;

internal class InOutPipeProxy<TPipe> : IAsyncDisposable
    where TPipe : PipeStream
{
    protected const string PipeName = "gdunit4-message-pipe";

    protected InOutPipeProxy(TPipe pipe, ITestEngineLogger logger)
    {
        Logger = logger;
        Pipe = pipe;
    }

    // ReSharper disable once StaticMemberInGenericType
    private static JsonSerializerSettings JsonSettings { get; } = new()
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented
    };

    private TPipe Pipe { get; }

    protected ITestEngineLogger Logger { get; }

    protected bool IsConnected => Pipe.IsConnected;

    protected TPipe Proxy => Pipe;

    public async ValueTask DisposeAsync()
    {
        if (IsConnected)
            Pipe.Close();
        await Pipe.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    protected async Task<object?> ReadInData(CancellationToken cancellationToken)
    {
        var responseLengthBytes = new byte[4];
        await ReadExactBytesAsync(responseLengthBytes, 0, 4, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return new Response
            {
                StatusCode = HttpStatusCode.Gone,
                Payload = "Connection interrupted by cancellation requested."
            };

        var responseLength = BinaryPrimitives.ReadInt32LittleEndian(responseLengthBytes);
        var responseBytes = new byte[responseLength];
        await ReadExactBytesAsync(responseBytes, 0, responseLength, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return new Response
            {
                StatusCode = HttpStatusCode.Gone,
                Payload = "Connection interrupted by cancellation requested."
            };

        var json = Encoding.UTF8.GetString(responseBytes);
        if (json.Length == 0)
            return null;

        return JsonConvert.DeserializeObject(json, JsonSettings)
               ?? throw new JsonSerializationException($"Failed to deserialize response:\n{json}");
    }

    protected async Task WriteResponse(Response response) => await WriteAsync(response);

    protected async Task<TCommand> ReadCommand<TCommand>(CancellationToken cancellationToken)
        where TCommand : BaseCommand
    {
        var responseLengthBytes = new byte[4];
        await ReadExactBytesAsync(responseLengthBytes, 0, 4, cancellationToken);
        var responseLength = BinaryPrimitives.ReadInt32LittleEndian(responseLengthBytes);

        if (!IsConnected)
            throw new IOException("Client not connected");

        var responseBytes = new byte[responseLength];
        await ReadExactBytesAsync(responseBytes, 0, responseLength, cancellationToken);
        if (!IsConnected)
            throw new IOException("Client not connected");

        var json = Encoding.UTF8.GetString(responseBytes);
        return DeserializeObject<TCommand>(json);
    }

    protected async Task WriteCommand<TCommand>(TCommand command)
        where TCommand : BaseCommand
        => await WriteAsync(command);

    protected async Task WriteAsync<TData>(TData data)
    {
        var json = SerializeObject(data);
        var messageBytes = Encoding.UTF8.GetBytes(json);

        // Write message length (4 bytes) followed by the message
        var lengthBytes = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lengthBytes, messageBytes.Length);

        await Pipe.WriteAsync(lengthBytes);
        await Pipe.WriteAsync(messageBytes);
        await Pipe.FlushAsync();
    }

    private static string SerializeObject<TObject>(TObject data)
    {
        try
        {
            return JsonConvert.SerializeObject(data, JsonSettings);
        }
        catch (JsonSerializationException ex)
        {
            var response = new Response
            {
                StatusCode = HttpStatusCode.BadRequest,
                Payload = $"Invalid command format: {ex.Message}"
            };
            return JsonConvert.SerializeObject(response, JsonSettings);
        }
    }

    private static TObject DeserializeObject<TObject>(string json)
    {
        var command = JsonConvert.DeserializeObject<TObject>(json, JsonSettings) ?? throw new JsonSerializationException($"Failed to deserialize command payload:'{json}'");
        return command;
    }

    private async Task ReadExactBytesAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;
        while (IsConnected && totalBytesRead < count)
            try
            {
                var bytesRead = await Pipe.ReadAsync(buffer.AsMemory(offset + totalBytesRead, count - totalBytesRead), cancellationToken);
                totalBytesRead += bytesRead;
            }
            catch (OperationCanceledException)
            {
                if (!cancellationToken.IsCancellationRequested)
                    throw;
                break;
            }

        // Console.WriteLine($"{typeof(TPipe)} Read {count} bytes from {totalBytesRead} of {count}, {IsConnected}");
    }
}
