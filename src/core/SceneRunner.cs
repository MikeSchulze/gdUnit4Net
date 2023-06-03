using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace GdUnit4
{
    using Asserts;
    using Executions;
    using Godot;
    using static Assertions;

    public static class GdUnitAwaiter
    {
        public static async Task WithTimeout(this Task task, int timeoutMillis)
        {
            var lineNumber = GetWithTimeoutLineNumber();
            var wrapperTask = Task.Run(async () => await task);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
            if (completedTask != wrapperTask)
                throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
            token.Cancel();
            await task;
        }

        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMillis)
        {
            var lineNumber = GetWithTimeoutLineNumber();
            var wrapperTask = Task.Run(async () => await task);
            using var token = new CancellationTokenSource();
            var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
            if (completedTask != wrapperTask)
                throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
            token.Cancel();
            return await task;
        }

        private static int GetWithTimeoutLineNumber()
        {
            StackTrace saveStackTrace = new StackTrace(true);
            return saveStackTrace.FrameCount > 4 ? saveStackTrace.GetFrame(4)!.GetFileLineNumber() : -1;
        }

        public sealed class GodotMethodAwaiter<V>
        {
            private string MethodName { get; }
            private Node Instance { get; }
            private Variant[] Args { get; }

            public GodotMethodAwaiter(Node instance, string methodName, params Variant[] args)
            {
                Instance = instance;
                MethodName = methodName;
                Args = args;
                if (!Instance.HasMethod(methodName))
                    throw new MissingMethodException($"The method '{methodName}' not exist on loaded scene.");
            }

            public async Task IsEqual(V expected) =>
                await Task.Run(async () => await IsReturnValue((current) => Comparable.IsEqual(current, expected).Valid));

            public async Task IsNull() =>
                await Task.Run(async () => await IsReturnValue((current) => current == null));

            public async Task IsNotNull() =>
                await Task.Run(async () => await IsReturnValue((current) => current != null));

            private delegate bool Comperator(object current);
            private async Task IsReturnValue(Comperator comperator)
            {
                var current = Instance.Call(MethodName, Args);
                // https://github.com/godotengine/godot/issues/77624
                Variant[] result = await Instance.ToSignal(Instance, "completed");
                if (comperator(result[0]))
                    return;
            }
        }

        public static async Task AwaitSignal(this Godot.Node node, string signal, params object[]? expectedArgs)
        {
            while (true)
            {
                Variant[] signalArgs = await Engine.GetMainLoop().ToSignal(node, signal);
                if (expectedArgs?.Length == 0 || signalArgs.Equals(expectedArgs))
                    return;
            }
        }
    }
}

namespace GdUnit4.Core
{
    using Godot;
    using Executions;
    internal sealed class SceneRunner : GdUnit4.ISceneRunner
    {
        private SceneTree SceneTree { get; set; }
        private Node CurrentScene { get; set; }
        private bool Verbose { get; set; }
        private bool SceneAutoFree { get; set; }
        private Vector2 CurrentMousePos { get; set; }
        private double TimeFactor { get; set; }
        private int SavedIterationsPerSecond { get; set; }
        private InputEvent? LastInputEvent { get; set; }

        public SceneRunner(string resourcePath, bool autoFree = false, bool verbose = false)
        {
            Verbose = verbose;
            SceneAutoFree = autoFree;
            ExecutionContext.RegisterDisposable(this);
            SceneTree = (SceneTree)Godot.Engine.GetMainLoop();
            CurrentScene = ((PackedScene)Godot.ResourceLoader.Load(resourcePath)).Instantiate();
            SceneTree.Root.AddChild(CurrentScene);
            CurrentMousePos = default;
            SavedIterationsPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
            SetTimeFactor(1.0);
        }

        public GdUnit4.ISceneRunner SetMousePos(Vector2 position)
        {
            CurrentScene.GetViewport().WarpMouse(position);
            CurrentMousePos = position;
            return this;
        }

        private void ApplyInputModifiers(InputEventWithModifiers inputEvent)
        {
            if (LastInputEvent is InputEventWithModifiers lastInputEvent)
            {
                inputEvent.MetaPressed = inputEvent.MetaPressed || lastInputEvent.MetaPressed;
                inputEvent.AltPressed = inputEvent.AltPressed || lastInputEvent.AltPressed;
                inputEvent.ShiftPressed = inputEvent.ShiftPressed || lastInputEvent.ShiftPressed;
                inputEvent.CtrlPressed = inputEvent.CtrlPressed || lastInputEvent.CtrlPressed;
            }
        }

        public GdUnit4.ISceneRunner SimulateKeyPress(Key keyCode, bool shiftPressed = false, bool controlPressed = false)
        {
            PrintCurrentFocus();
            var action = new InputEventKey();
            action.Pressed = true;
            action.Keycode = keyCode;
            action.PhysicalKeycode = keyCode;
            action.AltPressed = keyCode == Key.Alt;
            action.ShiftPressed = shiftPressed || keyCode == Key.Shift;
            action.CtrlPressed = controlPressed || keyCode == Key.Ctrl;
            ApplyInputModifiers(action);

            Print("	process key event {0} ({1}) <- {2}:{3}", CurrentScene, SceneName(), action.AsText(), action.IsPressed() ? "pressing" : "released");

            return this;
        }

        public GdUnit4.ISceneRunner SimulateKeyPressed(Key keyCode, bool shift = false, bool control = false)
        {
            SimulateKeyPress(keyCode, shift, control);
            SimulateKeyRelease(keyCode, shift, control);
            return this;
        }

        public GdUnit4.ISceneRunner SimulateKeyRelease(Key keyCode, bool shift = false, bool control = false)
        {
            PrintCurrentFocus();
            var action = new InputEventKey();
            action.Pressed = false;
            action.Keycode = keyCode;
            action.ShiftPressed = shift;
            action.CtrlPressed = control;

            Print("	process key event {0} ({1}) <- {2}:{3}", CurrentScene, SceneName(), action.AsText(), action.IsPressed() ? "pressing" : "released");

            return this;
        }

        public GdUnit4.ISceneRunner SimulateMouseMove(Vector2 relative, Vector2 speed = default)
        {
            var action = new InputEventMouseMotion();
            action.Relative = relative;
            action.Velocity = speed == default ? Vector2.One : speed;

            Print("	process mouse motion event {0} ({1}) <- {2}", CurrentScene, SceneName(), action.AsText());

            return this;
        }

        public GdUnit4.ISceneRunner SimulateMouseButtonPressed(MouseButton buttonIndex)
        {
            SimulateMouseButtonPress(buttonIndex);
            SimulateMouseButtonRelease(buttonIndex);
            return this;
        }

        public GdUnit4.ISceneRunner SimulateMouseButtonPress(MouseButton buttonIndex)
        {
            PrintCurrentFocus();
            var action = new InputEventMouseButton();
            action.ButtonIndex = buttonIndex;
            action.ButtonMask = MouseButtonMask.Left;
            action.Pressed = true;
            action.Position = CurrentMousePos;
            action.GlobalPosition = CurrentMousePos;

            Print("	process mouse button event {0} ({1}) <- {2}", CurrentScene, SceneName(), action.AsText());

            return this;
        }

        public GdUnit4.ISceneRunner SimulateMouseButtonRelease(MouseButton buttonIndex)
        {
            var action = new InputEventMouseButton();
            action.ButtonIndex = buttonIndex;
            action.ButtonMask = 0;
            action.Pressed = false;
            action.Position = CurrentMousePos;
            action.GlobalPosition = CurrentMousePos;

            Print("	process mouse button event {0} ({1}) <- {2}", CurrentScene, SceneName(), action.AsText());

            return this;
        }

        public GdUnit4.ISceneRunner SetTimeFactor(double timeFactor = 1.0)
        {
            TimeFactor = Math.Min(9.0, timeFactor);
            ActivateTimeFactor();

            Print("set time factor: {0}", TimeFactor);
            Print("set physics iterations_per_second: {0}", SavedIterationsPerSecond * TimeFactor);
            return this;
        }

        public async Task SimulateFrames(uint frames, uint deltaPeerFrame)
        {
            for (int frame = 0; frame < frames; frame++)
                await AwaitMillis(deltaPeerFrame);
        }

        public async Task SimulateFrames(uint frames)
        {
            var timeShiftFrames = Math.Max(1, frames / TimeFactor);
            for (int frame = 0; frame < timeShiftFrames; frame++)
                await AwaitIdleFrame();
        }

        private void ActivateTimeFactor()
        {
            Engine.TimeScale = (float)TimeFactor;
            Engine.PhysicsTicksPerSecond = (int)(SavedIterationsPerSecond * TimeFactor);

        }

        private void DeactivateTimeFactor()
        {
            Engine.TimeScale = 1;
            Engine.PhysicsTicksPerSecond = SavedIterationsPerSecond;
        }

        private void Print(string message, params object[] args)
        {
            if (Verbose)
                Console.WriteLine(String.Format(message, args));
        }

        private void PrintCurrentFocus()
        {
            if (!Verbose)
                return;
            var focusedNode = (CurrentScene as Control)?.Owner;//.GetFocusOwner();

            if (focusedNode != null)
                Console.WriteLine("	focus on {0}", focusedNode);
            else
                Console.WriteLine("	no focus set");
        }

        private string SceneName()
        {
            Script? sceneScript = (Script?)CurrentScene.GetScript();

            if (!(sceneScript is Script))
                return CurrentScene.Name;
            if (!CurrentScene.Name.IsEmpty)
                return CurrentScene.Name;

            return sceneScript.ResourceName;
        }

        public Node Scene() => CurrentScene;

        public GdUnitAwaiter.GodotMethodAwaiter<V> AwaitMethod<V>(string methodName) =>
            new GdUnitAwaiter.GodotMethodAwaiter<V>(CurrentScene, methodName);

        public async Task AwaitIdleFrame() => await Task.Run(() => SceneTree.ToSignal(SceneTree, SceneTree.SignalName.ProcessFrame));

        public async Task AwaitMillis(uint timeMillis)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                await Task.Delay(System.TimeSpan.FromMilliseconds(timeMillis), tokenSource.Token);
            }
        }

        public async Task AwaitSignal(string signal, params object[] args) =>
            await GdUnitAwaiter.AwaitSignal(CurrentScene, signal, args);

        public Variant Invoke(string name, params Variant[] args)
        {
            if (!CurrentScene.HasMethod(name))
                throw new MissingMethodException($"The method '{name}' not exist on loaded scene.");
            return CurrentScene.Call(name, args);
        }

        public T GetProperty<T>(string name)
        {
            var property = CurrentScene.Get(name);
            if (property.Obj != null)
                return (T)property.Obj;
            throw new MissingFieldException($"The property '{name}' not exist on loaded scene.");
        }

        public Node FindChild(string name, bool recursive = true, bool owned = false) => CurrentScene.FindChild(name, recursive, owned);

        public void MaximizeView()
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowMoveToForeground();
        }

        public void Dispose()
        {
            DeactivateTimeFactor();
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
            SceneTree.Root.RemoveChild(CurrentScene);
            if (SceneAutoFree)
                CurrentScene.Free();
        }
    }
}
