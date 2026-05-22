// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text
namespace GdUnit4.Core.TestExtensions;

using Api;

/// <summary>
/// Extension which loads a Godot scene before each test and disposes it after, injecting
/// <see cref="ISceneRunner"/> and the typed scene node into test method parameters.
/// </summary>
/// <remarks>
/// <para>
/// Register this extension with a default scene path using <see cref="RegisterExtensionAttribute"/> on a static field:
/// <code>
/// [RegisterExtension]
/// private static readonly SceneRunnerExtension SceneRunner = new("res://scenes/player.tscn");
/// </code>
/// </para>
/// <para>
/// Or register without a default path using <see cref="ExtendWithAttribute{T}"/> on the test class,
/// and provide a per-test scene path via <c>[TestCase("res://...")]</c>:
/// <code>
/// [TestSuite]
/// [ExtendWith&lt;SceneRunnerExtension&gt;]
/// public sealed class MyTest
/// {
///     [TestCase("res://scenes/player.tscn")]
///     public void Test(ISceneRunner runner, PlayerScene player) { }
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class SceneRunnerExtension : IBeforeEachCallback, IAfterEachCallback, IParameterResolver
{
    private const string RUNNER_KEY = "sceneRunner";

    private readonly string? defaultScenePath;

    private readonly bool verbose;

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneRunnerExtension"/> class with no default scene path.
    /// Each test must supply a scene path via <c>[TestCase("res://...")]</c>.
    /// </summary>
    public SceneRunnerExtension()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SceneRunnerExtension"/> class with a default scene path.
    /// </summary>
    /// <param name="defaultScenePath">
    /// Default scene path used when no path is supplied by a <c>[TestCase]</c> argument.
    /// </param>
    /// <param name="verbose">Whether to enable verbose logging on the scene runner.</param>
    public SceneRunnerExtension(string defaultScenePath, bool verbose = false)
    {
        this.defaultScenePath = defaultScenePath;
        this.verbose = verbose;
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Ownership transferred to context store; AfterEach is responsible for disposal.")]
    public Task BeforeEach(IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var path = context.TestCaseArguments.OfType<string>().FirstOrDefault()
                   ?? defaultScenePath
                   ?? throw new InvalidOperationException(
                       "No scene path provided. Set a default via [RegisterExtension] " +
                       "or specify a per-test path via [TestCase(\"res://...\")].");
        context.GetStore().Add(RUNNER_KEY, ISceneRunner.Load(path, autoFree: true, verbose: verbose));
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AfterEach(IExtensionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.GetStore().Remove<ISceneRunner>(RUNNER_KEY)?.Dispose();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public bool SupportsParameter(IParameterContext parameterContext, IExtensionContext extensionContext)
    {
        ArgumentNullException.ThrowIfNull(parameterContext);
        ArgumentNullException.ThrowIfNull(extensionContext);
        if (!extensionContext.GetStore().Has(RUNNER_KEY, typeof(ISceneRunner)))
            return false;
        var paramType = parameterContext.GetParameterInfo().ParameterType;
        if (paramType == typeof(ISceneRunner))
            return true;
        var scene = extensionContext.GetStore().Value<ISceneRunner>(RUNNER_KEY).Scene();
        return scene != null && paramType.IsInstanceOfType(scene);
    }

    /// <inheritdoc />
    public object? ResolveParameter(IParameterContext parameterContext, IExtensionContext extensionContext)
    {
        ArgumentNullException.ThrowIfNull(parameterContext);
        ArgumentNullException.ThrowIfNull(extensionContext);
        var runner = extensionContext.GetStore().Value<ISceneRunner>(RUNNER_KEY);
        return parameterContext.GetParameterInfo().ParameterType == typeof(ISceneRunner)
            ? runner
            : runner.Scene();
    }
}
