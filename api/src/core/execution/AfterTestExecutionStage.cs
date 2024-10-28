namespace GdUnit4.Core.Execution;

using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Asserts;

using Reporting;

using Signals;

internal class AfterTestExecutionStage : ExecutionStage<AfterTestAttribute>
{
    public AfterTestExecutionStage(TestSuite testSuite) : base("AfterTest", testSuite.Instance.GetType())
    {
    }

    public override async Task Execute(ExecutionContext context)
    {
        if (!context.IsSkipped)
        {
            GodotSignalCollector.Instance.Clean();
            context.MemoryPool.SetActive(StageName);
            context.OrphanMonitor.Start();
            await base.Execute(context);
            context.MemoryPool.ReleaseRegisteredObjects();
            context.OrphanMonitor.Stop();
            if (context.OrphanMonitor.OrphanCount > 0)
                context.ReportCollector.PushFront(new TestReport(TestReport.ReportType.WARN, 0, ReportOrphans(context)));
        }

        context.FireAfterTestEvent();
    }

    private static AfterTestAttribute? AfterTestAttribute(ExecutionContext context) => context.TestSuite.Instance
        .GetType()
        .GetMethods()
        .FirstOrDefault(m => m.IsDefined(typeof(AfterTestAttribute)))
        ?.GetCustomAttribute<AfterTestAttribute>();

    private static BeforeTestAttribute? BeforeTestAttribute(ExecutionContext context) => context.TestSuite.Instance
        .GetType()
        .GetMethods()
        .FirstOrDefault(m => m.IsDefined(typeof(BeforeTestAttribute)))
        ?.GetCustomAttribute<BeforeTestAttribute>();

    private static string ReportOrphans(ExecutionContext context)
    {
        var beforeAttribute = BeforeTestAttribute(context);
        var afterAttributes = AfterTestAttribute(context);
        if (beforeAttribute != null && afterAttributes != null)
            return $"""
                    {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
                        Detected <{context.OrphanMonitor.OrphanCount}> orphan nodes during test setup stage!
                        Check [b]{beforeAttribute.Name + ":" + beforeAttribute.Line}[/b] and [b]{afterAttributes.Name + ":" + afterAttributes.Line}[/b] for unfreed instances!
                    """;
        return $"""
                {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
                    Detected <{context.OrphanMonitor.OrphanCount}> orphan nodes during test setup stage!
                    Check [b]{(beforeAttribute != null ? beforeAttribute.Name + ":" + beforeAttribute.Line : afterAttributes?.Name + ":" + afterAttributes?.Line)}[/b] for unfreed instances!
                """;
    }
}
