using System;

public partial class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("Hello World");
    }



    public partial class SignalCollectorTest : Godot.GodotObject
    {
        public void ConnectAllSignals(Godot.GodotObject emitter)
        {
            foreach (Godot.Collections.Dictionary signalDef in emitter.GetSignalList())
            {
                string signalName = (string)signalDef["name"];

                var cb = new Godot.Callable(this, nameof(OnSignalEmmited));
                //cb.Bind({ emitter, signalName });
                if (!emitter.IsConnected(signalName, cb))
                {
                    emitter.Connect(signalName, cb);
                }
            }
        }

        private void OnSignalEmmited(Godot.GodotObject emitter, string signalName) => Godot.GD.PrintS(emitter, signalName);
        private void OnSignalEmmited(object arg1, Godot.GodotObject emitter, string signalName) => Godot.GD.PrintS(emitter, signalName, new[] { arg1 });
        private void OnSignalEmmited(object arg1, object arg2, Godot.GodotObject emitter, string signalName) => Godot.GD.PrintS(emitter, signalName, new[] { arg1, arg2 });
    }
}