namespace GdUnit4.TestAdapter.Test;

using Api;

internal sealed class NoOpLogger : ITestEngineLogger
{
    public void SendMessage(LogLevel logLevel, string message)
    {
    }
}
