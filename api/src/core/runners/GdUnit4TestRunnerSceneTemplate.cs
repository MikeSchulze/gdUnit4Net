//!/usr/bin/env -S godot -s

// ReSharper disable once CheckNamespace

namespace GdUnit4.TestRunner;

using System;

using Core.Runners;

using Godot;

public partial class GdUnit4TestRunnerSceneTemplate : SceneTree
{
    public override void _Initialize()
    {
        try
        {
            Root.AddChild(new TestRunner());
        }
        catch (Exception e)
        {
            GD.PrintErr("Exception", e.Message);
            Quit(100); // Exit with error code
        }
    }

    // ReSharper disable once PartialTypeWithSinglePart
    private sealed partial class TestRunner : Node
    {
        public TestRunner()
        {
            Logger = new GodotLogger();
            Server = new GodotGdUnit4RestServer(Logger);
        }

        private ITestEngineLogger Logger { get; }

        private GodotGdUnit4RestServer Server { get; }

        public override void _Ready() => _ = Server.Start();

        public override void _Process(double delta) => _ = Server.Process();

        public override void _Notification(int what)
        {
            if (what == NotificationPredelete)
                Server.Stop();
        }
    }
}
