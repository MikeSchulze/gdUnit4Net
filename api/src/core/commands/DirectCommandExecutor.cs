namespace GdUnit4.Core.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

using Api;

using Events;

public class DirectCommandExecutor : ICommandExecutor
{
    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public async Task<Response> ExecuteCommand<T>(T command, ITestEventListener testEventListener, CancellationToken cancellationToken) where T : BaseCommand
        => await command.Execute(testEventListener);
}
