namespace GdUnit4.Core.Execution;

using System.Threading.Tasks;

internal class BeforeExecutionStage : ExecutionStage<BeforeAttribute>
{
    public BeforeExecutionStage(TestSuite testSuite) : base("Before", testSuite.Instance.GetType())
    {
    }

    public override async Task Execute(ExecutionContext context)
    {
        context.FireBeforeEvent();
        context.MemoryPool.SetActive(StageName, true);
        await base.Execute(context);
        context.MemoryPool.StopMonitoring();
    }
}
