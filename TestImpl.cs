using Godot;
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
                Godot.Collections.Array args = (Godot.Collections.Array)signalDef["args"];
                var error = emitter.Connect(signalName, BuildCallable(emitter, signalName, args));
            }
        }

        private Callable BuildCallable(GodotObject emitter, string signalName, Godot.Collections.Array signalArguments)
        {
            switch (signalArguments.Count)
            {
                case 0:
                    return Callable.From(() => OnSignalEmitted(emitter, signalName, new Godot.Collections.Array<Variant>()));
                case 1:
                    return Callable.From<Godot.Variant>((node) => OnSignalEmitted(emitter, signalName, node));
                default:
                    throw new NotImplementedException();
            }
        }

        private void OnSignalEmitted(Godot.GodotObject emitter, string signalName, params Godot.Variant[] args) => Godot.GD.PrintS(emitter, signalName, args.Formated());

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
