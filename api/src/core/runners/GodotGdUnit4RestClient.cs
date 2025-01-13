namespace GdUnit4.Core.Runners;

using System;
using System.IO.Pipes;
using System.Net;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Commands;

using Events;

using Newtonsoft.Json;

using Reporting;

internal sealed class GodotGdUnit4RestClient : InOutPipeProxy<NamedPipeClientStream>, ICommandExecutor
{
    public GodotGdUnit4RestClient(ITestEngineLogger logger)
        : base(new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.Impersonation), logger)
        => Logger.LogInfo("Starting GodotGdUnit4RestClient.");

    public async Task<Response> ExecuteCommand<T>(T command, ITestEventListener testEventListener, CancellationToken cancellationToken) where T : BaseCommand
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
                            .WithStatistic(TestEvent.STATISTIC_KEY.ERRORS, 1)
                            .WithReport(new TestReport(TestReport.ReportType.INTERRUPTED, 0, response.Payload));
                        testEventListener.PublishEvent(testCanceledEvent);
                        return response;
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

        return new Response
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Payload = ""
        };
    }

    public async Task StartAsync()
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

    public async Task StopAsync()
    {
        try
        {
            // await ExecuteCommand(new TerminateGodotInstanceCommand(), CancellationToken.None);
            await Proxy.DisposeAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
