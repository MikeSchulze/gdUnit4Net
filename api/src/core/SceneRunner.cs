using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GdUnit4
{
    using Executions;
    using Godot;
    using static Assertions;

    public static class GdUnitAwaiter
    {

        public sealed class GodotMethodAwaiter<[Godot.MustBeVariant] V>
        {
            private string MethodName { get; }
            private Node Instance { get; }
            private Variant[] Args { get; }

            public GodotMethodAwaiter(Node instance, string methodName, params Variant[] args)
            {
                Instance = instance;
                MethodName = methodName;
                Args = args;
                if (!Instance.HasMethod(MethodName) && Instance.GetType().GetMethod(methodName) == null)
                    throw new MissingMethodException($"The method '{MethodName}' not exist on loaded scene.");
            }

            public async Task IsEqual(V expected) =>
                await CallAndWaitIsFinished((current) => AssertThat(current).IsEqual(expected));

            public async Task IsNull() =>
                await CallAndWaitIsFinished((current) => AssertThat(current).IsNull());

            public async Task IsNotNull() =>
                await CallAndWaitIsFinished((current) => AssertThat(current).IsNotNull());

            private delegate void Predicate(object? current);

            private async Task CallAndWaitIsFinished(Predicate comperator) => await Task.Run(async () =>
            {
                // sync to main thread
                await ISceneRunner.SyncProcessFrame;
                var value = await GodotObjectExtensions.Invoke(Instance, MethodName, Args);
                comperator(value);
            });
        }

        public static async Task AwaitSignal(this Godot.Node node, string signal, params Godot.Variant[] expectedArgs)
        {
            while (true)
            {
                Variant[] signalArgs = await Engine.GetMainLoop().ToSignal(node, signal);
                if (expectedArgs?.Length == 0 || signalArgs.VariantEquals(expectedArgs))
                    return;
            }
        }
    }
}

namespace GdUnit4.Core
{
    using Godot;
    using Executions;


    internal sealed class SceneRunner : ISceneRunner
    {
        private SceneTree SceneTree { get; set; }
        private Node CurrentScene { get; set; }
        private bool Verbose { get; set; }
        private bool SceneAutoFree { get; set; }
        private Vector2 CurrentMousePos { get; set; }
        private double TimeFactor { get; set; }
        private int SavedIterationsPerSecond { get; set; }
        private InputEvent? LastInputEvent { get; set; }
        private ICollection<Key> KeyOnPress = new HashSet<Key>();
        private ICollection<MouseButton> MouseButtonOnPress = new HashSet<MouseButton>();

        private Guid Uuid { get; set; } = Guid.NewGuid();


        public SceneRunner(string resourcePath, bool autoFree = false, bool verbose = false)
        {
            if (!Godot.FileAccess.FileExists(resourcePath))
                throw new FileNotFoundException($"GdUnitSceneRunner: Can't load scene by given resource path: '{resourcePath}'. The resource not exists.");
            Verbose = verbose;
            SceneAutoFree = autoFree;
            ExecutionContext.RegisterDisposable(this);
            SceneTree = (SceneTree)Godot.Engine.GetMainLoop();
            CurrentScene = ((PackedScene)Godot.ResourceLoader.Load(resourcePath)).Instantiate();
            SceneTree.Root.AddChild(CurrentScene);
            CurrentMousePos = default;
            SavedIterationsPerSecond = Engine.PhysicsTicksPerSecond;
            SetTimeFactor(1.0);
        }

        private void ResetInputToDefault()
        {
            // reset all mouse button to inital state if need
            foreach (var button in MouseButtonOnPress)
            {
                if (Input.IsMouseButtonPressed(button))
                    SimulateMouseButtonRelease(button);
            }
            MouseButtonOnPress.Clear();

            foreach (var key in KeyOnPress)
            {
                if (Input.IsKeyPressed(key))
                    SimulateKeyRelease(key);
            }
            KeyOnPress.Clear();
            Input.FlushBufferedEvents();
        }

        /// <summary>
        /// copy over current active modifiers
        /// </summary>
        /// <param name="inputEvent"></param>
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

        /// <summary>
        /// copy over current active mouse mask and combine with curren mask
        /// </summary>
        /// <param name="inputEvent"></param>
        private void ApplyInputMouseMask(InputEvent inputEvent)
        {
            // first apply last mask
            if (LastInputEvent is InputEventMouse lastInputEvent && inputEvent is InputEventMouse ie)
                ie.ButtonMask |= lastInputEvent.ButtonMask;
            if (inputEvent is InputEventMouseButton inputEventMouseButton)
            {
                MouseButtonMask mask = toMouseButtonMask(inputEventMouseButton.ButtonIndex);
                if (inputEventMouseButton.IsPressed())
                    inputEventMouseButton.ButtonMask |= mask;
                else
                    inputEventMouseButton.ButtonMask ^= mask;
            }
        }

        internal static MouseButtonMask toMouseButtonMask(MouseButton button)
        {
            int button_mask = 1 << ((int)button - 1);
            return (MouseButtonMask)Enum.ToObject(typeof(MouseButtonMask), button_mask);
        }

        /// <summary>
        /// copy over last mouse position if need
        /// </summary>
        /// <param name="inputEvent"></param>
        private void ApplyInputMousePosition(InputEvent inputEvent)
        {
            if (LastInputEvent is InputEventMouse lastInputEvent && inputEvent is InputEventMouseButton ie)
                ie.Position = lastInputEvent.Position;
        }

        /// <summary>
        /// for handling read https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html?highlight=inputevent#how-does-it-work
        /// </summary>
        /// <param name="inputEvent"></param>
        /// <returns></returns>
        private ISceneRunner HandleInputEvent(InputEvent inputEvent)
        {
            if (inputEvent is InputEventMouse ie)
                Input.WarpMouse(ie.Position);
            Input.ParseInputEvent(inputEvent);
            Input.FlushBufferedEvents();

            if (Godot.GodotObject.IsInstanceValid(CurrentScene))
            {
                Print($"	process event {CurrentScene} ({SceneName()}) <- {inputEvent.AsText()}");
                if (CurrentScene.HasMethod("_gui_input"))
                    CurrentScene.Call("_gui_input", inputEvent);
                if (CurrentScene.HasMethod("_unhandled_input"))
                    CurrentScene.Call("_unhandled_input", inputEvent);
                CurrentScene.GetViewport().SetInputAsHandled();
            }
            // save last input event needs to be merged with next InputEventMouseButton
            LastInputEvent = inputEvent;
            return this;
        }

        public ISceneRunner SimulateKeyPress(Key keyCode, bool shiftPressed = false, bool controlPressed = false)
        {
            PrintCurrentFocus();
            var inputEvent = new InputEventKey();
            inputEvent.Pressed = true;
            inputEvent.Keycode = keyCode;
            inputEvent.PhysicalKeycode = keyCode;
            inputEvent.AltPressed = keyCode == Key.Alt;
            inputEvent.ShiftPressed = shiftPressed || keyCode == Key.Shift;
            inputEvent.CtrlPressed = controlPressed || keyCode == Key.Ctrl;
            ApplyInputModifiers(inputEvent);
            KeyOnPress.Add(keyCode);
            return HandleInputEvent(inputEvent);
        }

        public ISceneRunner SimulateKeyPressed(Key keyCode, bool shift = false, bool control = false)
        {
            SimulateKeyPress(keyCode, shift, control);
            SimulateKeyRelease(keyCode, shift, control);
            return this;
        }

        public ISceneRunner SimulateKeyRelease(Key keyCode, bool shiftPressed = false, bool controlPressed = false)
        {
            PrintCurrentFocus();
            var inputEvent = new InputEventKey();
            inputEvent.Pressed = false;
            inputEvent.Keycode = keyCode;
            inputEvent.PhysicalKeycode = keyCode;
            inputEvent.AltPressed = keyCode == Key.Alt;
            inputEvent.ShiftPressed = shiftPressed || keyCode == Key.Shift;
            inputEvent.CtrlPressed = controlPressed || keyCode == Key.Ctrl;
            ApplyInputModifiers(inputEvent);
            KeyOnPress.Remove(keyCode);
            return HandleInputEvent(inputEvent);
        }

        public ISceneRunner SetMousePos(Vector2 position)
        {
            var inputEvent = new InputEventMouseMotion();
            inputEvent.Position = position;
            inputEvent.GlobalPosition = GetGlobalMousePosition();
            ApplyInputModifiers(inputEvent);
            return HandleInputEvent(inputEvent);
        }

        public Vector2 GetMousePosition()
        {
            if (LastInputEvent is InputEventMouse me)
                return me.Position;
            return CurrentScene.GetViewport().GetMousePosition();
        }

        public Vector2 GetGlobalMousePosition() =>
            SceneTree.Root.GetMousePosition();

        public ISceneRunner SimulateMouseMove(Vector2 position)
        {
            var inputEvent = new InputEventMouseMotion();
            inputEvent.Position = position;
            inputEvent.Relative = position - GetMousePosition();
            ApplyInputMouseMask(inputEvent);
            ApplyInputModifiers(inputEvent);
            return HandleInputEvent(inputEvent);
        }


        public async Task SimulateMouseMoveRelative(Vector2 relative, Vector2 speed = default)
        {
            if (LastInputEvent is InputEventMouse lastInputEvent)
            {
                var current_pos = lastInputEvent.Position;
                var final_pos = current_pos + relative;
                double delta_milli = speed.X * 0.1;
                var t = 0.0;

                while (!current_pos.IsEqualApprox(final_pos))
                {
                    t += delta_milli * speed.X;
                    SimulateMouseMove(current_pos);
                    await AwaitMillis((uint)(delta_milli * 1000));
                    current_pos = current_pos.Lerp(final_pos, (float)t);
                }
                SimulateMouseMove(final_pos);
                await ISceneRunner.SyncProcessFrame;
            }
        }


        public ISceneRunner SimulateMouseButtonPressed(MouseButton buttonIndex, bool doubleClick = false)
        {
            SimulateMouseButtonPress(buttonIndex, doubleClick);
            SimulateMouseButtonRelease(buttonIndex);
            return this;
        }

        public ISceneRunner SimulateMouseButtonPress(MouseButton buttonIndex, bool doubleClick = false)
        {
            PrintCurrentFocus();
            var inputEvent = new InputEventMouseButton();
            inputEvent.ButtonIndex = buttonIndex;
            inputEvent.Pressed = true;
            inputEvent.DoubleClick = doubleClick;

            MouseButtonOnPress.Add(buttonIndex);
            ApplyInputMousePosition(inputEvent);
            ApplyInputMouseMask(inputEvent);
            ApplyInputModifiers(inputEvent);
            return HandleInputEvent(inputEvent);
        }

        public ISceneRunner SimulateMouseButtonRelease(MouseButton buttonIndex)
        {
            var inputEvent = new InputEventMouseButton();
            inputEvent.ButtonIndex = buttonIndex;
            inputEvent.Pressed = false;

            MouseButtonOnPress.Remove(buttonIndex);
            ApplyInputMousePosition(inputEvent);
            ApplyInputMouseMask(inputEvent);
            ApplyInputModifiers(inputEvent);
            return HandleInputEvent(inputEvent);
        }

        public ISceneRunner SetTimeFactor(double timeFactor = 1.0)
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
                await ISceneRunner.SyncProcessFrame;
        }

        private void ActivateTimeFactor()
        {
            if (Verbose)
                Console.WriteLine($"ActivateTimeFactor: Engine.TimeScale={TimeFactor}, Engine.PhysicsTicksPerSecond={(int)(SavedIterationsPerSecond * TimeFactor)}");
            Engine.TimeScale = TimeFactor;
            Engine.PhysicsTicksPerSecond = (int)(SavedIterationsPerSecond * TimeFactor);

        }

        private void DeactivateTimeFactor()
        {
            if (Verbose)
                Console.WriteLine($"ActivateTimeFactor: Engine.TimeScale={1}, Engine.PhysicsTicksPerSecond={SavedIterationsPerSecond}");
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
            GDScript? sceneScript = CurrentScene.GetScript().Obj as GDScript;
            if (sceneScript == null)
                return CurrentScene.Name.ToString();
            return sceneScript.ResourceName.GetBaseName();
        }

        public Node Scene() => CurrentScene;

        public GdUnitAwaiter.GodotMethodAwaiter<V> AwaitMethod<[Godot.MustBeVariant] V>(string methodName) =>
            new GdUnitAwaiter.GodotMethodAwaiter<V>(CurrentScene, methodName);

        public async Task AwaitMillis(uint timeMillis)
        {
            using (var tokenSource = new CancellationTokenSource())
            {
                await Task.Delay(System.TimeSpan.FromMilliseconds(timeMillis), tokenSource.Token);
            }
        }

        public async Task AwaitSignal(string signal, params Godot.Variant[] args) =>
            await CurrentScene.AwaitSignal(signal, args);


        public async Task AwaitIdleFrame() => await ISceneRunner.SyncProcessFrame;


        public Variant Invoke(string name, params Variant[] args)
        {
            if (!CurrentScene.HasMethod(name))
                throw new MissingMethodException($"The method '{name}' not exist on loaded scene.");
            return CurrentScene.Call(name, args);
        }

        public dynamic? GetProperty(string name)
        {
            if (!PropertyExists(name))
                throw new MissingFieldException($"The property '{name}' not exist on loaded scene.");
            return CurrentScene.Get(name)!.UnboxVariant();
        }

        public T? GetProperty<T>(string name) => GetProperty(name);


        public void SetProperty(string name, Variant value)
        {
            if (!PropertyExists(name))
                throw new MissingFieldException($"The property '{name}' not exist on loaded scene.");
            CurrentScene.Set(name, value);
        }

        private bool PropertyExists(string name) =>
            CurrentScene.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any((field) => field.Name.Equals(name)) || CurrentScene.GetPropertyList().Any(p => p["name"].VariantEquals(name));

        public Node FindChild(string name, bool recursive = true, bool owned = false) =>
            CurrentScene.FindChild(name, recursive, owned);

        public void MaximizeView()
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            DisplayServer.WindowMoveToForeground();
        }

        internal bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;
            DeactivateTimeFactor();
            ResetInputToDefault();
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
            if (CurrentScene != null)
            {
                SceneTree.Root.RemoveChild(CurrentScene);
                if (SceneAutoFree)
                    CurrentScene.Free();
            }
            disposed = true;
        }
    }
}
