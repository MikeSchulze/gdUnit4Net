using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;


namespace GdUnit4.Asserts
{
    using Exceptions;
    using Core.Signals;

    internal sealed partial class SignalAssert : AssertBase<Godot.GodotObject>, ISignalAssert
    {
        public SignalAssert(Godot.GodotObject current) : base(current)
        {
            GodotSignalCollector.Instance.RegisterEmitter(current);
        }

        public async Task<ISignalAssert> IsEmitted(string signal, params Godot.Variant[] args)
        {
            IsNotNull();
            IsSignalExists(signal);

            var lineNumber = new StackFrame(3, true).GetFileLineNumber();
            var isEmitted = await IsEmittedTask(signal, args);
            if (!isEmitted)
                ThrowTestFailureReport(AssertFailures.IsEmitted(Current, signal, args), lineNumber);
            return this;
        }

        public async Task<ISignalAssert> IsNotEmitted(string signal, params Godot.Variant[] args)
        {
            IsNotNull();
            IsSignalExists(signal);

            var lineNumber = new StackFrame(3, true).GetFileLineNumber();
            var isEmitted = await IsEmittedTask(signal, args);
            if (isEmitted)
                ThrowTestFailureReport(AssertFailures.IsNotEmitted(Current, signal, args), lineNumber);
            return this;
        }

        public ISignalAssert IsSignalExists(string signal)
        {
            IsNotNull();
            if (!Current!.HasSignal(signal))
                ThrowTestFailureReport(AssertFailures.IsSignalExists(Current, signal), Current, signal);
            return this;
        }

        public ISignalAssert IsCountEmitted(int expectedCount, string signal, params Godot.Variant[] args)
        {
            IsNotNull();
            IsSignalExists(signal);
            var count = GodotSignalCollector.Instance.Count(Current!, signal, args);
            if (count != expectedCount)
                ThrowTestFailureReport($"Expecting emitted count is {expectedCount} but was {count}", Current, signal);
            return this;
        }

        private async Task<bool> IsEmittedTask(string signal, params Godot.Variant[] args)
        {
            using var SignalCancellationToken = new CancellationTokenSource();
            Thread.SetData(Thread.GetNamedDataSlot("SignalCancellationToken"), SignalCancellationToken);
            return await Task.Run<bool>(() => GodotSignalCollector.Instance.IsEmitted(SignalCancellationToken, Current!, signal, args), SignalCancellationToken.Token);
        }

        private void ThrowTestFailureReport(string message, int lineNumber)
        {
            CurrentFailureMessage = CustomFailureMessage ?? message;
            throw new TestFailedException(CurrentFailureMessage, 0, lineNumber);
        }
    }


    internal static class SignalAssertTaskExtension
    {
        public static async Task<ISignalAssert> WithTimeout(this Task<ISignalAssert> task, int timeoutMillis)
        {
            using (var token = new CancellationTokenSource())
            {
                var wrapperTask = Task.Run(async () => await task.ConfigureAwait(false));
                var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
                token.Cancel();
                if (completedTask == wrapperTask)
                    return await task.ConfigureAwait(false);
                else
                {
                    var data = Thread.GetData(Thread.GetNamedDataSlot("SignalCancellationToken"));
                    if (data is CancellationTokenSource cancelToken)
                        cancelToken.Cancel();
                    return await task.ConfigureAwait(false);
                }
            }
        }
    }
}
