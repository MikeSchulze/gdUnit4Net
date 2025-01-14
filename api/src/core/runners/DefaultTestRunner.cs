namespace GdUnit4.Core.Runners;

using Execution;

public sealed class DefaultTestRunner : BaseTestRunner
{
    internal DefaultTestRunner(ITestEngineLogger logger, TestEngineSettings settings)
        : base(new DirectCommandExecutor(), logger, settings)
    {
    }
}
