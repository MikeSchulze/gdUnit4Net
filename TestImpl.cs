using Godot;
using Godot.Collections;
using System;



namespace GdUnit4
{


    public partial class SignalCollectorTest : Godot.GodotObject
    {

        public void ConnectAllSignals(Godot.GodotObject emitter)
        {
            foreach (Godot.Collections.Dictionary signalDef in emitter.GetSignalList())
            {
                string signalName = (string)signalDef["name"];
                var cb = new Callable(this, nameof(OnSignalEmitted)); // works
                //var cb = Callable.From(OnSignalEmmited); // not works

                //var signal = emitter.ToSignal(emitter, signalName);
                // signal += () => OnSignalEmmited(emitter, signalName);
                //signal.OnCompleted(() => GD.PrintS("OnCompleted", signalName, signal.));


                emitter.Connect(signalName, cb);
            }
        }

        private void OnSignalEmitted() => Godot.GD.PrintS("A");
        private void OnSignalEmitted(Godot.GodotObject arg) => Godot.GD.PrintS("B", arg);
        private void OnSignalEmitted(params Godot.GodotObject[] args) => Godot.GD.PrintS("C", args, args.Length);

        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName, params Godot.GodotObject[] args) => Godot.GD.PrintS(emitter, signalName, args);
    }


    public partial class TestImpl : Control
    {

        bool ForceQuit = false;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            Console.WriteLine("Hallo World");

            var collector = new SignalCollectorTest();
            Node emitter = new Node();
            collector.ConnectAllSignals(emitter);

            AddChild(emitter);
            emitter.AddChild(new Node2D());
            emitter.QueueFree();



            ForceQuit = true;

        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            if (ForceQuit)
            {
                GC.Collect();
                GC.WaitForFullGCComplete(5000);

                PrintOrphanNodes();
                GetTree().Quit();
            }
        }
    }
}
