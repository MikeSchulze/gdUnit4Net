//!/usr/bin/env -S godot -s

namespace GdUnit4.Core.Runners;

using System;

using Api;

using Godot;

public partial class GodotTestRunnerScene : SceneTree
{
    public override void _Initialize()
    {
        try
        {
            var testCaseRunner = new TestRunner();
            Root.AddChild(testCaseRunner);
        }
        catch (Exception e)
        {
            GD.PrintErr("Exception", e.Message);
            Quit(100); // Exit with error code
        }
    }

    // ReSharper disable once PartialTypeWithSinglePart
    private partial class TestRunner : Node
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
