// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Data;

using System;

public sealed class AsyncDataPointCanceledException : Exception
{
    public AsyncDataPointCanceledException(string message, string stackTrace)
        : base(message) => StackTrace = stackTrace;

    private AsyncDataPointCanceledException() => StackTrace = "";

    public override string StackTrace { get; }
}
