namespace GdUnit4.Core.Commands;

using System;
using System.Threading.Tasks;

using ExecutionContext = Execution.ExecutionContext;

public abstract class BaseCommand
{
    public string Payload { get; init; } = "";

    public abstract Task<Response> Execute();

    internal ExecutionContext? CurrentExecutionContext(Guid id) => ExecutionContextStore.GetContext(id);
}
