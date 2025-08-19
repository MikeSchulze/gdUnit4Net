// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

using System.Diagnostics;

using Constraints;

using Core.Execution.Exceptions;
using Core.Signals;

using Godot;

internal sealed class SignalAssert : AssertBase<GodotObject, ISignalConstraint>, ISignalAssert
{
    internal SignalAssert(GodotObject current)
        : base(current)
        => GodotSignalCollector.Instance.RegisterEmitter(current);

    // Is just a fake method that is called to register the monitor on the emitter, which is done in the constructor
    public ISignalConstraint StartMonitoring()
        => this;

    public async Task<ISignalConstraint> IsEmitted(string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);

        var lineNumber = new StackFrame(3, true).GetFileLineNumber();
        var isEmitted = await IsEmittedTask(signal, args).ConfigureAwait(true);
        if (!isEmitted)
            ThrowTestFailureReport(AssertFailures.IsEmitted(Current, signal, args), lineNumber);
        return this;
    }

    public async Task<ISignalConstraint> IsNotEmitted(string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);

        var lineNumber = new StackFrame(3, true).GetFileLineNumber();
        var isEmitted = await IsEmittedTask(signal, args).ConfigureAwait(true);
        if (isEmitted)
            ThrowTestFailureReport(AssertFailures.IsNotEmitted(Current, signal, args), lineNumber);
        return this;
    }

    public ISignalConstraint IsSignalExists(string signal)
    {
        _ = IsNotNull();
        if (!Current!.HasSignal(signal))
            ThrowTestFailureReport(AssertFailures.IsSignalExists(Current, signal), Current, signal);
        return this;
    }

    public ISignalConstraint IsCountEmitted(int expectedCount, string signal, params Variant[] args)
    {
        _ = IsNotNull();
        _ = IsSignalExists(signal);
        var count = GodotSignalCollector.Instance.Count(Current!, signal, args);
        if (count != expectedCount)
            ThrowTestFailureReport($"Expecting emitted count is {expectedCount} but was {count}", Current, signal);
        return this;
    }

    private async Task<bool> IsEmittedTask(string signal, params Variant[] args)
        => await GodotSignalCollector.Instance.IsEmitted(Current!, signal, args).ConfigureAwait(true);

    private void ThrowTestFailureReport(string message, int lineNumber)
    {
        CurrentFailureMessage = CustomFailureMessage ?? message;
        throw new TestFailedException(CurrentFailureMessage, lineNumber);
    }
}
