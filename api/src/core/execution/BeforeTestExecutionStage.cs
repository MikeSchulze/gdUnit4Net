namespace GdUnit4.Executions;

using System.Threading.Tasks;

internal class BeforeTestExecutionStage : ExecutionStage<BeforeTestAttribute>
{
    public BeforeTestExecutionStage(TestSuite testSuite) : base("BeforeTest", testSuite.Instance.GetType())
    { }

    public override async Task Execute(ExecutionContext context)
    {
        context.FireBeforeTestEvent();
        if (!context.IsSkipped)
        {
            context.MemoryPool.SetActive(StageName);
            context.OrphanMonitor.Start(true);
            await base.Execute(context);
            context.OrphanMonitor.Stop();
        }
    }
}
