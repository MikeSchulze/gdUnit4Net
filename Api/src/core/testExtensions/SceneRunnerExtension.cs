// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using Api;

using Godot;

/// <summary>
/// Extension which loads a Godot scene before each test and disposes it after, injecting
/// <see cref="ISceneRunner"/> and the typed scene node into test method parameters.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="RegisterExtensionAttribute"/> on a static field to register this extension
/// with constructor arguments:
/// <code>
/// [RegisterExtension]
/// private static readonly SceneRunnerExtension SceneRunner = new("res://scenes/player.tscn");
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="T">The expected type of scene to be loaded by this extension.</typeparam>
public sealed class SceneRunnerExtension<T> : ContextStoreParameterResolver, IBeforeEachCallback, IAfterEachCallback
    where T : Node
{
    private const string RUNNER_KEY = "sceneRunner";

    private const string SCENE_KEY = "scene";

    private readonly string scenePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneRunnerExtension{T}"/> class.
    /// </summary>
    /// <param name="scenePath">
    /// Default scene path used when no path is supplied by a <c>[TestCase]</c> argument.
    /// </param>
    public SceneRunnerExtension(string scenePath) => this.scenePath = scenePath;

    /// <summary>
    /// Loads the scene (using the per-test path override or the constructor default) and stores
    /// the <see cref="ISceneRunner"/> in the context for later retrieval by
    /// <see cref="AfterEach"/> and parameter injection.
    /// </summary>
    /// <param name="context">The extension context for the current test.</param>
    /// <returns>A completed task.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership is transferred to context storage; AfterEach is responsible for disposal.")]
    public Task BeforeEach(IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sceneRunner = ISceneRunner.Load(scenePath, autoFree: true);
        context.GetStore().Add(RUNNER_KEY, sceneRunner);
        var scene = sceneRunner.Scene() as T;
        context.GetStore().Add(SCENE_KEY, scene!);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Disposes the <see cref="ISceneRunner"/> that was created in <see cref="BeforeEach"/>.
    /// </summary>
    /// <param name="context">The extension context for the current test.</param>
    /// <returns>A completed task.</returns>
    public Task AfterEach(IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var sceneRunner = context.GetStore().Remove<ISceneRunner>(RUNNER_KEY);
        _ = context.GetStore().Remove<T>(SCENE_KEY);
        sceneRunner?.Dispose();
        return Task.CompletedTask;
    }
}
