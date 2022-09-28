using System.Threading.Tasks;

namespace GdUnit3.Executions
{
    internal interface IExecutionStage
    {
        public Task Execute(ExecutionContext context);
    }
}
