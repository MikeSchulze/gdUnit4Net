namespace GdUnit4.core.runners;

using System.Collections.Generic;

using Core;
using Core.Events;

internal class GodotProcessTestRunner : ITestRunner
{
    private readonly IGdUnitLogger logger;

    public GodotProcessTestRunner(IGdUnitLogger logger) => this.logger = logger;

    public void Dispose() => logger.SendMessage(IGdUnitLogger.Level.Warning, "Not implemented, GodotProcessTestRunner.Dispose");

    public void RunAndWait(ITestEventListener eventListener, IList<GdUnitTestCase> tests)
        => logger.SendMessage(IGdUnitLogger.Level.Warning, "Not implemented, GodotProcessTestRunner.RunAndWait");

    public void Cancel() => logger.SendMessage(IGdUnitLogger.Level.Warning, "Not implemented, GodotProcessTestRunner.Cancel");
}
