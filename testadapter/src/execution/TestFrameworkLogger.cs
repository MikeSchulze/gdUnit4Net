namespace GdUnit4.TestAdapter.Execution;

using System;

using core;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

internal class TestFrameworkLogger : IGdUnitLogger
{
    private readonly IFrameworkHandle framework;

    public TestFrameworkLogger(IFrameworkHandle framework) => this.framework = framework;


    public void SendMessage(IGdUnitLogger.Level level, string message)
    {
        if (Enum.TryParse(level.ToString(), out TestMessageLevel testLogLevel))
            framework.SendMessage(testLogLevel, message);
        else
            framework.SendMessage(TestMessageLevel.Error, $"Can't parse logging level {level.ToString()}");
    }
}
