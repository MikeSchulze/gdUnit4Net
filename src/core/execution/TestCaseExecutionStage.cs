using System;
using System.Threading.Tasks;
using System.Linq;

using GdUnit3.Asserts;

namespace GdUnit3.Executions
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
            context.MemoryPool.ReleaseRegisteredObjects();
            context.OrphanMonitor.Stop();

            if (context.OrphanMonitor.OrphanCount > 0)
                context.ReportCollector.PushFront(new TestReport(TestReport.TYPE.WARN, context.CurrentTestCase?.Line ?? 0, ReportOrphans(context)));
        }

        private static string ReportOrphans(ExecutionContext context) =>
            String.Format("{0}\n Detected <{1}> orphan nodes during test execution!",
                AssertFailures.FormatValue("WARNING:", AssertFailures.WARN_COLOR, false),
                context.OrphanMonitor.OrphanCount);
    }
}
