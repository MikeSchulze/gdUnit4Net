// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using System.Reflection;

using Api;

/// <summary>
/// Extension which loads a Godot scene before each test and disposes it after, injecting
/// <see cref="ISceneRunner"/> and the typed scene node into test method parameters.
/// </summary>
/// <remarks>
/// <para>
/// The scene path can be set as a constructor default and/or overridden per-test via
/// <c>[TestCase("res://...")]</c>. The path string is consumed from
/// <see cref="IExtensionContext.TestCaseArguments"/> in <see cref="BeforeEach"/> and never
/// reaches the test method signature.
/// </para>
/// <para>
/// Use <see cref="RegisterExtensionAttribute"/> on a static field to register this extension
/// with constructor arguments:
/// <code>
/// [RegisterExtension]
/// private static readonly SceneRunnerExtension SceneRunner = new("res://scenes/player.tscn");
/// </code>
/// </para>
/// </remarks>
public sealed class SceneRunnerExtension : IBeforeEachCallback, IAfterEachCallback, IParameterResolver
{
    private const string RUNNER_KEY = "sceneRunner";

    private readonly string? defaultScenePath;
    private readonly bool verbose;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneRunnerExtension"/> class.
    /// </summary>
    /// <param name="defaultScenePath">
    /// Default scene path used when no path is supplied by a <c>[TestCase]</c> argument.
    /// </param>
    /// <param name="verbose">When <c>true</c>, the scene runner operates in verbose mode.</param>
    public SceneRunnerExtension(string? defaultScenePath, bool verbose = false)
    {
        this.defaultScenePath = defaultScenePath;
        this.verbose = verbose;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneRunnerExtension"/> class with no default scene path and
    /// verbose mode disabled.
    /// </summary>
    public SceneRunnerExtension()
        : this(null)
    {
    }

    /// <summary>
    /// Loads the scene (using the per-test path override or the constructor default) and stores
    /// the <see cref="ISceneRunner"/> in the context for later retrieval by
    /// <see cref="AfterEach"/> and <see cref="ResolveParameter"/>.
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

        var path = context.TestCaseArguments.OfType<string>().FirstOrDefault()
                   ?? defaultScenePath
                   ?? throw new InvalidOperationException(
                       $"[{context.TestCaseName}]: No scene path provided. " +
                       "Set a default via [RegisterExtension] or specify per-test via [TestCase(\"res://...\")].");

        var runner = ISceneRunner.Load(path, autoFree: true, verbose: verbose);
        context.Store(RUNNER_KEY, runner);
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
        context.Retrieve<ISceneRunner>(RUNNER_KEY)?.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <c>true</c> if the parameter is an <see cref="ISceneRunner"/> or an instance of
    /// the scene type returned by <see cref="ISceneRunner.Scene"/>.
    /// </summary>
    /// <param name="parameter">The parameter to check for resolution eligibility.</param>
    /// <param name="context">The extension context for the current test.</param>
    /// <returns><c>true</c> if this extension can resolve the parameter; otherwise, <c>false</c>.</returns>
    public bool SupportsParameter(ParameterInfo parameter, IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(context);

        if (parameter.ParameterType == typeof(ISceneRunner))
            return true;

        var scene = context.Retrieve<ISceneRunner>(RUNNER_KEY)?.Scene();
        return scene != null && parameter.ParameterType.IsInstanceOfType(scene);
    }

    /// <summary>
    /// Returns the <see cref="ISceneRunner"/> for <see cref="ISceneRunner"/>-typed parameters,
    /// or the scene node cast to the parameter's declared type.
    /// </summary>
    /// <param name="parameter">The parameter to resolve.</param>
    /// <param name="context">The extension context for the current test.</param>
    /// <returns>The resolved value to pass into the test method parameter.</returns>
    public object? ResolveParameter(ParameterInfo parameter, IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(context);

        var runner = context.Retrieve<ISceneRunner>(RUNNER_KEY)!;
        return parameter.ParameterType == typeof(ISceneRunner) ? runner : runner.Scene();
    }
}
