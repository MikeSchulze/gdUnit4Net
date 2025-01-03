﻿namespace GdUnit4.Api;

using Godot;

public class GodotLogger : ITestEngineLogger
{
    public void SendMessage(ITestEngineLogger.Level level, string message)
    {
        switch (level)
        {
            case ITestEngineLogger.Level.Informational:
                GD.PrintS(message);
                break;
            case ITestEngineLogger.Level.Warning:
                GD.PrintS(message);
                break;
            case ITestEngineLogger.Level.Error:
                GD.PrintErr(message);
                break;
            default:
                GD.PrintS(message);
                break;
        }
    }
}
