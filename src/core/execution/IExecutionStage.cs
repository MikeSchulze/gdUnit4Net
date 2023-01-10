using System.Threading.Tasks;

namespace GdUnit4.Executions
{
    internal interface IExecutionStage
    {
        public Task Execute(ExecutionContext context);
    }
}
