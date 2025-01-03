namespace GdUnit4.Core.Commands;

using System.Net;
using System.Threading.Tasks;

using Api;

using Newtonsoft.Json;

public class ExecuteTeardownTestSuiteCommand : BaseCommand
{
    [JsonConstructor]
    private ExecuteTeardownTestSuiteCommand() { }

    public ExecuteTeardownTestSuiteCommand(TestSuiteNode testSuite) => Suite = testSuite;
    [JsonProperty] private TestSuiteNode Suite { get; set; } = null!;

    public override Task<Response> Execute()
    {
        ExecutionContextStore.GetContext(Suite.Id)?.Dispose();
        ExecutionContextStore.RemoveContext(Suite.Id);

        return Task.FromResult(new Response
        {
            StatusCode = HttpStatusCode.OK,
            Payload = $"Test suite {Suite.ManagedType} loaded"
        });
    }
}
