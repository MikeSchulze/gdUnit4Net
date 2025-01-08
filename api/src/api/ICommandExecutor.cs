namespace GdUnit4.Api;

using System;
using System.Threading;
using System.Threading.Tasks;

using Core.Commands;
using Core.Events;

public interface ICommandExecutor : IAsyncDisposable
{
    public Task StartAsync();
    public Task StopAsync();

    public Task<Response> ExecuteCommand<T>(T command, ITestEventListener testEventListener, CancellationToken cancellationToken) where T : BaseCommand;
}
