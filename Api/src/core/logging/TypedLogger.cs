// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using Api;

/// <summary>
///     An <see cref="ITestEngineLogger" /> that routes messages to active <see cref="LogCapture" /> scopes
///     registered for its type, and forwards all messages to the backing backingLogger.
/// </summary>
/// <param name="type">The class this logger is scoped to.</param>
/// <param name="backingLogger">The backing logger to forward messages to after dispatching to any active captures.</param>
internal sealed class TypedLogger(Type type, ITestEngineLogger backingLogger) : ITestEngineLogger
{
    void ITestEngineLogger.SendMessage(LogLevel logLevel, string message)
    {
        var captureSet = LogCapture.GetCaptures(type);
        foreach (var capture in captureSet)
            capture.Capture(logLevel, message, type);

        // When a LogCapture is active the message is intentionally being tested.
        // Demote Error/Warning to Informational so the message stays visible in test output
        // but does not leak into VSTest's error channel and falsely fail the run.
        // Prefix the forwarded message so the original severity remains visible in raw output.
        if (captureSet.Count > 0 && logLevel is LogLevel.Error or LogLevel.Warning)
        {
            backingLogger.LogInfo($"[Captured {logLevel}] {message}");
            return;
        }

        switch (logLevel)
        {
            case LogLevel.Debug:
                backingLogger.LogDebug(message);
                break;
            case LogLevel.Informational:
                backingLogger.LogInfo(message);
                break;
            case LogLevel.Warning:
                backingLogger.LogWarning(message);
                break;
            case LogLevel.Error:
                backingLogger.LogError(message);
                break;
            default:
                backingLogger.LogInfo(message);
                break;
        }
    }
}
