namespace GdUnit4.Core.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

public class DirectCommandExecutor : ICommandExecutor
{
    public Task Start() => Task.CompletedTask;

    public Task Stop() => Task.CompletedTask;

    public async Task<Response> ExecuteCommand<T>(T command, CancellationToken cancellationToken) where T : BaseCommand => await command.Execute();

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }
}
