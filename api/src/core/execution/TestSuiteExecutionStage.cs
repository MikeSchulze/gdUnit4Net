namespace GdUnit4.Executions;

using System.Threading.Tasks;

internal sealed class TestSuiteExecutionStage : IExecutionStage
{
    public TestSuiteExecutionStage(TestSuite testSuite)
    {
        BeforeStage = new BeforeExecutionStage(testSuite);
        AfterStage = new AfterExecutionStage(testSuite);
        BeforeTestStage = new BeforeTestExecutionStage(testSuite);
        AfterTestStage = new AfterTestExecutionStage(testSuite);
    }

    private BeforeExecutionStage BeforeStage
    { get; set; }

    private AfterExecutionStage AfterStage
    { get; set; }

    private BeforeTestExecutionStage BeforeTestStage
    { get; set; }

    private AfterTestExecutionStage AfterTestStage
    { get; set; }

    public async Task Execute(ExecutionContext testSuiteContext)
    {
        await BeforeStage.Execute(testSuiteContext);
        foreach (var testCase in testSuiteContext.TestSuite.TestCases)
        {
            using var testCaseContext = new ExecutionContext(testSuiteContext, testCase);
            if (testCase.IsParameterized)
                await RunParameterizedTest(testCaseContext, testCase);
            else
                await RunTestCase(testCaseContext, testCase, testCase.TestCaseAttribute, testCase.Arguments);


            if (testCaseContext.IsFailed || testCaseContext.IsError)
            {
                //break;
            }
        }
        await AfterStage.Execute(testSuiteContext);
    }

    private async Task RunParameterizedTest(ExecutionContext executionContext, TestCase testCase)
    {
        executionContext.FireBeforeTestEvent();
        foreach (var testAttribute in testCase.TestCaseAttributes)
        {
            using ExecutionContext testCaseContext = new(executionContext, testCase, testAttribute);
            await RunTestCase(testCaseContext, testCase, testAttribute, testAttribute.Arguments);
        }
        executionContext.FireAfterTestEvent();
    }

    private async Task RunTestCase(ExecutionContext executionContext, TestCase testCase, TestCaseAttribute stageAttribute, params object?[] methodArguments)
    {
        await BeforeTestStage.Execute(executionContext);
        using (ExecutionContext context = new(executionContext, methodArguments))
        {
            await new TestCaseExecutionStage(context.TestCaseName, testCase, stageAttribute).Execute(context);
        }
        await AfterTestStage.Execute(executionContext);
    }
}
