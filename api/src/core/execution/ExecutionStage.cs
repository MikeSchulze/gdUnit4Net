namespace GdUnit4.Core.Execution;

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Exceptions;

using Extensions;

using Hooks;

using Reporting;

using static Reporting.TestReport;

internal abstract class ExecutionStage<T> : IExecutionStage
{
    protected ExecutionStage(string name, Type type)
    {
        var method = type
            .GetMethods()
            .FirstOrDefault(m => m.IsDefined(typeof(T)));
        InitExecutionAttributes(method?.Name ?? name, method, method?.GetCustomAttribute<TestStageAttribute>()!);
    }

    protected ExecutionStage(string name, MethodInfo method, TestStageAttribute stageAttribute)
        => InitExecutionAttributes(name, method, stageAttribute);

    protected string StageName { get; private set; } = "";

    private bool IsAsync { get; set; }

    private bool IsTask { get; set; }

    private int DefaultTimeout { get; } = 30000;

    private MethodInfo? Method { get; set; }

    private TestStageAttribute? StageAttribute { get; set; }

    public virtual async Task Execute(ExecutionContext context)
    {
        // no stage defined?
        if (Method == default)
        {
            await Task.Run(() => { });
            return;
        }

        try
        {
            // if the method is defined asynchronously, the return type must be a Task
            if (IsAsync != IsTask)
            {
                context.ReportCollector.Consume(new TestReport(ReportType.FAILURE, ExecutionLineNumber(context),
                    $"Invalid method signature found at: {StageName}.\n You must return a <Task> for an asynchronously specified method."));
                return;
            }

            // subscribe on Godot caught exceptions
            Exception? caughtException = null;
            using var subscribe = GodotExceptionHook.Subscribe(ex => caughtException ??= ex);

            await ExecuteStage(context);
            // For async tests, wait for one more frame to catch any pending exceptions
            if (IsAsync)
            {
                await ISceneRunner.SyncProcessFrame;
                await ISceneRunner.SyncPhysicsFrame;
            }

            // If we caught an exception during execution, throw it now
            if (caughtException != null) ExceptionDispatchInfo.Capture(caughtException).Throw();

            ValidateForExpectedException(context);
        }
        catch (ExecutionTimeoutException e)
        {
            if (ValidateForExpectedException(context, e))
                return;
            if (context.FailureReporting)
                context.ReportCollector.Consume(new TestReport(ReportType.INTERRUPTED, e.LineNumber, e.Message));
        }
        catch (TestFailedException e)
        {
            ReportAsFailure(context, e);
        }
        catch (Exception e)
        {
            if (e.GetBaseException() is TestFailedException ex)
                ReportAsFailure(context, ex);
            else
                // handle unexpected exceptions
                ReportUnexpectedException(context, e);
        }
    }

    private void InitExecutionAttributes(string stageName, MethodInfo? method, TestStageAttribute stageAttribute)
    {
        StageName = stageName;
        Method = method;
        StageAttribute = stageAttribute;
        IsAsync = method?.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
        IsTask = method?.ReturnType.IsEquivalentTo(typeof(Task)) ?? false;
    }


    private static bool ValidateForExpectedException(ExecutionContext context, Exception? e = null)
    {
        try
        {
            if (context.IsExpectingToFailWithException(e))
                return true;
        }
        catch (TestFailedException e2)
        {
            context.ReportCollector.Consume(new TestReport(e2));
            return true;
        }

        return false;
    }

    private static void ReportAsFailure(ExecutionContext context, TestFailedException e)
    {
        if (ValidateForExpectedException(context, e)) return;

        if (context.FailureReporting)
            context.ReportCollector.Consume(new TestReport(e));
    }

    private static void ReportUnexpectedException(ExecutionContext context, Exception exception)
    {
        if (exception is TargetInvocationException)
        {
            var ei = ExceptionDispatchInfo.Capture(exception.InnerException ?? exception);
            ReportUnexpectedException(context, ei.SourceException);
        }
        else
        {
            if (ValidateForExpectedException(context, exception))
                return;
            var stack = new StackTrace(exception, true);
            var lineNumber = ScanFailureLineNumber(stack);
            context.ReportCollector.Consume(new TestReport(ReportType.FAILURE, lineNumber, exception.Message, TrimStackTrace(stack.ToString())));
        }
    }

    private static int ScanFailureLineNumber(StackTrace stack)
    {
        foreach (var frame in stack.GetFrames().Reverse())
        {
            if (frame.GetFileName() == null)
                continue;
            if (frame.GetMethod()?.IsDefined(typeof(TestCaseAttribute)) ?? false)
                return frame.GetFileLineNumber();
        }

        return stack.FrameCount > 1 ? stack.GetFrame(0)!.GetFileLineNumber() : -1;
    }

    internal static string TrimStackTrace(string stackTrace)
    {
        if (stackTrace.Length == 0)
            return stackTrace;

        StringBuilder result = new(stackTrace.Length);
        var stackFrames = Regex.Split(stackTrace, Environment.NewLine);

        foreach (var stackFrame in stackFrames)
        {
            if (string.IsNullOrEmpty(stackFrame) || stackFrame.Contains("Microsoft.VisualStudio.TestTools"))
                continue;

            result.Append(stackFrame);
            result.Append(Environment.NewLine);
        }

        return result.ToString();
    }

    private async Task ExecuteStage(ExecutionContext context)
    {
        using var tokenSource = new CancellationTokenSource();
        var timeout = TimeSpan.FromMilliseconds(StageAttribute!.Timeout != -1 ? StageAttribute.Timeout : DefaultTimeout);
        //Godot.GD.PrintS("Execute", StageName);//, context.MethodArguments.Formatted());
        var obj = Method?.Invoke(context.TestSuite.Instance, context.MethodArguments);
        var task = obj is Task t ? t : Task.Run(() => { });
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout, tokenSource.Token));
        tokenSource.Cancel();
        if (completedTask == task)
            // Very important in order to propagate exceptions
            await task;
        else
            throw new ExecutionTimeoutException($"The execution has timed out after {timeout.Humanize()}.", ExecutionLineNumber(context));
    }

    private int ExecutionLineNumber(ExecutionContext context)
    {
        if (StageAttribute?.Line == -1)
            return context.CurrentTestCase?.Line ?? -1;
        return StageAttribute?.Line ?? -1;
    }
}
