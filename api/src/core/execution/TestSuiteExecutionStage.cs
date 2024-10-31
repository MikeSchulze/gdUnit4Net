namespace GdUnit4.Core.Execution;

using System;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Hooks;

using Reporting;

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
        await BeforeStage.Execute(testSuiteContext);
        using (var stdoutHook = testSuiteContext.IsCaptureStdOut ? StdOutHookFactory.CreateStdOutHook() : null)
            foreach (var testCase in testSuiteContext.TestSuite.TestCases)
            {
                using var testCaseContext = new ExecutionContext(testSuiteContext, testCase);
                if (testCase.HasDataPoint)
                    await RunTestCaseWithDataPoint(stdoutHook, testCaseContext, testCase);
                else if (testCase.IsParameterized)
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

    private async Task RunTestCaseWithDataPoint(IStdOutHook? stdoutHook, ExecutionContext executionContext, TestCase testCase)
    {
        executionContext.FireBeforeTestEvent();

        try
        {
            var testAttribute = testCase.TestCaseAttributes.First();
            if (DataPointValueProvider.IsAsyncDataPoint(testCase))
                try
                {
                    var timeout = executionContext.GetExecutionTimeout(testAttribute);
                    await foreach (var dataPointValues in DataPointValueProvider.GetDataAsync(testCase, timeout).ConfigureAwait(false))
                    {
                        using ExecutionContext testCaseContext = new(executionContext, testCase, testCase.TestCaseAttributes);
                        await RunTestCase(stdoutHook, testCaseContext, testCase, testAttribute, dataPointValues);
                    }
                }
                catch (AsyncDataPointCanceledException e)
                {
                    if (!executionContext.IsExpectingToFailWithException(e))
                        executionContext.ReportCollector.Consume(
                            new TestReport(
                                TestReport.ReportType.INTERRUPTED,
                                executionContext.CurrentTestCase?.Line ?? -1,
                                e.Message,
                                e.StackTrace));
                }
            else
                foreach (var dataPointValues in DataPointValueProvider.GetData(testCase))
                {
                    using ExecutionContext testCaseContext = new(executionContext, testCase, testCase.TestCaseAttributes);
                    await RunTestCase(stdoutHook, testCaseContext, testCase, testAttribute, dataPointValues);
                }
        }
        catch (Exception e)
        {
            executionContext.ReportCollector.Consume(new TestReport(TestReport.ReportType.FAILURE, executionContext.CurrentTestCase?.Line ?? -1, e.Message, e.StackTrace));
        }

        executionContext.FireAfterTestEvent();
    }

    private async Task RunParameterizedTest(IStdOutHook? stdoutHook, ExecutionContext executionContext, TestCase testCase)
    {
        executionContext.FireBeforeTestEvent();
        foreach (var testAttribute in testCase.TestCaseAttributes)
        {
            using ExecutionContext testCaseContext = new(executionContext, testCase, testAttribute);
            await RunTestCase(stdoutHook, testCaseContext, testCase, testAttribute, testAttribute.Arguments);
        }

        executionContext.FireAfterTestEvent();
    }

    private async Task RunTestCase(IStdOutHook? stdoutHook, ExecutionContext executionContext, TestCase testCase, TestCaseAttribute stageAttribute,
        params object?[] methodArguments)
    {
        try
        {
            // start capturing stdout if enabled
            stdoutHook?.StartCapture();

            await BeforeTestStage.Execute(executionContext);

            using ExecutionContext context = new(executionContext, methodArguments);
            await new TestCaseExecutionStage(context.TestCaseName, testCase, stageAttribute).Execute(context);
        }
        finally
        {
            // stop capturing stdout and add as report if enabled
            stdoutHook?.StopCapture();
            var stdoutMessage = stdoutHook?.GetCapturedOutput();
            if (!string.IsNullOrEmpty(stdoutMessage))
            {
                executionContext.ReportCollector.PushFront(new TestReport(TestReport.ReportType.STDOUT,
                    executionContext.CurrentTestCase?.Line ?? 0, stdoutMessage));
                // and finally redirect to the console because it was fully captured
                Console.WriteLine(stdoutMessage);
            }

            await AfterTestStage.Execute(executionContext);
        }
    }
}
