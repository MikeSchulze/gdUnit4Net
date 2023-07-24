using static System.Console;
using System;
using System.Threading;
using System.Collections.Generic;

namespace GdUnit4.Core.Signals
{

    internal sealed partial class GodotSignalCollector : Godot.RefCounted, IDisposable
    {
        private Dictionary<Godot.GodotObject, Dictionary<string, List<Godot.Variant[]>>> _collectedSignals = new Dictionary<Godot.GodotObject, Dictionary<string, List<Godot.Variant[]>>>();

        private static GodotSignalCollector INSTANCE = new GodotSignalCollector();

        public static GodotSignalCollector Instance => INSTANCE;

        public void RegisterEmitter(Godot.GodotObject emitter)
        {
            // do not register the same emitter at twice
            if (_collectedSignals.ContainsKey(emitter))
                return;
            _collectedSignals[emitter] = new Dictionary<string, List<Godot.Variant[]>>();
            // connect to 'TreeExiting' of the emitter to finally release all acquired resources/connections.
            Action<GodotSignalCollector, Godot.GodotObject> action = UnregisterEmitter;
            if (!emitter.IsConnected(Godot.Node.SignalName.TreeExiting, Godot.Callable.From(action)))
                ((Godot.Node)emitter).TreeExiting += () => UnregisterEmitter(this, emitter);

            ConnectAllSignals(emitter);
        }

        private void ConnectAllSignals(Godot.GodotObject emitter)
        {
            foreach (Godot.Collections.Dictionary signalDef in emitter.GetSignalList())
            {
                string signalName = (string)signalDef["name"];
                Godot.Collections.Array args = (Godot.Collections.Array)signalDef["args"];
                var error = emitter.Connect(signalName, BuildCallable(emitter, signalName, args.Count));
                if (error != Godot.Error.Ok)
                    Console.WriteLine($"Error on connecting signal {signalName}, Error: {error}");
                _collectedSignals[emitter][signalName] = new List<Godot.Variant[]>();
            }
        }

        private Godot.Callable BuildCallable(Godot.GodotObject emitter, string signalName, int argumentCount)
        {
            switch (argumentCount)
            {
                case 0:
                    return Godot.Callable.From(() => CollectSignal(emitter, signalName));
                case 1:
                    return Godot.Callable.From<Godot.Variant>((arg0) => CollectSignal(emitter, signalName, arg0));
                case 2:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant>((arg0, arg1) => CollectSignal(emitter, signalName, arg0, arg1));
                case 3:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2) => CollectSignal(emitter, signalName, arg0, arg1, arg2));
                case 4:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3));
                case 5:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4));
                case 6:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5));
                case 7:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6));
                case 8:
                    return Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7));
                default:
                    throw new NotImplementedException();
            }
        }

        private void CollectSignal(Godot.GodotObject emitter, string signalName, params Godot.Variant[] signalArgs)
        {
            if (IsSignalCollecting(emitter, signalName))
                _collectedSignals[emitter][signalName].Add(signalArgs);
        }

        private bool IsSignalCollecting(Godot.GodotObject emitter, string signalName) =>
            _collectedSignals.ContainsKey(emitter) && _collectedSignals[emitter].ContainsKey(signalName);

        // unregister all acquired resources/connections, otherwise it ends up in orphans
        // is called when the emitter is removed from the parent
        public void UnregisterEmitter(GodotSignalCollector collector, Godot.GodotObject emitter)
        {
            if (IsInstanceValid(collector))
            {
                //WriteLine($"disconnect_signals: {emitter}");
                foreach (Godot.Collections.Dictionary connection in collector.GetIncomingConnections())
                {
                    Godot.GodotObject source = (Godot.GodotObject)connection["source"];
                    string signalName = (string)connection["signal_name"];
                    string methodName = (string)connection["method_name"];
                    //WriteLine($"disconnect: {signalName} from {source} target {collector} -> {methodName}");
                    source!.Disconnect(signalName, new Godot.Callable(collector, methodName));
                }
            }
            if (IsInstanceValid(emitter))
                _collectedSignals.Remove(emitter);
            //DebugSignalList("UnregisterEmitter");
        }

        private void ResetCollectedSignals(Godot.GodotObject emitter)
        {
            //DebugSignalList("before claer");
            if (_collectedSignals.ContainsKey(emitter))
                foreach (var signalName in _collectedSignals[emitter].Keys)
                    _collectedSignals[emitter][signalName].Clear();
            //DebugSignalList("after claer");
        }

        private bool Match(Godot.GodotObject emitter, string signalName, params Godot.Variant[] args)
        {
            //DebugSignalList("--match--");
            foreach (var receivedArgs in _collectedSignals[emitter][signalName])
            {
                if (receivedArgs.VariantEquals(args))
                    return true;
            }
            return false;
        }

        private void DebugSignalList(string message)
        {
            WriteLine($"-----{message}-------");
            WriteLine("senders {");
            foreach (var emitter in _collectedSignals.Keys)
            {
                if (IsInstanceValid(emitter))
                {
                    WriteLine($"\t{emitter}");
                    foreach (var signalName in _collectedSignals[emitter].Keys)
                    {
                        var args = _collectedSignals[emitter][signalName];
                        WriteLine($"\t\t{signalName} {args.Formated()}");
                    }
                }
            }
            WriteLine("}");
        }

        internal bool IsEmitted(CancellationTokenSource token, Godot.GodotObject emitter, string signal, Godot.Variant[] args)
        {
            try
            {
                var node = emitter as Godot.GodotObject;
                var isProcess = node?.HasMethod("_Process") ?? false;
                var isPysicsProcess = node?.HasMethod("_PhysicsProcess") ?? false;
                int sleepTimeInMs = 10;
                while (!Match(emitter, signal, args))
                {
                    Thread.Sleep(sleepTimeInMs);
                    if (isProcess)
                        node!.CallDeferred("_Process", sleepTimeInMs);
                    if (isPysicsProcess)
                        node!.CallDeferred("_PhysicsProcess", sleepTimeInMs);
                    if (token.IsCancellationRequested)
                        return false;
                }

                return true;
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                return false;
            }
            finally
            {
                ResetCollectedSignals(emitter);
            }
        }

        internal int Count(Godot.GodotObject emitter, string signalName, Godot.Variant[] args)
        {
            if (IsSignalCollecting(emitter, signalName))
                return _collectedSignals[emitter][signalName].FindAll(signalArgs => signalArgs.VariantEquals(args)).Count;
            return 0;
        }
    }
}
