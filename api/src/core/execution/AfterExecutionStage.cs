namespace GdUnit4.Executions;

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using GdUnit4.Asserts;

internal class AfterExecutionStage : ExecutionStage<AfterAttribute>
{
    public AfterExecutionStage(TestSuite testSuite) : base("After", testSuite.Instance.GetType())
    { }

    public override async Task Execute(ExecutionContext context)
    {
        context.MemoryPool.SetActive(StageName);
        context.OrphanMonitor.Start();
        await base.Execute(context);
        Utils.ClearTempDir();
        context.MemoryPool.ReleaseRegisteredObjects();
        context.OrphanMonitor.Stop();
        if (context.OrphanMonitor.OrphanCount > 0)
            context.ReportCollector.PushFront(new TestReport(TestReport.ReportType.WARN, 0, ReportOrphans(context)));
        context.FireAfterEvent();
    }

    private static TestStageAttribute? AfterAttribute(ExecutionContext context) => context.TestSuite.Instance
        .GetType()
        .GetMethods()
        .FirstOrDefault(m => m.IsDefined(typeof(AfterAttribute)))
        ?.GetCustomAttribute<AfterAttribute>();

    private static TestStageAttribute? BeforeAttribute(ExecutionContext context) => context.TestSuite.Instance
        .GetType()
        .GetMethods()
        .FirstOrDefault(m => m.IsDefined(typeof(BeforeAttribute)))
        ?.GetCustomAttribute<BeforeAttribute>();

    private static string ReportOrphans(ExecutionContext context)
    {
        var beforeAttribute = BeforeAttribute(context);
        var afterAttributes = AfterAttribute(context);

        if (beforeAttribute != null && afterAttributes != null)
            return $"""
                {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
                    Detected <{context.OrphanMonitor.OrphanCount}> orphan nodes during test suite setup stage!
                    Check [b]{beforeAttribute.Name + ":" + beforeAttribute.Line}[/b] and [b]{afterAttributes.Name + ":" + afterAttributes.Line}[/b] for unfreed instances!
                """;
        return $"""
                {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
                    Detected <{context.OrphanMonitor.OrphanCount}> orphan nodes during test suite setup stage!
                    Check [b]{(beforeAttribute != null ? (beforeAttribute.Name + ":" + beforeAttribute.Line) : (afterAttributes?.Name + ":" + afterAttributes?.Line))}[/b] for unfreed instances!
                """;
    }
}


