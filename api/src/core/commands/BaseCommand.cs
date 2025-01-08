namespace GdUnit4.Core.Commands;

using System.Threading.Tasks;

using Events;

public abstract class BaseCommand
{
    public abstract Task<Response> Execute(ITestEventListener testEventListener);
}
