// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

using GdUnit4.Core.TestExtensions;
using GdUnit4.Tests.Core.Resources.Scenes;

[TestSuite]
[RequireGodotRuntime]
public sealed class SceneRunnerExtensionRegisteredTest
{
    [RegisterExtension]
    private static readonly SceneRunnerExtension SceneRunner = new(
        defaultScenePath: "res://src/core/resources/scenes/TestSceneWithButton.tscn"
    );

    [TestCase]
    public void Runner_IsInjected(ISceneRunner runner)
        => AssertThat(runner).IsNotNull();

    [TestCase]
    public void Scene_IsInjected(TestSceneWithButton scene)
        => AssertThat(scene).IsNotNull();

    [TestCase]
    public void RunnerAndScene_AreInjected(ISceneRunner runner, TestSceneWithButton scene)
    {
        AssertThat(runner).IsNotNull();
        AssertThat(scene).IsNotNull();
        AssertThat(runner.Scene()).IsSame(scene);
    }

    [TestCase("foobar", 1234)]
    public void MixedArgs_InjectedAndTestCase(ISceneRunner runner, TestSceneWithButton scene, string arg1, int arg2)
    {
        AssertThat(runner).IsNotNull();
        AssertThat(scene).IsNotNull();
        AssertThat(arg1).IsEqual("foobar");
        AssertThat(arg2).IsEqual(1234);
    }
}
