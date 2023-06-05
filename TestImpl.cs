using Godot;
using System;
using System.Collections.Generic;



namespace GdUnit4
{


    public partial class SignalCollectorTest : Godot.GodotObject
    {

        public void ConnectAllSignals(Godot.GodotObject emitter)
        {
            foreach (Godot.Collections.Dictionary signalDef in emitter.GetSignalList())
            {
                string signalName = (string)signalDef["name"];
                var cb = Callable.From(() => OnSignalEmitted(emitter, signalName)); // not works
                var error = emitter.Connect(signalName, cb);
            }
        }


        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName) => Godot.GD.PrintS("A", emitter, signalName);

        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName, Godot.Variant arg) => Godot.GD.PrintS("B", emitter, signalName, arg);

        private void OnSignalEmitted(Godot.Variant arg, Godot.GodotObject emitter, string signalName) => Godot.GD.PrintS("C", emitter, signalName, arg);

        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName, params Godot.Variant[] args) => Godot.GD.PrintS("D", emitter, signalName, args);

        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName, IEnumerable<Godot.Variant> args) => Godot.GD.PrintS("E", emitter, signalName, args);
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
