// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution.Exceptions;

using System;

#pragma warning disable CA1064
internal sealed class ExecutionTimeoutException : Exception
#pragma warning restore CA1064
{
    public ExecutionTimeoutException()
    {
    }

    public ExecutionTimeoutException(string message)
         : base(message)
    {
    }

    public ExecutionTimeoutException(string message, Exception innerException)
         : base(message, innerException)
    {
    }

    public ExecutionTimeoutException(string message, int line)
        : base(message)
        => LineNumber = line;

    public int LineNumber { get; private set; }
}
