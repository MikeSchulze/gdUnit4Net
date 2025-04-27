namespace GdUnit4.Core.Execution;

using System.Threading.Tasks;

internal interface IExecutionStage
{
    public Task Execute(ExecutionContext context);
}
