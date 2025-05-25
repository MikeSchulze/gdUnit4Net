// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

// ReSharper disable once CheckNamespace
namespace GdUnit4;

using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Asserts;

using Core.Execution.Exceptions;
using Core.Extensions;
using Core.Signals;

using Godot;

public static class GodotAwaiterExtension
{
    /// <summary>
    ///     Waits for given signal is emitted.
    ///     <example>
    ///         <code>
    ///     // Waits for signal "mySignal" is emitted by the scene.
    ///     await node.AwaitSignal("mySignal");
    /// </code>
    ///     </example>
    /// </summary>
    /// <param name="emitter">The signal emitter.</param>
    /// <param name="signal">The name of the signal to wait.</param>
    /// <param name="args">An optional set of signal arguments.</param>
    /// <returns>Task to wait.</returns>
    public static async Task<ISignalAssert> AwaitSignal(this Node emitter, string signal, params Variant[] args)
        => await new SignalAssert(emitter).IsEmitted(signal, args).ConfigureAwait(true);

    /// <summary>
    ///     Adds a timeout to the signal assertion task.
    /// </summary>
    /// <param name="task">The signal assertion task to apply the timeout to.</param>
    /// <param name="timeoutMillis">The timeout duration in milliseconds.</param>
    /// <returns>The original signal assertion if it completes before the timeout; otherwise, the canceled task result.</returns>
    /// <remarks>
    ///     This method allows tests to specify a maximum wait time for signals.
    ///     If the timeout is reached, the signal wait is canceled and the test continues.
    ///     This is useful for preventing tests from hanging indefinitely when expected signals are not emitted.
    /// </remarks>
    public static async Task<TVariant> WithTimeout<TVariant>(this Task<TVariant> task, int timeoutMillis)
        where TVariant : IGdUnitAwaitable
    {
        Debug.Assert(task != null, nameof(task) + " != null");

        using var timeoutCts = new CancellationTokenSource();
        try
        {
            var timeoutTask = Task.Delay(timeoutMillis, timeoutCts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(true);
            if (completedTask == task)
                return await task.ConfigureAwait(false);

            var data = Thread.GetData(Thread.GetNamedDataSlot(GodotSignalCollector.SIGNAL_CANCELLATION_TOKEN_SLOT_NAME));
            if (data is CancellationTokenSource { IsCancellationRequested: false } signalCancelToken)
                signalCancelToken.Cancel();
            else
            {
                var lineNumber = GdUnitExtensions.GetWithTimeoutLineNumber();
                throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
            }

            return await task.ConfigureAwait(false);
        }
        finally
        {
            if (!timeoutCts.IsCancellationRequested)
                timeoutCts.Cancel();
        }
    }
}
