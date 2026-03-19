using GdUnit4.Core.TestExtensions;
using Godot;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[RequireGodotRuntime]
public class SceneRunnerExtensionMethodLevelTest
{
    [ExtendWith<SceneRunnerExtension>]
    [TestCase("res://src/core/resources/scenes/TestSceneCSharp.tscn")]
    public void TestSceneRunnerIsInjected(ISceneRunner sceneRunner)
    {
        AssertThat(sceneRunner)
            .IsNotNull();
    }

    [ExtendWith<SceneRunnerExtension>]
    [TestCase("res://src/core/resources/scenes/TestSceneCSharp.tscn")]
    public void TestSceneRunnerAndSceneAreInjected(ISceneRunner sceneRunner, Node scene)
    {
        AssertThat(sceneRunner)
            .IsNotNull();

        AssertThat(scene)
            .IsNotNull();

        AssertThat(sceneRunner.Scene())
            .IsSame(scene);
    }

    [ExtendWith<SceneRunnerExtension>]
    [TestCase("res://src/core/resources/scenes/TestSceneCSharp.tscn")]
    public void TestSceneIsInjected(Node scene)
    {
        AssertThat(scene)
            .IsNotNull();
    }

    [ExtendWith<SceneRunnerExtension>]
    [TestCase("res://src/core/resources/scenes/TestSceneCSharp.tscn")]
    public void TestTypedSceneIsInjected(TestScene scene)
    {
        AssertThat(scene)
            .IsNotNull();

        // Verify it's the live typed instance by exercising a method on it.
        AssertThat(scene.Add(2, 3))
            .IsEqual(5);
    }

    // Extra [TestCase] args that don't match ISceneRunner or the scene type pass through
    // to remaining method parameters by type. The scene-path string is matched to the first
    // string parameter; use non-string extra args to avoid ambiguity.
    [ExtendWith<SceneRunnerExtension>]
    [TestCase("res://src/core/resources/scenes/TestSceneCSharp.tscn", 42, true, "foobar")]
    public void TestAdditionalTestCaseArgsInjected(ISceneRunner sceneRunner, Node scene, int arg1, bool arg2)
    {
        AssertThat(sceneRunner)
            .IsNotNull();

        AssertThat(scene)
            .IsNotNull();

        AssertThat(arg1)
            .IsEqual(42);

        AssertThat(arg2)
            .IsEqual(true);
    }
}
