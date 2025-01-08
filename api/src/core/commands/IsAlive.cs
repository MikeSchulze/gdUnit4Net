namespace GdUnit4.Core.Commands;

using System.Net;
using System.Threading.Tasks;

using Events;

public class IsAlive : BaseCommand
{
    public override Task<Response> Execute(ITestEventListener testEventListener) => Task.FromResult<Response>(new Response
    {
        StatusCode = HttpStatusCode.OK,
        Payload = "alive: true"
    });
}
