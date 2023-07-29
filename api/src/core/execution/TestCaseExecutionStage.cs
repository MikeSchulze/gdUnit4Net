using System.Threading.Tasks;
using GdUnit4.Asserts;

namespace GdUnit4.Executions
{
    internal sealed class TestCaseExecutionStage : ExecutionStage<TestCaseAttribute>
    {
        public TestCaseExecutionStage(string name, TestCase testCase, TestCaseAttribute stageAttribute) : base(name, testCase.MethodInfo, stageAttribute)
        { }

        public override async Task Execute(ExecutionContext context)
        {
            context.MemoryPool.SetActive(StageName);
            context.OrphanMonitor.Start(true);
            await base.Execute(context);
            await ISceneRunner.SyncProcessFrame;
            context.MemoryPool.ReleaseRegisteredObjects();
            context.OrphanMonitor.Stop();

            if (context.OrphanMonitor.OrphanCount > 0)
                context.ReportCollector.PushFront(new TestReport(TestReport.TYPE.WARN, context.CurrentTestCase?.Line ?? 0, ReportOrphans(context)));
        }

        private static string ReportOrphans(ExecutionContext context) => $"""
            {AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false)}
                Detected <{context.OrphanMonitor.OrphanCount}> orphan nodes during test execution!
            """;
    }
}
