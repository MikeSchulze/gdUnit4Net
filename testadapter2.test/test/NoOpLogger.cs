namespace GdUnit4.TestAdapter.test;

using Api;

public sealed class NoOpLogger : ITestEngineLogger
{
    public void SendMessage(ITestEngineLogger.Level level, string message)
    {
    }
}
