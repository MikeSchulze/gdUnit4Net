namespace GdUnit4.core;

internal interface IGdUnitLogger
{
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
    public void SendMessage(Level level, string message);
}
