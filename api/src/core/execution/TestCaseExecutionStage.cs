namespace GdUnit4.Core.Execution;

using System.Threading.Tasks;

using Asserts;

using Reporting;

internal sealed class TestCaseExecutionStage : ExecutionStage<TestCaseAttribute>
{
    public TestCaseExecutionStage(string name, TestCase testCase, TestCaseAttribute stageAttribute) : base(name, testCase.MethodInfo, stageAttribute)
    {
    }

    public override async Task Execute(ExecutionContext context)
    {
        context.MemoryPool.SetActive(StageName, true);

        await base.Execute(context);

        await context.MemoryPool.Gc();
        if (context.MemoryPool.OrphanCount > 0)
            context.ReportCollector.PushFront(new TestReport(TestReport.ReportType.WARN, context.CurrentTestCase?.Line ?? 0, ReportOrphans(context)));
    }

    private static string ReportOrphans(ExecutionContext context) =>
        $"""
         {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
             Detected <{context.MemoryPool.OrphanCount}> orphan nodes during test execution!
         """;
}
