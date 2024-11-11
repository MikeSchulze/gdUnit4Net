namespace GdUnit4.Tests.Core.Hooks;

using System;
using System.Threading.Tasks;

using Godot;

[TestSuite]
public partial class GodotExceptionHookTest
{
    [TestCase]
    [ThrowsException(typeof(InvalidOperationException), "This is a internal test exception.",
        "/src/core/hooks/GodotExceptionHookTest.cs", 45)]
    public void CatchExceptionOnAddingNodeToSceneTree()
    {
        var sceneTree = (SceneTree)Engine.GetMainLoop();
        sceneTree.Root.AddChild(new TestNode());
    }

    [TestCase]
    [ThrowsException(typeof(InvalidProgramException), "Exception during scene processing",
        "src/core/resources/scenes/TestSceneWithExceptionTest.cs", 21)]
    public async Task CatchExceptionOnSceneTreeProcessing()
    {
        var sceneRunner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn", true);
        // run scene, it will throw an InvalidProgramException at frame 10
        await sceneRunner.SimulateFrames(10);
    }

    [TestCase]
    [ThrowsException(typeof(InvalidOperationException), "Test Exception",
        "src/core/resources/scenes/TestSceneWithExceptionTest.cs", 14)]
    public void CatchExceptionIsThrownOnSceneInvoke()
    {
        var runner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn");

        runner.Invoke("SomeMethodThatThrowsException");
    }

    // Test class to verify the interceptor
    public partial class TestNode : Node
    {
        public override void _Ready() =>
            // Trigger a test exception
            throw new InvalidOperationException("This is a internal test exception.");
    }
}
