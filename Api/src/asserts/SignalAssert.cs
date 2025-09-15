// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System.Diagnostics;

using Constraints;

using Core.Execution.Exceptions;
using Core.Signals;

using Godot;

/// <inheritdoc cref="ISignalAssert" />
public sealed class SignalAssert : AssertBase<GodotObject, ISignalConstraint>, ISignalAssert
{
    internal SignalAssert(GodotObject current)
        : base(current)
        => GodotSignalCollector.Instance.RegisterEmitter(current);

    /// <inheritdoc />
    public ISignalConstraint StartMonitoring()
        => this;

    /// <inheritdoc />
    public Task<ISignalConstraint> IsEmitted(string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);

        var stackTrace = new StackTrace(true);
        var signalCancellationToken = new CancellationTokenSource();
        var continuation = IsEmittedTask(signalCancellationToken, signal, args)
            .ContinueWith<ISignalConstraint>(
                antecedent =>
                {
                    var isEmitted = antecedent.Result;
                    if (!isEmitted)
                        ThrowTestFailureReport(AssertFailures.IsEmitted(Current, signal, args), stackTrace);
                    return this;
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

        GodotSignalCollector.TaskCancellations[continuation.Id] = signalCancellationToken;

        // Cleanup continuation using captured taskId
        _ = continuation.ContinueWith(
            _ =>
            {
                if (GodotSignalCollector.TaskCancellations.TryRemove(continuation.Id, out var cts))
                    cts.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return continuation;
    }

    /// <inheritdoc />
    public Task<ISignalConstraint> IsNotEmitted(string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);

        var stackTrace = new StackTrace(true);
        var signalCancellationToken = new CancellationTokenSource();
        var continuation = IsEmittedTask(signalCancellationToken, signal, args)
            .ContinueWith<ISignalConstraint>(
                antecedent =>
                {
                    var isEmitted = antecedent.Result;
                    if (isEmitted)
                        ThrowTestFailureReport(AssertFailures.IsNotEmitted(Current, signal, args), stackTrace);
                    return this;
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

        GodotSignalCollector.TaskCancellations[continuation.Id] = signalCancellationToken;

        // Cleanup continuation using captured taskId
        _ = continuation.ContinueWith(
            _ =>
            {
                if (GodotSignalCollector.TaskCancellations.TryRemove(continuation.Id, out var cts))
                    cts.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
        return continuation;
    }

    /// <inheritdoc />
    public ISignalConstraint IsSignalExists(string signal)
    {
        _ = IsNotNull();
        if (!Current!.HasSignal(signal))
            ThrowTestFailureReport(AssertFailures.IsSignalExists(Current, signal), Current, signal);
        return this;
    }

    /// <inheritdoc />
    public ISignalConstraint IsCountEmitted(int expectedCount, string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);
        var count = GodotSignalCollector.Instance.Count(Current!, signal, args);
        if (count != expectedCount)
            ThrowTestFailureReport($"Expecting emitted count is {expectedCount} but was {count}", Current, signal);
        return this;
    }

    private Task<bool> IsEmittedTask(CancellationTokenSource cancellationTokenSource, string signal, params Variant[] args)
        => GodotSignalCollector.Instance.IsEmitted(cancellationTokenSource, Current!, signal, args);

    private void ThrowTestFailureReport(string message, StackTrace stackTrace)
    {
        CurrentFailureMessage = CustomFailureMessage ?? message;
        throw new TestFailedException(CurrentFailureMessage, stackTrace);
    }
}
