namespace GdUnit4.Core.Commands;

using System.Net;
using System.Threading.Tasks;

using Api;

using Newtonsoft.Json;

/// <summary>
///     Command to check if the test engine is alive and responding.
/// </summary>
public class IsAlive : BaseCommand
{
    [JsonConstructor]
    private IsAlive() { }

    /// <summary>
    ///     Executes a health check and returns an "alive" status response.
    /// </summary>
    public override Task<Response> Execute(ITestEventListener testEventListener) => Task.FromResult<Response>(new Response
    {
        StatusCode = HttpStatusCode.OK,
        Payload = "alive: true"
    });
}
