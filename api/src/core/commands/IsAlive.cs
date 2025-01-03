namespace GdUnit4.Core.Commands;

using System.Net;
using System.Threading.Tasks;

public class IsAlive : BaseCommand
{
    public override Task<Response> Execute() => Task.FromResult<Response>(new Response
    {
        StatusCode = HttpStatusCode.OK,
        Payload = "alive: true"
    });
}
