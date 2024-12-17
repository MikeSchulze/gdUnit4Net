namespace GdUnit4.core.runners;

using System.Collections.Generic;

using Core;
using Core.Events;

internal class GodotProcessTestRunner : ITestRunner
{
    private readonly ITestEngineLogger logger;

    public GodotProcessTestRunner(ITestEngineLogger logger) => this.logger = logger;

    public void Dispose() => logger.LogWarning("Not implemented, GodotProcessTestRunner.Dispose");

    public void RunAndWait(ITestEventListener eventListener, IList<GdUnitTestCase> tests)
        => logger.LogWarning("Not implemented, GodotProcessTestRunner.RunAndWait");

    public void Cancel() => logger.LogWarning("Not implemented, GodotProcessTestRunner.Cancel");
}
