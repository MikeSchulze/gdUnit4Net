namespace GdUnit4.Core.Runners;

using Commands;

public sealed class DirectTestRunner : BaseTestRunner
{
    internal DirectTestRunner(ITestEngineLogger logger) : base(new DirectCommandExecutor(), logger)
    {
    }
}
