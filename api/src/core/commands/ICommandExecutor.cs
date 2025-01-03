namespace GdUnit4.Core.Commands;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface ICommandExecutor : IAsyncDisposable
{
    public Task Start();

    public Task<Response> ExecuteCommand<T>(T command, CancellationToken cancellationToken) where T : BaseCommand;
    public Task Stop();
}
