namespace GdUnit4.Api;

using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Core.Commands;

using Newtonsoft.Json;

public class InOutPipeProxy<TPipe> : IAsyncDisposable where TPipe : PipeStream
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

    protected async Task<Response> ReadResponse()
    {
        var responseLengthBytes = new byte[4];
        await ReadExactBytesAsync(responseLengthBytes, 0, 4);
        var responseLength = BinaryPrimitives.ReadInt32LittleEndian(responseLengthBytes);


        var responseBytes = new byte[responseLength];
        await ReadExactBytesAsync(responseBytes, 0, responseLength);

        var json = Encoding.UTF8.GetString(responseBytes);

        return JsonConvert.DeserializeObject<Response>(json, JsonSettings)
               ?? throw new JsonSerializationException("Failed to deserialize response");
    }

    protected async Task WriteResponse(Response response) => await WriteAsync(response);

    protected async Task<TCommand> ReadCommand<TCommand>() where TCommand : BaseCommand
    {
        var responseLengthBytes = new byte[4];
        await ReadExactBytesAsync(responseLengthBytes, 0, 4);
        var responseLength = BinaryPrimitives.ReadInt32LittleEndian(responseLengthBytes);

        if (!IsConnected)
            throw new IOException("Client not connected");

        var responseBytes = new byte[responseLength];
        await ReadExactBytesAsync(responseBytes, 0, responseLength);
        if (!IsConnected)
            throw new IOException("Client not connected");

        var json = Encoding.UTF8.GetString(responseBytes);
        return DeserializeObject<TCommand>(json);
    }

    protected async Task WriteCommand<TCommand>(TCommand command) where TCommand : BaseCommand => await WriteAsync(command);

    private async Task WriteAsync<TData>(TData data)
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
        var command = JsonConvert.DeserializeObject<TObject>(json, JsonSettings);
        if (command == null)
            throw new JsonSerializationException($"Failed to deserialize command payload:'{json}'");
        return command;
    }

    private async Task ReadExactBytesAsync(byte[] buffer, int offset, int count)
    {
        var totalBytesRead = 0;
        while (IsConnected && totalBytesRead < count)
        {
            var bytesRead = await Pipe.ReadAsync(buffer.AsMemory(offset + totalBytesRead, count - totalBytesRead));
            totalBytesRead += bytesRead;
        }

        //Console.WriteLine($"{typeof(TPipe)} Read {count} bytes from {totalBytesRead} of {count}, {IsConnected}");
    }
}
