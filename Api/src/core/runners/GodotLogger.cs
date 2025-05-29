// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using Api;

using Godot;

internal sealed class GodotLogger : ITestEngineLogger
{
    /// <inheritdoc />
    public void SendMessage(LogLevel logLevel, string message)
    {
        switch (logLevel)
        {
            case LogLevel.Informational:
                GD.PrintS(message);
                break;
            case LogLevel.Warning:
                GD.PrintS(message);
                break;
            case LogLevel.Error:
                GD.PrintErr(message);
                break;
            default:
                GD.PrintS(message);
                break;
        }
    }
}
