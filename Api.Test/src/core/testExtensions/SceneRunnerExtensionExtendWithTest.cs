// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

using GdUnit4.Core.TestExtensions;
using GdUnit4.Tests.Core.Resources.Scenes;

[TestSuite]
[RequireGodotRuntime]
[ExtendWith<SceneRunnerExtension>]
public sealed class SceneRunnerExtensionExtendWithTest
{
    private const string ScenePath = "res://src/core/resources/scenes/TestSceneWithButton.tscn";

    [TestCase(ScenePath)]
    public void Runner_IsInjected(ISceneRunner runner)
        => AssertThat(runner).IsNotNull();

    [TestCase(ScenePath)]
    public void Scene_IsInjected(TestSceneWithButton scene)
        => AssertThat(scene).IsNotNull();

    [TestCase(ScenePath)]
    public void RunnerAndScene_AreInjected(ISceneRunner runner, TestSceneWithButton scene)
    {
        AssertThat(runner).IsNotNull();
        AssertThat(scene).IsNotNull();
        AssertThat(runner.Scene()).IsSame(scene);
    }

    // Scene path is consumed by the extension via the store; the int arg is type-matched to `value`
    [TestCase(ScenePath, 42)]
    public void MixedArgs_InjectedAndTestCase(ISceneRunner runner, TestSceneWithButton scene, int value)
    {
        AssertThat(runner).IsNotNull();
        AssertThat(scene).IsNotNull();
        AssertThat(value).IsEqual(42);
    }
}
