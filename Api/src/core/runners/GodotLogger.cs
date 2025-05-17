// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using Api;

using Godot;

public sealed class GodotLogger : ITestEngineLogger
{
    public void SendMessage(ITestEngineLogger.Level level, string message)
    {
        switch (level)
        {
            case ITestEngineLogger.Level.INFORMATIONAL:
                GD.PrintS(message);
                break;
            case ITestEngineLogger.Level.WARNING:
                GD.PrintS(message);
                break;
            case ITestEngineLogger.Level.ERROR:
                GD.PrintErr(message);
                break;
            default:
                GD.PrintS(message);
                break;
        }
    }
}
