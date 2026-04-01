namespace GdUnit4.Tests.Core.Commands;

using System.Threading.Tasks;

using Api;

internal sealed class ExampleTestExtension : IBeforeAllCallback, IBeforeEachCallback, IAfterEachCallback, IAfterAllCallback
{
    private static ITestEngineLogger Logger => LoggerFactory.GetLogger<ExampleTestExtension>();

    public Task AfterAll(IExtensionContext context)
    {
        Logger.LogInfo("Extension.AfterAll");
        return Task.CompletedTask;
    }

    public Task AfterEach(IExtensionContext context)
    {
        Logger.LogInfo("Extension.AfterEach");
        return Task.CompletedTask;
    }

    public Task BeforeAll(IExtensionContext context)
    {
        Logger.LogInfo("Extension.BeforeAll");
        return Task.CompletedTask;
    }

    public Task BeforeEach(IExtensionContext context)
    {
        Logger.LogInfo("Extension.BeforeEach");
        return Task.CompletedTask;
    }
}
