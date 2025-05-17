// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Commands;

using System;
using System.Net;
using System.Threading.Tasks;

using Api;

using Godot;

using Newtonsoft.Json;

/// <summary>
///     Command to safely terminate a running Godot instance.
/// </summary>
public class TerminateGodotInstanceCommand : BaseCommand
{
    [JsonConstructor]
    public TerminateGodotInstanceCommand()
    {
    }

    /// <summary>
    ///     Executes the termination command, shutting down the Godot instance.
    /// </summary>
    /// <remarks>
    ///     This command gracefully closes the Godot scene tree and engine.
    /// </remarks>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public override Task<Response> Execute(ITestEventListener testEventListener)
    {
        Console.WriteLine("Terminating Godot instance.");
        var hint = Engine.IsEditorHint();
        (Engine.GetMainLoop() as SceneTree)?.Quit();
        return Task.FromResult(new Response
        {
            StatusCode = HttpStatusCode.OK,
            Payload = string.Empty
        });
    }
}
