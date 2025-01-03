namespace GdUnit4.Core.Commands;

using System;
using System.Net;
using System.Threading.Tasks;

using Godot;

public class TerminateGodotInstanceCommand : BaseCommand
{
    public override Task<Response> Execute()
    {
        Console.WriteLine("Terminating Godot instance.");
        var hint = Engine.IsEditorHint();
        (Engine.GetMainLoop() as SceneTree)?.Quit();
        return Task.FromResult(new Response
        {
            StatusCode = HttpStatusCode.OK,
            Payload = ""
        });
    }
}
