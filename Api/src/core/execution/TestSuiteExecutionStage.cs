// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution;

using System.Diagnostics.CodeAnalysis;

using Data;

using Hooks;

using Reporting;

using TestExtensions;

using static Api.ReportType;

internal sealed class TestSuiteExecutionStage : IExecutionStage
{
    public TestSuiteExecutionStage(TestSuite testSuite)
    {
        BeforeStage = new BeforeExecutionStage(testSuite);
        AfterStage = new AfterExecutionStage(testSuite);
        BeforeTestStage = new BeforeTestExecutionStage(testSuite);
        AfterTestStage = new AfterTestExecutionStage(testSuite);
        ExtensionRegistry = new ExtensionRegistry(testSuite.Instance.GetType(), testSuite.Instance);
    }

    private BeforeExecutionStage BeforeStage { get; }

    private AfterExecutionStage AfterStage { get; }

    private BeforeTestExecutionStage BeforeTestStage { get; }

    private AfterTestExecutionStage AfterTestStage { get; }

    private ExtensionRegistry ExtensionRegistry { get; }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "testSuiteContext ownership is transferred to ExecutionContext which handles disposal")]
    public async Task Execute(ExecutionContext testSuiteContext)
    {
        var suiteType = testSuiteContext.TestSuite.Instance.GetType();
        var suiteExtensionContext = new ExtensionContext(suiteType, testSuiteContext.TestSuite.Instance, null, null, []);

        try
        {
            await ExtensionRegistry
                .RunBeforeAll(suiteExtensionContext)
                .ConfigureAwait(true);

            await BeforeStage
                .Execute(testSuiteContext)
                .ConfigureAwait(true);

            using (var stdoutHook = testSuiteContext.IsCaptureStdOut ? StdOutHookFactory.CreateStdOutHook() : null)
            {
                foreach (var testCase in testSuiteContext.TestSuite.TestCases)
                {
                    using var testCaseContext = new ExecutionContext(testSuiteContext, testCase);
                    if (testCase.HasDataPoint)
                    {
                        await RunTestCaseWithDataPoint(stdoutHook, testCaseContext, testCase, suiteExtensionContext)
                            .ConfigureAwait(true);
                    }
                    else
                    {
                        await RunTestCase(stdoutHook, testCaseContext, testCase, testCase.TestCaseAttribute, testCase.Arguments, suiteExtensionContext)
                            .ConfigureAwait(true);
                    }

                    if (testCaseContext.IsFailed || testCaseContext.IsError)
                    {
                        // break;
                    }
                }
            }

            await AfterStage
                .Execute(testSuiteContext)
                .ConfigureAwait(true);
        }
        finally
        {
            await ExtensionRegistry
                .RunAfterAll(suiteExtensionContext)
                .ConfigureAwait(true);
        }
    }

    [SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "testSuiteContext ownership is transferred to ExecutionContext which handles disposal")]
    private async Task RunTestCaseWithDataPoint(IStdOutHook? stdoutHook, ExecutionContext executionContext, TestCase testCase, ExtensionContext suiteExtensionContext)
    {
        executionContext.FireBeforeTestEvent();

        try
        {
            var testAttribute = testCase.TestCaseAttributes.First();
            if (DataPointValueProvider.IsAsyncDataPoint(testCase))
            {
                try
                {
                    var timeout = executionContext.GetExecutionTimeout(testAttribute);
                    await foreach (var dataPointValues in DataPointValueProvider.GetDataAsync(testCase, timeout).ConfigureAwait(false))
                    {
                        var displayName = TestCase.BuildDisplayName(testCase.Name, new TestCaseAttribute(dataPointValues));
                        using ExecutionContext testCaseContext = new(executionContext, displayName);
                        await RunTestCase(stdoutHook, testCaseContext, testCase, testAttribute, dataPointValues, suiteExtensionContext)
                            .ConfigureAwait(true);
                    }
                }
                catch (AsyncDataPointCanceledException e)
                {
                    if (!executionContext.IsExpectingToFailWithException(e, testCase.MethodInfo))
                    {
                        executionContext.ReportCollector.Consume(
                            new TestReport(
                                Interrupted,
                                executionContext.CurrentTestCase?.Line ?? -1,
                                e.Message,
                                e.StackTrace));
                    }
                }
            }
            else
            {
                foreach (var dataPointValues in DataPointValueProvider.GetData(testCase))
                {
                    var displayName = TestCase.BuildDisplayName(testCase.Name, new TestCaseAttribute(dataPointValues));
                    using ExecutionContext testCaseContext = new(executionContext, displayName);
                    await RunTestCase(stdoutHook, testCaseContext, testCase, testAttribute, dataPointValues, suiteExtensionContext)
                        .ConfigureAwait(true);
                }
            }
        }
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            executionContext.ReportCollector.Consume(new TestReport(Failure, executionContext.CurrentTestCase?.Line ?? -1, e.Message, e.StackTrace));
        }

        executionContext.FireAfterTestEvent();
    }

    private async Task RunTestCase(
        IStdOutHook? stdoutHook,
        ExecutionContext executionContext,
        TestCase testCase,
        TestCaseAttribute stageAttribute,
        object?[] methodArguments,
        ExtensionContext suiteExtensionContext)
    {
        var testExtensionContext = new ExtensionContext(
            suiteExtensionContext.TestSuiteType,
            suiteExtensionContext.TestSuiteInstance,
            testCase.MethodInfo,
            executionContext.TestCaseName,
            methodArguments,
            suiteExtensionContext.DataStore);

        try
        {
            // start capturing stdout if enabled
            stdoutHook?.StartCapture();

            // BeforeEach extension callbacks run before [BeforeTest]
            await ExtensionRegistry
                .RunBeforeEach(testExtensionContext, testCase.MethodInfo)
                .ConfigureAwait(true);

            await BeforeTestStage
                .Execute(executionContext)
                .ConfigureAwait(true);

            if (!executionContext.IsSkipped)
            {
                var resolvedArguments = ExtensionRegistry.ResolveArguments(testCase.MethodInfo, testExtensionContext, methodArguments);
                using ExecutionContext context = new(executionContext, resolvedArguments);
                await new TestCaseExecutionStage(context.TestCaseName, testCase, stageAttribute)
                    .Execute(context)
                    .ConfigureAwait(true);
            }
        }
        finally
        {
            // stop capturing stdout and add as report if enabled
            stdoutHook?.StopCapture();
            var stdoutMessage = stdoutHook?.GetCapturedOutput();
            if (!string.IsNullOrEmpty(stdoutMessage))
            {
                executionContext.ReportCollector.PushFront(
                    new TestReport(
                        Stdout,
                        executionContext.CurrentTestCase?.Line ?? 0,
                        stdoutMessage));

                // and finally redirect to the console because it was fully captured
                Console.WriteLine(stdoutMessage);
            }

            await AfterTestStage
                .Execute(executionContext)
                .ConfigureAwait(true);

            // AfterEach extension callbacks run after [AfterTest]
            await ExtensionRegistry
                .RunAfterEach(testExtensionContext, testCase.MethodInfo)
                .ConfigureAwait(true);
        }
    }
}
