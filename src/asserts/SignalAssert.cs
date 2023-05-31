using static System.Console;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GdUnit4.Asserts
{
    using Exceptions;

    internal sealed partial class SignalAssert : AssertBase<Godot.GodotObject>, ISignalAssert
    {

        public SignalAssert(Godot.GodotObject current) : base(current)
        {
            SignalCollector.Instance.RegisterEmitter(current);
        }

        public async Task<ISignalAssert> IsEmitted(string signal, params object[] args)
        {
            IsNotNull();
            IsSignalExists(signal);

            var lineNumber = new StackFrame(3, true).GetFileLineNumber();
            var isEmitted = await IsEmittedTask(signal, args);
            if (!isEmitted)
                ThrowTestFailureReport(AssertFailures.IsEmitted(Current, signal, args), Current, signal, lineNumber);
            return this;
        }

        public async Task<ISignalAssert> IsNotEmitted(string signal, params object[] args)
        {
            IsNotNull();
            IsSignalExists(signal);

            var lineNumber = new StackFrame(3, true).GetFileLineNumber();
            var isEmitted = await IsEmittedTask(signal, args);
            if (isEmitted)
                ThrowTestFailureReport(AssertFailures.IsNotEmitted(Current, signal, args), Current, signal, lineNumber);
            return this;
        }

        public ISignalAssert IsSignalExists(string signal)
        {
            IsNotNull();
            if (!Current!.HasSignal(signal))
                ThrowTestFailureReport(AssertFailures.IsSignalExists(Current, signal), Current, signal);
            return this;
        }

        public ISignalAssert IsCountEmitted(int expectedCount, string signal, params object[] args)
        {
            IsNotNull();
            IsSignalExists(signal);
            var count = SignalCollector.Instance.Count(Current!, signal, args);
            if (count != expectedCount)
                ThrowTestFailureReport($"Expecting emitted count is {expectedCount} but was {count}", Current, signal);
            return this;
        }

        private async Task<bool> IsEmittedTask(string signal, params object[] args)
        {
            using var token = new CancellationTokenSource();
            Thread.SetData(Thread.GetNamedDataSlot("SignalCancellationToken"), token);
            return await Task.Run<bool>(() => SignalCollector.Instance.IsEmitted(token, Current!, signal, args), token.Token);
        }

        private void ThrowTestFailureReport(string message, object? current, object? expected, int lineNumber)
        {
            CurrentFailureMessage = CustomFailureMessage ?? message;
            throw new TestFailedException(CurrentFailureMessage, 0, lineNumber);
        }

        internal sealed partial class SignalCollector : Godot.RefCounted, IDisposable
        {
            private Dictionary<Godot.GodotObject, Dictionary<string, List<object[]>>> _collectedSignals = new Dictionary<Godot.GodotObject, Dictionary<string, List<object[]>>>();

            private static SignalCollector INSTANCE = new SignalCollector();

            public static SignalCollector Instance => INSTANCE;

            public void OnEmitterRenamedConnectedInScript()
            {
                Godot.GD.Print("Invoked: OnEmitterRenamedConnectedInScript");
            }

            public void RegisterEmitter(Godot.GodotObject emitter)
            {
                // do not register the same emitter at twice
                if (_collectedSignals.ContainsKey(emitter))
                    return;
                _collectedSignals[emitter] = new Dictionary<string, List<object[]>>();
                // connect to 'TreeExiting' of the emitter to finally release all acquired resources/connections.
                Action<SignalCollector, Godot.GodotObject> action = UnregisterEmitter;
                if (!emitter.IsConnected(Godot.Node.SignalName.TreeExiting, Godot.Callable.From(action)))
                    ((Godot.Node)emitter).TreeExiting += () => UnregisterEmitter(this, emitter);

                

                foreach (Godot.Collections.Dictionary signalDef in emitter.GetSignalList())
                {
                    string signalName = (string)signalDef["name"];

                    var signalTest = new Godot.Signal(emitter, signalName);
                    // set inital collected to empty
                    if (!IsSignalCollecting(emitter, signalName))
                        _collectedSignals[emitter][signalName] = new List<object[]>();
                    if (!emitter.IsConnected(signalName, new Godot.Callable(this, nameof(SignalCollector.OnSignalEmmited))))
                    {
                        var signalAwaiter = emitter.ToSignal(emitter, signalName);
                        var signal = new Godot.Signal(emitter, signalName);

                        // TODO connect to signal with arguments
                        // emitter.Connect(signalName, new Godot.Callable(this, nameof(SignalCollector.OnSignalEmmited)), new Godot.Collections.Array { emitter, signalName });
                    }
                }
            }

            private bool IsSignalCollecting(Godot.GodotObject emitter, string signalName) =>
                _collectedSignals.ContainsKey(emitter) && _collectedSignals[emitter].ContainsKey(signalName);

            // unregister all acquired resources/connections, otherwise it ends up in orphans
            // is called when the emitter is removed from the parent
            public void UnregisterEmitter(SignalCollector collector, Godot.GodotObject emitter)
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

            private bool Match(Godot.GodotObject emitter, string signalName, params object[] args)
            {
                //DebugSignalList("--match--");
                foreach (var receivedArgs in _collectedSignals[emitter][signalName])
                {
                    if (Comparable.IsEqual(receivedArgs, args).Valid)
                        return true;
                }
                return false;
            }

            // receives the signal from the emitter with all emitted signal arguments and additional the emitter and signal_name as last two arguements
            private void OnSignalEmmited(Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName);
            private void OnSignalEmmited(object arg1, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1 });
            private void OnSignalEmmited(object arg1, object arg2, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5, arg6 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9 });
            private void OnSignalEmmited(object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7, object arg8, object arg9, object arg10, Godot.GodotObject emitter, string signalName) =>
                CollectSignal(emitter, signalName, new[] { arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10 });

            private void CollectSignal(Godot.GodotObject emitter, string signalName, params object[] signalArgs)
            {
                //WriteLine($"CollectSignal: {emitter} Signal: {signalName} {signalArgs.Formated()}");
                if (IsSignalCollecting(emitter, signalName))
                    _collectedSignals[emitter][signalName].Add(signalArgs);
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

            internal bool IsEmitted(CancellationTokenSource token, Godot.GodotObject emitter, string signal, object[] args)
            {
                try
                {
                    int sleepTimeInMs = 10;
                    while (!Match(emitter, signal, args))
                    {
                        Thread.Sleep(sleepTimeInMs);
                        if (emitter is Godot.Node)
                        {
                            Godot.Node node = (Godot.Node)emitter;
                            node._Process(sleepTimeInMs);
                            node._PhysicsProcess(sleepTimeInMs);
                        }
                        if (token.IsCancellationRequested)
                            return false;
                    }
                    return true;
                }
                finally
                {
                    ResetCollectedSignals(emitter);
                }
            }

            internal int Count(Godot.GodotObject emitter, string signalName, object[] args)
            {
                if (IsSignalCollecting(emitter, signalName))
                    return _collectedSignals[emitter][signalName].FindAll(signalArgs => Comparable.IsEqual(signalArgs, args).Valid).Count;
                return 0;
            }
        }
    }

    internal static class SignalAssertTaskExtension
    {
        public static async Task<ISignalAssert> WithTimeout(this Task<ISignalAssert> task, int timeoutMillis)
        {
            var wrapperTask = Task.Run(async () => await task);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
            token.Cancel();
            if (completedTask == wrapperTask)
                return await task;
            else
            {
                CancellationTokenSource cancelToken = (CancellationTokenSource)Thread.GetData(Thread.GetNamedDataSlot("SignalCancellationToken"));
                if (cancelToken != null)
                    cancelToken.Cancel();
                return await task;
            }
        }
    }
}
