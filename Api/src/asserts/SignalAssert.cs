namespace GdUnit4.Asserts;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Core.Execution.Exceptions;
using Core.Signals;

using Godot;

public sealed class SignalAssert : AssertBase<GodotObject>, ISignalAssert
{
    public SignalAssert(GodotObject current) : base(current)
        => GodotSignalCollector.Instance.RegisterEmitter(current);

    // Is just a dummy method that is called to register the monitor on the emitter, which is done in the constructor
    public ISignalAssert StartMonitoring()
        => this;

    public async Task<ISignalAssert> IsEmitted(string signal, params Variant[] args)
    {
        IsNotNull();
        IsSignalExists(signal);

        var lineNumber = new StackFrame(3, true).GetFileLineNumber();
        var isEmitted = await IsEmittedTask(signal, args);
        if (!isEmitted)
            ThrowTestFailureReport(AssertFailures.IsEmitted(Current, signal, args), lineNumber);
        return this;
    }

    public async Task<ISignalAssert> IsNotEmitted(string signal, params Variant[] args)
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

    public ISignalAssert IsCountEmitted(int expectedCount, string signal, params Variant[] args)
    {
        IsNotNull();
        IsSignalExists(signal);
        var count = GodotSignalCollector.Instance.Count(Current!, signal, args);
        if (count != expectedCount)
            ThrowTestFailureReport($"Expecting emitted count is {expectedCount} but was {count}", Current, signal);
        return this;
    }

    private async Task<bool> IsEmittedTask(string signal, params Variant[] args)
    {
        var signalCancellationToken = new CancellationTokenSource();
        Thread.SetData(Thread.GetNamedDataSlot("SignalCancellationToken"), signalCancellationToken);
        return await Task.Run(() => GodotSignalCollector.Instance.IsEmitted(signalCancellationToken, Current!, signal, args), signalCancellationToken.Token);
    }

    private void ThrowTestFailureReport(string message, int lineNumber)
    {
        CurrentFailureMessage = CustomFailureMessage ?? message;
        throw new TestFailedException(CurrentFailureMessage, lineNumber);
    }
}

public static class SignalAssertExtensions
{
    public static async Task<ISignalAssert> WithTimeout(this Task<ISignalAssert> task, int timeoutMillis)
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);
        try
        {
            var timeoutTask = Task.Delay(timeoutMillis, timeoutCts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            if (completedTask == task)
                return await task.ConfigureAwait(false);

            var data = Thread.GetData(Thread.GetNamedDataSlot("SignalCancellationToken"));
            if (data is CancellationTokenSource cancelToken)
                cancelToken.Cancel();
            return await task.ConfigureAwait(false);
        }
        finally
        {
            timeoutCts.Cancel();
            linkedCts.Cancel();
        }
    }
}
