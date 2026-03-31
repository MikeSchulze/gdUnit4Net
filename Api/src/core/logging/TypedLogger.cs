// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using System;

using Api;

/// <summary>
///     An <see cref="ITestEngineLogger" /> that routes messages to active <see cref="LogCapture" /> scopes
///     registered for its type, and forwards all messages to the global logger.
/// </summary>
internal sealed class TypedLogger(Type type) : ITestEngineLogger
{
    void ITestEngineLogger.SendMessage(LogLevel logLevel, string message)
    {
        foreach (var capture in LogCapture.GetCaptures(type))
            capture.Capture(logLevel, message, type);

        switch (logLevel)
        {
            case LogLevel.Debug:
                TestEngineLogger.Global.LogDebug(message);
                break;
            case LogLevel.Informational:
                TestEngineLogger.Global.LogInfo(message);
                break;
            case LogLevel.Warning:
                TestEngineLogger.Global.LogWarning(message);
                break;
            case LogLevel.Error:
                TestEngineLogger.Global.LogError(message);
                break;
            default:
                TestEngineLogger.Global.LogInfo(message);
                break;
        }
    }
}
