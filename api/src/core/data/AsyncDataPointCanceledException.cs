namespace GdUnit4.Core.Data;

using System;

internal sealed class AsyncDataPointCanceledException : OperationCanceledException
{
    public AsyncDataPointCanceledException(OperationCanceledException baseException, string stackTrace) : base(baseException.Message, baseException.CancellationToken)
        => StackTrace = stackTrace;

    public override string StackTrace { get; }
}
