// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Asserts;

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
///     Extension methods for signal assertions to provide additional functionality.
/// </summary>
public static class SignalAssertExtensions
{
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
    [SuppressMessage("Performance", "CA1849", Justification = "Call async methods when in an async method")]
    public static async Task<ISignalAssert> WithTimeout(this Task<ISignalAssert> task, int timeoutMillis)
    {
        Debug.Assert(task != null, nameof(task) + " != null");
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);
        try
        {
            var timeoutTask = Task.Delay(timeoutMillis, timeoutCts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask).ConfigureAwait(true);
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
