// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

using System.Runtime.CompilerServices;

/// <summary>
///     Abstract base class for test engine logging functionality.
///     Provides standardized logging methods for different severity levels
///     and defines the logging level hierarchy.
/// </summary>
public interface ITestEngineLogger
{
    /// <summary>
    ///     Defines the available logging severity levels.
    /// </summary>
    public enum Level
    {
        /// <summary>
        ///     Informational message.
        /// </summary>
        Informational = 0,

        /// <summary>
        ///     Warning message.
        /// </summary>
        Warning = 1,

        /// <summary>
        ///     Error message.
        /// </summary>
        Error = 2
    }

    /// <summary>
    ///     Sends a message to the enabled loggers.
    /// </summary>
    /// <param name="level">Level of the message.</param>
    /// <param name="message">The message to be sent.</param>
    protected void SendMessage(Level level, string message);

    /// <summary>
    ///     Logs an informational message.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogInfo(string message) => SendMessage(Level.Informational, message);

    /// <summary>
    ///     Logs a warning message.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogWarning(string message) => SendMessage(Level.Warning, message);

    /// <summary>
    ///     Logs an error message.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LogError(string message) => SendMessage(Level.Error, message);
}
