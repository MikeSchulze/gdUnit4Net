namespace GdUnit4.Executions;

using System;
using System.Threading.Tasks;

using core.hooks;

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
    {
        get;
    }

    private AfterExecutionStage AfterStage
    {
        get;
    }

    private BeforeTestExecutionStage BeforeTestStage
    {
        get;
    }

    private AfterTestExecutionStage AfterTestStage
    {
        get;
    }

    public async Task Execute(ExecutionContext testSuiteContext)
    {
        using var stdoutHook = StdOutHookFactory.CreateStdOutHook();
        await BeforeStage.Execute(testSuiteContext);
        foreach (var testCase in testSuiteContext.TestSuite.TestCases)
        {
            using var testCaseContext = new ExecutionContext(testSuiteContext, testCase);
            if (testCase.IsParameterized)
                await RunParameterizedTest(stdoutHook, testCaseContext, testCase);
            else
                await RunTestCase(stdoutHook, testCaseContext, testCase, testCase.TestCaseAttribute, testCase.Arguments);


            if (testCaseContext.IsFailed || testCaseContext.IsError)
            {
                //break;
            }
        }

        await AfterStage.Execute(testSuiteContext);
    }

    private async Task RunParameterizedTest(IStdOutHook iStdOutHook, ExecutionContext executionContext, TestCase testCase)
    {
        executionContext.FireBeforeTestEvent();
        foreach (var testAttribute in testCase.TestCaseAttributes)
        {
            using ExecutionContext testCaseContext = new(executionContext, testCase, testAttribute);
            await RunTestCase(iStdOutHook, testCaseContext, testCase, testAttribute, testAttribute.Arguments);
        }

        executionContext.FireAfterTestEvent();
    }

    private async Task RunTestCase(IStdOutHook? stdoutHook, ExecutionContext executionContext, TestCase testCase, TestCaseAttribute stageAttribute,
        params object?[] methodArguments)
    {
        // start capturing stdout if enabled
        stdoutHook?.StartCapture();

        await BeforeTestStage.Execute(executionContext);
        using (ExecutionContext context = new(executionContext, methodArguments))
            await new TestCaseExecutionStage(context.TestCaseName, testCase, stageAttribute).Execute(context);

        // stop capturing stdout and add as report if enabled
        stdoutHook?.StopCapture();
        var stdoutMessage = stdoutHook?.GetCapturedOutput();
        if (!string.IsNullOrEmpty(stdoutMessage))
        {
            executionContext.ReportCollector.PushFront(new TestReport(TestReport.ReportType.STDOUT, executionContext.CurrentTestCase?.Line ?? 0, stdoutMessage));
            // and finally redirect to the console because it was fully captured
            Console.WriteLine(stdoutMessage);
        }

        await AfterTestStage.Execute(executionContext);
    }
}
