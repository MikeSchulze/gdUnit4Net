namespace GdUnit4.Core.Runners;

using Commands;

public sealed class DefaultTestRunner : BaseTestRunner
{
    internal DefaultTestRunner(ITestEngineLogger logger, TestEngineSettings settings)
        : base(new DirectCommandExecutor(), logger, settings)
    {
    }
}
