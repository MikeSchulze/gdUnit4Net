// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Signals;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

using Extensions;

using Godot;

using static System.Console;

using Array = Godot.Collections.Array;
using Error = Godot.Error;

internal sealed partial class GodotSignalCollector : RefCounted
{
    public static GodotSignalCollector Instance { get; } = new();

    internal Dictionary<GodotObject, Dictionary<string, List<Variant[]>>> CollectedSignals { get; } = new();

    public void RegisterEmitter(GodotObject emitter)
    {
        // do not register the same emitter at twice
        if (CollectedSignals.ContainsKey(emitter))
            return;
        CollectedSignals[emitter] = new Dictionary<string, List<Variant[]>>();

        // connect to 'TreeExiting' of the emitter to finally release all acquired resources/connections.
        var action = UnregisterEmitter;
        if (emitter is Node node && !node.IsConnected(Node.SignalName.TreeExiting, Callable.From(action)))
            node.TreeExiting += () => UnregisterEmitter(this, emitter);

        ConnectAllSignals(emitter);
    }

    internal int Count(GodotObject emitter, string signalName, Variant[] args)
        => IsSignalCollecting(emitter, signalName)
            ? CollectedSignals[emitter][signalName].FindAll(signalArgs => signalArgs.VariantEquals(args)).Count
            : 0;

    internal bool IsSignalCollecting(GodotObject emitter, string signalName)
        => CollectedSignals.ContainsKey(emitter) && CollectedSignals[emitter].ContainsKey(signalName);

    internal bool IsEmitted(CancellationTokenSource token, GodotObject emitter, string signal, Variant[] args)
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
#pragma warning disable CA1031
        catch (Exception e)
#pragma warning restore CA1031
        {
            WriteLine(e.Message);
            return false;
        }
        finally
        {
            ResetCollectedSignals(emitter);
        }
    }

    internal void Clean()
    {
        CollectedSignals.Keys.ToList().ForEach(emitter => UnregisterEmitter(this, emitter));
        CollectedSignals.Clear();
    }

    private void ConnectAllSignals(GodotObject emitter)
    {
        foreach (var signalDef in emitter.GetSignalList())
        {
            var signalName = (string)signalDef["name"];
            var args = (Array)signalDef["args"];
            var error = emitter.Connect(signalName, BuildCallable(emitter, signalName, args.Count));
            if (error != Error.Ok)
                WriteLine($"Error on connecting signal {signalName}, Error: {error}");
            CollectedSignals[emitter][signalName] = new List<Variant[]>();
        }
    }

    private Callable BuildCallable(GodotObject emitter, string signalName, int argumentCount)
        => argumentCount switch
        {
            0 => Callable.From(() => CollectSignal(emitter, signalName)),
            1 => Callable.From<Variant>(arg0 => CollectSignal(emitter, signalName, arg0)),
            2 => Callable.From<Variant, Variant>((arg0, arg1) => CollectSignal(emitter, signalName, arg0, arg1)),
            3 => Callable.From<Variant, Variant, Variant>((arg0, arg1, arg2) => CollectSignal(emitter, signalName, arg0, arg1, arg2)),
            4 => Callable.From<Variant, Variant, Variant, Variant>((arg0, arg1, arg2, arg3) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3)),
            5 => Callable.From<Variant, Variant, Variant, Variant, Variant>((arg0, arg1, arg2, arg3, arg4) => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4)),
            6 => Callable.From<Variant, Variant, Variant, Variant, Variant, Variant>((arg0, arg1, arg2, arg3, arg4, arg5)
                => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5)),
            7 => Callable.From<Variant, Variant, Variant, Variant, Variant, Variant, Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6)
                => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6)),
            8 => Callable.From<Variant, Variant, Variant, Variant, Variant, Variant, Variant, Variant>((arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)
                => CollectSignal(emitter, signalName, arg0, arg1, arg2, arg3, arg4, arg5, arg6, arg7)),
            _ => throw new NotImplementedException()
        };

    private void CollectSignal(GodotObject emitter, string signalName, params Variant[] signalArgs)
    {
        if (IsSignalCollecting(emitter, signalName))
            CollectedSignals[emitter][signalName].Add(signalArgs);
    }

    // unregister all acquired resources/connections, otherwise it ends up in orphans
    // is called when the emitter is removed from the parent
    private void UnregisterEmitter(GodotSignalCollector collector, GodotObject emitter)
    {
        if (IsInstanceValid(collector))
        {
            // WriteLine($"disconnect_signals: {emitter}");
            foreach (var connection in collector.GetIncomingConnections())
            {
                var source = (GodotObject)connection["source"];
                var signalName = (string)connection["signal_name"];
                var methodName = (string)connection["method_name"];

                // WriteLine($"disconnect: {signalName} from {source} target {collector} -> {methodName}");
                source.Disconnect(signalName, new Callable(collector, methodName));
            }
        }

        if (IsInstanceValid(emitter))
            CollectedSignals.Remove(emitter);

        // DebugSignalList("UnregisterEmitter");
    }

    private void ResetCollectedSignals(GodotObject emitter)
    {
        // DebugSignalList("before clear");
        if (!CollectedSignals.TryGetValue(emitter, out var value))
            return;
        foreach (var signalName in value.Keys)
            value[signalName].Clear();

        // DebugSignalList("after clear");
    }

    private bool Match(GodotObject emitter, string signalName, params Variant[] args)

        // DebugSignalList("--match--");
        => CollectedSignals[emitter][signalName].Any(receivedArgs => receivedArgs.VariantEquals(args));

    [SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Debug output not requiring localization")]
    private void DebugSignalList(string message)
    {
        WriteLine($"-----{message}-------");
        WriteLine("senders {");
        foreach (var emitter in CollectedSignals.Keys.Where(IsInstanceValid))
        {
            WriteLine($"\t{emitter}");
            foreach (var signalName in CollectedSignals[emitter].Keys)
            {
                var args = CollectedSignals[emitter][signalName];
                WriteLine($"\t\t{signalName} {args.Formatted()}");
            }
        }

        WriteLine("}");
    }
}
