namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.Collections.Generic;

using Api;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

/// <summary>
///     Adapts ITestEngineLogger to work with Visual Studio's IMessageLogger.
///     Provides test logging functionality within the VS test platform.
/// </summary>
public class Logger : ITestEngineLogger
{
    private static readonly Dictionary<ITestEngineLogger.Level, TestMessageLevel> LevelMap = new()
    {
        { ITestEngineLogger.Level.Informational, TestMessageLevel.Informational },
        { ITestEngineLogger.Level.Warning, TestMessageLevel.Warning },
        { ITestEngineLogger.Level.Error, TestMessageLevel.Error }
    };

    private readonly IMessageLogger delegator;

    /// <summary>
    ///     Initializes a new instance of the TestLogger class.
    /// </summary>
    /// <param name="delegator">The VS test platform message logger to delegate to</param>
    public Logger(IMessageLogger delegator)
        => this.delegator = delegator ?? throw new ArgumentNullException(nameof(delegator));

    /// <summary>
    ///     Sends a message to the VS test platform logger with the appropriate level.
    /// </summary>
    /// <param name="level">The severity level of the message</param>
    /// <param name="message">The message to log</param>
    public void SendMessage(ITestEngineLogger.Level level, string message)
    {
        if (LevelMap.TryGetValue(level, out var testLogLevel))
            delegator.SendMessage(testLogLevel, message);
        else
            delegator.SendMessage(TestMessageLevel.Error, $"Can't parse logging level {level.ToString()}");
    }
}
