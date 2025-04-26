namespace GdUnit4.Core.Data;

using System;

internal sealed class AsyncDataPointCanceledException : Exception
{
    public AsyncDataPointCanceledException(string message, string stackTrace) : base(message) => StackTrace = stackTrace;

    public override string StackTrace { get; }
}
