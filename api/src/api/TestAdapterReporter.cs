namespace GdUnit4.Api;

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

using Newtonsoft.Json;

internal class TestAdapterReporter : ITestEventListener
{
    public const string PipeName = "gdunit4-event-pipe";
    private readonly NamedPipeClientStream client;
    private readonly StreamWriter? writer;

    public TestAdapterReporter()
    {
        client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation);
        if (!client.IsConnected)
            try
            {
                Console.WriteLine("Try to connect to GdUnit4 test report server!");
                client.Connect(TimeSpan.FromSeconds(5));
                writer = new StreamWriter(client) { AutoFlush = true };
                writer.WriteLine("TestAdapterReporter: Successfully connected to GdUnit4 test report server!");
            }
            catch (TimeoutException e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
    }

    public void Dispose()
    {
        Console.WriteLine("TestAdapterReporter: Disconnecting from GdUnit4 test report server.");
        writer?.WriteLine("TestAdapterReporter: Disconnecting from GdUnit4 test report server.");
        writer?.Dispose();
        client.Dispose();
    }

    public bool IsFailed { get; set; }

    public void PublishEvent(TestEvent e)
    {
        if (e.IsFailed || e.IsError)
            IsFailed = true;
        var json = JsonConvert.SerializeObject(e);
        writer!.WriteLine($"GdUnitTestEvent:{json}");
    }
}
