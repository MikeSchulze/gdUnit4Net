// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.Logging;

using System.Collections.Generic;

using Api;

using GdUnit4.Core.Logging;

[TestSuite]
public class TypedLoggerTest
{
    #region Log-source stubs

    // Subject owns its logger, mirroring real production code.
    private class Subject
    {
        // ReSharper disable once UnusedMember.Local
        internal static readonly ITestEngineLogger _ = LoggerFactory.GetLogger<Subject>();
    }

    // Records every SendMessage call; used as the backing logger so tests
    // can verify the level TypedLogger forwards without touching the global logger.
    private sealed class BackingTestLogger : ITestEngineLogger
    {
        public List<LogEntry> Entries { get; } = [];

        void ITestEngineLogger.SendMessage(LogLevel logLevel, string message)
            => Entries.Add(new LogEntry(logLevel, message, typeof(BackingTestLogger)));
    }

    #endregion

    #region Forwarding without active capture

    [TestCase]
    public void SendMessage_WithoutCapture_ErrorIsForwardedAtOriginalLevel()
    {
        var backingLogger = new BackingTestLogger();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogError("boom");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Error, "boom", typeof(BackingTestLogger)));
    }

    [TestCase]
    public void SendMessage_WithoutCapture_WarningIsForwardedAtOriginalLevel()
    {
        var backingLogger = new BackingTestLogger();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogWarning("caution");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Warning, "caution", typeof(BackingTestLogger)));
    }

    [TestCase]
    public void SendMessage_WithoutCapture_InformationalIsForwardedAtOriginalLevel()
    {
        var backingLogger = new BackingTestLogger();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogInfo("info");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Informational, "info", typeof(BackingTestLogger)));
    }

    #endregion

    #region Forwarding with active capture

    [TestCase]
    public void SendMessage_WithActiveCapture_ErrorIsDemotedToInformationalInBackingLogger()
    {
        var backingLogger = new BackingTestLogger();
        using var capture = LogCapture.Watch<Subject>();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogError("boom");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Informational, "[Captured Error] boom", typeof(BackingTestLogger)));
    }

    [TestCase]
    public void SendMessage_WithActiveCapture_WarningIsDemotedToInformationalInBackingLogger()
    {
        var backingLogger = new BackingTestLogger();
        using var capture = LogCapture.Watch<Subject>();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogWarning("caution");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Informational, "[Captured Warning] caution", typeof(BackingTestLogger)));
    }

    [TestCase]
    public void SendMessage_WithActiveCapture_InformationalIsNotDemotedInBackingLogger()
    {
        var backingLogger = new BackingTestLogger();
        using var capture = LogCapture.Watch<Subject>();
        ITestEngineLogger logger = new TypedLogger(typeof(Subject), backingLogger);

        logger.LogInfo("info");

        AssertThat(backingLogger.Entries)
            .ContainsExactly(new LogEntry(LogLevel.Informational, "info", typeof(BackingTestLogger)));
    }

    #endregion
}
