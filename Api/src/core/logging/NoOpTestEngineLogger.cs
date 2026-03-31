// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using Api;

/// <summary>Discards all log messages. Used as the default logger before one is registered.</summary>
internal sealed class NoOpTestEngineLogger : ITestEngineLogger
{
    internal static readonly NoOpTestEngineLogger Instance = new();

    private NoOpTestEngineLogger()
    {
    }

    void ITestEngineLogger.SendMessage(LogLevel logLevel, string message)
    {
    }
}
