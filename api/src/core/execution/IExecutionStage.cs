namespace GdUnit4.Executions;

using System.Threading.Tasks;

internal interface IExecutionStage
{
    public Task Execute(ExecutionContext context);
}
