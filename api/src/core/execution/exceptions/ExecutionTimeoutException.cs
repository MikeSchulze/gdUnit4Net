namespace GdUnit4.Core.Execution.Exceptions;

using System;

internal sealed class ExecutionTimeoutException : Exception
{
    public ExecutionTimeoutException(string message, int line) : base(message)
        => LineNumber = line;

    public int LineNumber
    {
        get;
        private set;
    }
}
