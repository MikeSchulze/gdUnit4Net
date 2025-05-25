// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Execution.Exceptions;

using System;

internal sealed class ExecutionTimeoutException : Exception
{
    public ExecutionTimeoutException(string message, int line)
        : base(message)
        => LineNumber = line;

    public int LineNumber
    {
        get;
        private set;
    }
}
