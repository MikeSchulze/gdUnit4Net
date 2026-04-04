using GdUnit4.Core.TestExtensions;
using GdUnit4.Tests.Core.Resources.Scenes;
using static GdUnit4.Assertions;

namespace GdUnit4.Tests.Core.TestExtensions;

[TestSuite]
[RequireGodotRuntime]
public class SceneRunnerExtensionConstructedTest
{
    [RegisterExtension]
    public static readonly SceneRunnerExtension<TestSceneWithButton> SceneRunnerExtension = new(
        scenePath: "res://src/core/resources/scenes/TestSceneWithButton.tscn"
    );


    [TestCase]
    public void TestSceneRunnerIsInjected(ISceneRunner sceneRunner)
    {
        AssertThat(sceneRunner)
            .IsNotNull();
    }
    
    [TestCase]
    public void TestSceneIsInjected(TestSceneWithButton scene)
    {
        AssertThat(scene)
            .IsNotNull();
    }


    [TestCase]
    public void TestSceneRunnerAndSceneAreInjected(ISceneRunner sceneRunner, TestSceneWithButton scene)
    {
        AssertThat(sceneRunner)
            .IsNotNull();
        AssertThat(scene)
            .IsNotNull();
        AssertThat(sceneRunner.Scene())
            .IsSame(scene);
    }

    [TestCase("foobar", 1234)]
    public void TestAdditionalTestCaseArgsInjected(ISceneRunner sceneRunner, TestSceneWithButton scene, string arg1, int arg2)
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
}
