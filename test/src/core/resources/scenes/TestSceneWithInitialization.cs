using Godot;

namespace GdUnit4.Tests.core.resources.scenes;

using System.Collections.Generic;

public partial class TestSceneWithInitialization : Node2D
{
    private readonly List<string> methodCalls = new();
    public List<string> MethodCalls => methodCalls;
    public void Initialize() => methodCalls.Add("Initialize");

    public override void _Ready() => methodCalls.Add("_Ready");
}

