// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using System.Diagnostics;

using Api;

internal sealed class ScopeLogger(ITestEngineLogger logger, string scopeId, string? source = null) : ITestEngineLogger
{
    public string ScopeId => scopeId;

    private string Tag => source is null ? $"[{scopeId}]" : $"[{scopeId}] {source,-6}";

    void ITestEngineLogger.SendMessage(LogLevel logLevel, string message)
    {
        var formatted = source is null ? $"{Tag} {message}" : $"{Tag}: {message}";
        switch (logLevel)
        {
            case LogLevel.Debug:
                logger.LogDebug(formatted);
                break;
            case LogLevel.Warning:
                logger.LogWarning(formatted);
                break;
            case LogLevel.Error:
                logger.LogError(formatted);
                break;
            case LogLevel.Informational:
            default:
                logger.LogInfo(formatted);
                break;
        }
    }

    public void Output(object sender, DataReceivedEventArgs args)
    {
        if (args.Data?.Trim() is { Length: > 0 } m)
            logger.LogInfo($"{Tag}/out: {m}");
    }

    public void Error(object sender, DataReceivedEventArgs args)
    {
        if (args.Data?.Trim() is { Length: > 0 } m)
            logger.LogInfo($"{Tag}/err: {m}");
    }

    public ScopeLogger WithSource(string sourceId) => new(logger, scopeId, sourceId);
}
