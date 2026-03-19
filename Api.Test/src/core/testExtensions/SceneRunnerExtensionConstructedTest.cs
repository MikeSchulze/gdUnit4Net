using GdUnit4.Core.TestExtensions;
using Godot;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[RequireGodotRuntime]
public class SceneRunnerExtensionConstructedTest
{
    [RegisterExtension] public static readonly SceneRunnerExtension SceneRunnerExtension = new(
        defaultScenePath: "res://src/core/resources/scenes/TestSceneCSharp.tscn",
        verbose: true
    );


    [TestCase]
    public void TestSceneRunnerIsInjected(ISceneRunner sceneRunner)
    {
        AssertThat(sceneRunner)
            .IsNotNull();
    }


    [TestCase]
    public void TestSceneRunnerAndSceneAreInjected(ISceneRunner sceneRunner, Node scene)
    {
        AssertThat(sceneRunner)
            .IsNotNull();

        AssertThat(scene)
            .IsNotNull();

        AssertThat(sceneRunner.Scene())
            .IsSame(scene);
    }
    
    [TestCase]
    public void TestSceneIsInjected(Node scene)
    {
        AssertThat(scene)
            .IsNotNull();
    }
    
    [TestCase]
    public void TestTypedSceneIsInjected(TestScene scene)
    {
        AssertThat(scene)
            .IsNotNull();

        // Verify it's the live typed instance by exercising a method on it.
        AssertThat(scene.Add(2, 3))
            .IsEqual(5);
    }

    [TestCase("foobar", 1234)]
    public void TestAdditionalTestCaseArgsInjected(ISceneRunner sceneRunner, Node scene, string arg1, int arg2)
    {
        AssertThat(sceneRunner)
            .IsNotNull();

        AssertThat(scene)
            .IsNotNull();

        AssertThat(arg1)
            .IsEqual("foobar");

        AssertThat(arg2)
            .IsEqual(1234);
    }


    [TestCase("res://src/core/resources/scenes/TestScene.tscn")]
    public void TestPerTestOverrideOfSceneRunnerPath(ISceneRunner sceneRunner, TestScene scene)
    {
        AssertThat(scene)
            .IsNotNull();
    }
}
