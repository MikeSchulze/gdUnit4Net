using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace GdUnit3.Executions
{
    using Exceptions;

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
        {
            InitExecutionAttributes(name, method, stageAttribute);
        }

        private void InitExecutionAttributes(string stageName, MethodInfo? method, TestStageAttribute stageAttribute)
        {
            StageName = stageName;
            Method = method;
            StageAttribute = stageAttribute;
            IsAsync = method?.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            IsTask = method?.ReturnType.IsEquivalentTo(typeof(Task)) ?? false;
        }

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
                    context.ReportCollector.Consume(new TestReport(TestReport.TYPE.FAILURE, ExecutionLineNumber(context), $"Invalid method signature found at: {StageName}.\n You must return a <Task> for an asynchronously specified method."));
                    return;
                }
                await ExecuteStage(context);
            }
            catch (ExecutionTimeoutException e)
            {
                if (context.FailureReporting)
                    context.ReportCollector.Consume(new TestReport(TestReport.TYPE.INTERUPTED, e.LineNumber, e.Message));
            }
            catch (TestFailedException e)
            {
                if (context.FailureReporting)
                    context.ReportCollector.Consume(new TestReport(TestReport.TYPE.FAILURE, e.LineNumber, e.Message));
            }
            catch (TargetInvocationException e)
            {
                var baseException = e.GetBaseException();
                if (baseException is TestFailedException)
                {
                    var ex = baseException as TestFailedException;
                    if (ex != null && context.FailureReporting)
                        context.ReportCollector.Consume(new TestReport(TestReport.TYPE.FAILURE, ex.LineNumber, ex.Message));
                }
                else
                {
                    // unexpected exceptions
                    Godot.GD.PushError(baseException.Message);
                    Godot.GD.PushError(baseException.StackTrace);

                    StackTrace stack = new StackTrace(baseException, true);
                    var lineNumber = stack.FrameCount > 1 ? stack.GetFrame(1).GetFileLineNumber() : -1;
                    context.ReportCollector.Consume(new TestReport(TestReport.TYPE.ABORT, lineNumber, baseException.Message));
                }
            }
            catch (Exception e)
            {
                // unexpected exceptions
                Godot.GD.PushError(e.Message);
                Godot.GD.PushError(e.StackTrace);
                StackTrace stack = new StackTrace(e, true);
                var lineNumber = stack.FrameCount > 1 ? stack.GetFrame(1).GetFileLineNumber() : -1;
                context.ReportCollector.Consume(new TestReport(TestReport.TYPE.ABORT, lineNumber, e.Message));
            }
        }

        private async Task ExecuteStage(ExecutionContext context)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                var timeout = TimeSpan.FromMilliseconds(StageAttribute!.Timeout != -1 ? StageAttribute.Timeout : DefaultTimeout);
                //Godot.GD.PrintS("Execute", StageName);//, context.MethodArguments.Formated());
                object? obj = Method?.Invoke(context.TestSuite.Instance, context.MethodArguments);
                Task task = obj is Task ? (Task)obj : Task.Run(() => { });
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, tokenSource.Token));
                tokenSource.Cancel();
                if (completedTask == task)
                    // Very important in order to propagate exceptions
                    await task;
                else
                    throw new ExecutionTimeoutException($"The execution has timed out after {timeout.TotalMilliseconds}ms.", ExecutionLineNumber(context));
            }
        }

        public string StageName { get; private set; } = "";

        private bool IsAsync { get; set; }

        private bool IsTask { get; set; }

        private int DefaultTimeout { get; set; } = 30000;

        private MethodInfo? Method { get; set; } = null;

        public TestStageAttribute? StageAttribute { get; set; }

        private int ExecutionLineNumber(ExecutionContext context)
        {
            if (StageAttribute?.Line == -1)
                return context.CurrentTestCase?.Line ?? -1;
            return StageAttribute?.Line ?? -1;
        }
    }
}
