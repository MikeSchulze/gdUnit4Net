﻿namespace GdUnit4.Tests.Core.Execution.Monitoring;

using System;
using System.Threading.Tasks;

using GdUnit4.Core.Execution.Exceptions;

using Godot;

[TestSuite]
public partial class GodotExceptionMonitorTest
{
    [GodotTestCase]
    [ThrowsException(typeof(InvalidOperationException), "TestNode '_Ready' failed.",
        "/src/core/execution/monitoring/GodotExceptionMonitorTest.cs", 62)]
    public void CatchExceptionOnAddingNodeToSceneTree()
    {
        var sceneTree = (SceneTree)Engine.GetMainLoop();
        sceneTree.Root.AddChild(new TestNode());
    }

    [GodotTestCase]
    [ThrowsException(typeof(InvalidProgramException), "Exception during scene processing",
        "src/core/resources/scenes/TestSceneWithExceptionTest.cs", 22)]
    public async Task CatchExceptionOnSceneTreeProcessing()
    {
        var sceneRunner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn", true);
        // run scene, it will throw an InvalidProgramException at frame 10
        await sceneRunner.SimulateFrames(10);
    }

    [GodotTestCase]
    [GodotExceptionMonitor]
    public async Task MonitorOnExceptionsButNotThrows()
    {
        var sceneRunner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn", true);
        // run scene
        await sceneRunner.SimulateFrames(6);
    }

    [GodotTestCase]
    [ThrowsException(typeof(InvalidOperationException), "Test Exception",
        "src/core/resources/scenes/TestSceneWithExceptionTest.cs", 14)]
    public void CatchExceptionIsThrownOnSceneInvoke()
    {
        var runner = ISceneRunner.Load("res://src/core/resources/scenes/TestSceneWithExceptionTest.tscn");

        runner.Invoke("SomeMethodThatThrowsException");
    }

    [GodotTestCase]
    [ThrowsException(typeof(TestFailedException), "Testing Godot PushError",
        "src/core/execution/monitoring/GodotExceptionMonitorTest.cs", 54)]
    public void PushErrorAsTestFailure() => GD.PushError("Testing Godot PushError");


    // Test class to verify the interceptor
    public partial class TestNode : Node
    {
        public override void _Ready() =>
            // Trigger a test exception
            throw new InvalidOperationException("TestNode '_Ready' failed.");
    }
}