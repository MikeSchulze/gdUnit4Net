namespace GdUnit4.Core.Signals;

using static System.Console;

using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

internal sealed partial class GodotSignalCollector : Godot.RefCounted
{
    private readonly Dictionary<Godot.GodotObject, Dictionary<string, List<Godot.Variant[]>>> collectedSignals = new();

    public static GodotSignalCollector Instance { get; } = new();

    public void RegisterEmitter(Godot.GodotObject emitter)
    {
        // do not register the same emitter at twice
        if (collectedSignals.ContainsKey(emitter))
            return;
        collectedSignals[emitter] = new Dictionary<string, List<Godot.Variant[]>>();
        // connect to 'TreeExiting' of the emitter to finally release all acquired resources/connections.
        var action = UnregisterEmitter;
        if (!emitter.IsConnected(Godot.Node.SignalName.TreeExiting, Godot.Callable.From(action)))
            ((Godot.Node)emitter).TreeExiting += () => UnregisterEmitter(this, emitter);

        ConnectAllSignals(emitter);
    }

    private void ConnectAllSignals(Godot.GodotObject emitter)
    {
        foreach (var signalDef in emitter.GetSignalList())
        {
            var signalName = (string)signalDef["name"];
            var args = (Godot.Collections.Array)signalDef["args"];
            var error = emitter.Connect(signalName, BuildCallable(emitter, signalName, args.Count));
            if (error != Godot.Error.Ok)
                WriteLine($"Error on connecting signal {signalName}, Error: {error}");
            collectedSignals[emitter][signalName] = new List<Godot.Variant[]>();
        }
    }

    private Godot.Callable BuildCallable(Godot.GodotObject emitter, string signalName, int argumentCount)
        => argumentCount switch
        {
            0 => Godot.Callable.From(() => CollectSignal(emitter, signalName)),
            1 => Godot.Callable.From<Godot.Variant>((arg0) => CollectSignal(emitter, signalName, arg0)),
            2 => Godot.Callable.From<Godot.Variant, Godot.Variant>((arg0, arg1) => CollectSignal(emitter, signalName, arg0, arg1)),
            3 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2) => CollectSignal(emitter, signalName, arg0, arg1, arg2)),
            4 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3)),
            5 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4)),
            6 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5)),
            7 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6)),
            8 => Godot.Callable.From<Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant, Godot.Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)),
            _ => throw new NotImplementedException(),
        };

    private void CollectSignal(Godot.GodotObject emitter, string signalName, params Godot.Variant[] signalArgs)
    {
        if (IsSignalCollecting(emitter, signalName))
            collectedSignals[emitter][signalName].Add(signalArgs);
    }

    internal bool IsSignalCollecting(Godot.GodotObject emitter, string signalName)
        => collectedSignals.ContainsKey(emitter) && collectedSignals[emitter].ContainsKey(signalName);

    // unregister all acquired resources/connections, otherwise it ends up in orphans
    // is called when the emitter is removed from the parent
    private void UnregisterEmitter(GodotSignalCollector collector, Godot.GodotObject emitter)
    {
        if (IsInstanceValid(collector))
            //WriteLine($"disconnect_signals: {emitter}");
            foreach (var connection in collector.GetIncomingConnections())
            {
                var source = (Godot.GodotObject)connection["source"];
                var signalName = (string)connection["signal_name"];
                var methodName = (string)connection["method_name"];
                //WriteLine($"disconnect: {signalName} from {source} target {collector} -> {methodName}");
                source.Disconnect(signalName, new Godot.Callable(collector, methodName));
            }
        if (IsInstanceValid(emitter))
            collectedSignals.Remove(emitter);
        //DebugSignalList("UnregisterEmitter");
    }

    private void ResetCollectedSignals(Godot.GodotObject emitter)
    {
        //DebugSignalList("before clear");
        if (!collectedSignals.TryGetValue(emitter, out var value)) return;
        foreach (var signalName in value.Keys)
            value[signalName].Clear();
        //DebugSignalList("after clear");
    }

    private bool Match(Godot.GodotObject emitter, string signalName, params Godot.Variant[] args)
        //DebugSignalList("--match--");
        => collectedSignals[emitter][signalName].Any(receivedArgs => receivedArgs.VariantEquals(args));

    private void DebugSignalList(string message)
    {
        WriteLine($"-----{message}-------");
        WriteLine("senders {");
        foreach (var emitter in collectedSignals.Keys.Where(IsInstanceValid))
        {
            WriteLine($"\t{emitter}");
            foreach (var signalName in collectedSignals[emitter].Keys)
            {
                var args = collectedSignals[emitter][signalName];
                WriteLine($"\t\t{signalName} {args.Formatted()}");
            }
        }
        WriteLine("}");
    }

    internal bool IsEmitted(CancellationTokenSource token, Godot.GodotObject emitter, string signal, Godot.Variant[] args)
    {
        try
        {
            var isProcess = emitter.HasMethod("_Process");
            var isPhysicsProcess = emitter.HasMethod("_PhysicsProcess");
            var sleepTimeInMs = 10;
            while (!Match(emitter, signal, args))
            {
                Thread.Sleep(sleepTimeInMs);
                if (isProcess)
                    emitter.CallDeferred("_Process", sleepTimeInMs);
                if (isPhysicsProcess)
                    emitter.CallDeferred("_PhysicsProcess", sleepTimeInMs);
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
        => IsSignalCollecting(emitter, signalName)
            ? collectedSignals[emitter][signalName].FindAll(signalArgs => signalArgs.VariantEquals(args)).Count
            : 0;
}
