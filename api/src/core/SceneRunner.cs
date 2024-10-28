namespace GdUnit4.Core;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Extensions;

using Godot;

using ExecutionContext = Execution.ExecutionContext;

/// <summary>
///     A helper to simulate mouse moving form a source to final position
/// </summary>
internal partial class MouseMoveTask : Node, IDisposable
{
    public MouseMoveTask(Vector2 currentPosition, Vector2 finalPosition)
    {
        CurrentMousePosition = currentPosition;
        FinalMousePosition = finalPosition;
    }

    private Vector2 CurrentMousePosition { get; set; }

    private Vector2 FinalMousePosition { get; }

    public new void Dispose()
    {
        QueueFree();
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task WaitOnFinalPosition(ISceneRunner sceneRunner, double time, Tween.TransitionType transitionType)
    {
        using var tween = sceneRunner.Scene().CreateTween();
        tween.TweenProperty(this, "CurrentMousePosition", FinalMousePosition, time).SetTrans(transitionType);
        tween.Play();

        while (!sceneRunner.GetMousePosition().IsEqualApprox(FinalMousePosition))
        {
            sceneRunner.SimulateMouseMove(CurrentMousePosition);
            await ISceneRunner.SyncProcessFrame;
        }

        sceneRunner.SimulateMouseMove(FinalMousePosition);
        await ISceneRunner.SyncProcessFrame;
    }
}

internal sealed class SceneRunner : ISceneRunner
{
    private readonly ICollection<string> actionOnPress = new HashSet<string>();
    private readonly Node currentScene;
    private readonly ICollection<Key> keyOnPress = new HashSet<Key>();
    private readonly ICollection<MouseButton> mouseButtonOnPress = new HashSet<MouseButton>();

    public SceneRunner(string resourcePath, bool autoFree = false, bool verbose = false) : this(LoadScene(resourcePath), autoFree, verbose)
    {
    }


    public SceneRunner(Node currentScene, bool autoFree = false, bool verbose = false)
    {
        Verbose = verbose;
        SceneAutoFree = autoFree;
        ExecutionContext.RegisterDisposable(this);
        SceneTree = (SceneTree)Engine.GetMainLoop();
        this.currentScene = currentScene;
        SceneTree.Root.AddChild(this.currentScene);
        SavedIterationsPerSecond = Engine.PhysicsTicksPerSecond;
        SetTimeFactor();
    }

    private SceneTree SceneTree { get; }
    private bool Verbose { get; }
    private bool SceneAutoFree { get; }
    private double TimeFactor { get; set; }
    private int SavedIterationsPerSecond { get; }
    private InputEvent? LastInputEvent { get; set; }

    private bool IsDisposed { get; set; }

    public ISceneRunner SimulateActionPress(string action)
    {
        var inputEvent = new InputEventAction
        {
            Pressed = true,
            Action = action
        };
        actionOnPress.Add(action);
        return HandleInputEvent(inputEvent);
    }

    public ISceneRunner SimulateActionPressed(string action)
    {
        SimulateActionPress(action);
        SimulateActionRelease(action);
        return this;
    }

    public ISceneRunner SimulateActionRelease(string action)
    {
        var inputEvent = new InputEventAction
        {
            Pressed = false,
            Action = action
        };
        actionOnPress.Remove(action);
        return HandleInputEvent(inputEvent);
    }

    public ISceneRunner SimulateKeyPress(Key keyCode, bool shiftPressed = false, bool controlPressed = false)
    {
        PrintCurrentFocus();
        var inputEvent = new InputEventKey
        {
            Pressed = true,
            Keycode = keyCode,
            PhysicalKeycode = keyCode,
            AltPressed = keyCode == Key.Alt,
            ShiftPressed = shiftPressed || keyCode == Key.Shift,
            CtrlPressed = controlPressed || keyCode == Key.Ctrl
        };
        ApplyInputModifiers(inputEvent);
        keyOnPress.Add(keyCode);
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
        var inputEvent = new InputEventKey
        {
            Pressed = false,
            Keycode = keyCode,
            PhysicalKeycode = keyCode,
            AltPressed = keyCode == Key.Alt,
            ShiftPressed = shiftPressed || keyCode == Key.Shift,
            CtrlPressed = controlPressed || keyCode == Key.Ctrl
        };
        ApplyInputModifiers(inputEvent);
        keyOnPress.Remove(keyCode);
        return HandleInputEvent(inputEvent);
    }

    public ISceneRunner SetMousePos(Vector2 position)
    {
        var inputEvent = new InputEventMouseMotion
        {
            Position = position,
            GlobalPosition = GetGlobalMousePosition()
        };
        ApplyInputModifiers(inputEvent);
        return HandleInputEvent(inputEvent);
    }

    public Vector2 GetMousePosition()
    {
        if (LastInputEvent is InputEventMouse me)
            return me.Position;
        return currentScene.GetViewport().GetMousePosition();
    }

    public Vector2 GetGlobalMousePosition() =>
        SceneTree.Root.GetMousePosition();

    public ISceneRunner SimulateMouseMove(Vector2 position)
    {
        var inputEvent = new InputEventMouseMotion
        {
            Position = position,
            Relative = position - GetMousePosition()
        };
        ApplyInputMouseMask(inputEvent);
        ApplyInputModifiers(inputEvent);
        return HandleInputEvent(inputEvent);
    }

    public async Task SimulateMouseMoveRelative(Vector2 relative, double time = 1.0, Tween.TransitionType transitionType = Tween.TransitionType.Linear)
        => await SimulateMouseMoveAbsolute(GetMousePosition() + relative, time, transitionType);

    public async Task SimulateMouseMoveAbsolute(Vector2 position, double time = 1.0, Tween.TransitionType transitionType = Tween.TransitionType.Linear)
    {
        using var mouseMove = new MouseMoveTask(GetMousePosition(), position);
        await mouseMove.WaitOnFinalPosition(this, time, transitionType);
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
        var inputEvent = new InputEventMouseButton
        {
            ButtonIndex = buttonIndex,
            Pressed = true,
            DoubleClick = doubleClick
        };

        mouseButtonOnPress.Add(buttonIndex);
        ApplyInputMousePosition(inputEvent);
        ApplyInputMouseMask(inputEvent);
        ApplyInputModifiers(inputEvent);
        return HandleInputEvent(inputEvent);
    }

    public ISceneRunner SimulateMouseButtonRelease(MouseButton buttonIndex)
    {
        var inputEvent = new InputEventMouseButton
        {
            ButtonIndex = buttonIndex,
            Pressed = false
        };

        mouseButtonOnPress.Remove(buttonIndex);
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
        for (var frame = 0; frame < frames; frame++)
            await AwaitMillis(deltaPeerFrame);
    }

    public async Task SimulateFrames(uint frames)
    {
        var timeShiftFrames = Math.Max(1, frames / TimeFactor);
        for (var frame = 0; frame < timeShiftFrames; frame++)
            await ISceneRunner.SyncProcessFrame;
    }

    public Node Scene() => currentScene;

    public GdUnitAwaiter.GodotMethodAwaiter<TVariant> AwaitMethod<[MustBeVariant] TVariant>(string methodName) where TVariant : notnull
        => new(currentScene, methodName);

    public async Task AwaitMillis(uint timeMillis)
    {
        using var tokenSource = new CancellationTokenSource();
        await Task.Delay(TimeSpan.FromMilliseconds(timeMillis), tokenSource.Token);
    }

    public async Task AwaitSignal(string signal, params Variant[] args) =>
        await currentScene.AwaitSignal(signal, args);

    public async Task AwaitIdleFrame() => await ISceneRunner.SyncProcessFrame;


    public Variant Invoke(string name, params Variant[] args)
        => GodotObjectExtensions.Invoke(currentScene, name, args)
            .GetAwaiter()
            .GetResult()
            .ToVariant();

    public async Task<Variant> InvokeAsync(string name, params Variant[] args)
    {
        var result = await GodotObjectExtensions.Invoke(currentScene, name, args);
        return result.ToVariant();
    }

    public dynamic? GetProperty(string name)
    {
        if (!PropertyExists(name))
            throw new MissingFieldException($"The property '{name}' not exist on loaded scene.");
        return currentScene.Get(name).UnboxVariant();
    }

    public T? GetProperty<T>(string name) => GetProperty(name);

    public void SetProperty(string name, Variant value)
    {
        if (!PropertyExists(name))
            throw new MissingFieldException($"The property '{name}' not exist on loaded scene.");
        currentScene.Set(name, value);
    }

    public Node FindChild(string name, bool recursive = true, bool owned = false) =>
        currentScene.FindChild(name, recursive, owned);

    public void MaximizeView()
    {
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowMoveToForeground();
    }

    public void Dispose()
    {
        if (IsDisposed)
            return;
        DeactivateTimeFactor();
        ResetInputToDefault();
        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
        SceneTree.Root.RemoveChild(currentScene);
        if (SceneAutoFree && GodotObject.IsInstanceValid(currentScene))
            currentScene.Free();

        IsDisposed = true;
    }

    private static Node LoadScene(string resourcePath)
    {
        if (!ResourceLoader.Exists(resourcePath))
            throw new FileNotFoundException($"GdUnitSceneRunner: Can't load scene by given resource path: '{resourcePath}'. The resource does not exists.");
        if (!resourcePath.EndsWith(".tscn") && !resourcePath.EndsWith(".scn") && !resourcePath.StartsWith("uid://"))
            throw new ArgumentException($"GdUnitSceneRunner: The given resource: '{resourcePath}' is not a scene.");

        return ((PackedScene)ResourceLoader.Load(resourcePath)).Instantiate();
    }

    private void ResetInputToDefault()
    {
        // reset all mouse button to initial state if is need
        foreach (var button in mouseButtonOnPress)
            if (Input.IsMouseButtonPressed(button))
                SimulateMouseButtonRelease(button);
        mouseButtonOnPress.Clear();

        foreach (var key in keyOnPress)
            if (Input.IsKeyPressed(key))
                SimulateKeyRelease(key);
        keyOnPress.Clear();

        foreach (var action in actionOnPress)
            if (Input.IsActionPressed(action))
                SimulateActionRelease(action);
        actionOnPress.Clear();

        Input.FlushBufferedEvents();
    }

    /// <summary>
    ///     copy over current active modifiers
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
    ///     copy over current active mouse mask and combine with current mask
    /// </summary>
    /// <param name="inputEvent"></param>
    private void ApplyInputMouseMask(InputEvent inputEvent)
    {
        // first apply last mask
        if (LastInputEvent is InputEventMouse lastInputEvent && inputEvent is InputEventMouse ie)
            ie.ButtonMask |= lastInputEvent.ButtonMask;
        if (inputEvent is InputEventMouseButton inputEventMouseButton)
        {
            var mask = ToMouseButtonMask(inputEventMouseButton.ButtonIndex);
            if (inputEventMouseButton.IsPressed())
                inputEventMouseButton.ButtonMask |= mask;
            else
                inputEventMouseButton.ButtonMask ^= mask;
        }
    }

    internal static MouseButtonMask ToMouseButtonMask(MouseButton button)
    {
        var button_mask = 1 << ((int)button - 1);
        return (MouseButtonMask)Enum.ToObject(typeof(MouseButtonMask), button_mask);
    }

    /// <summary>
    ///     copy over last mouse position if is need
    /// </summary>
    /// <param name="inputEvent"></param>
    private void ApplyInputMousePosition(InputEvent inputEvent)
    {
        if (LastInputEvent is InputEventMouse lastInputEvent && inputEvent is InputEventMouseButton ie)
            ie.Position = lastInputEvent.Position;
    }

    /// <summary>
    ///     for handling read https://docs.godotengine.org/en/stable/tutorials/inputs/inputevent.html?highlight=inputevent#how-does-it-work
    /// </summary>
    /// <param name="inputEvent"></param>
    /// <returns></returns>
    private SceneRunner HandleInputEvent(InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouse mouseEvent)
            Input.WarpMouse(mouseEvent.Position);
        Input.ParseInputEvent(inputEvent);
        if (inputEvent is InputEventAction actionEvent)
            HandleActionEvent(actionEvent);
        Input.FlushBufferedEvents();

        if (GodotObject.IsInstanceValid(currentScene))
        {
            Print($"	process event {currentScene} ({SceneName()}) <- {inputEvent.AsText()}");
            if (currentScene.HasMethod("_gui_input"))
                currentScene.Call("_gui_input", inputEvent);
            if (currentScene.HasMethod("_unhandled_input"))
                currentScene.Call("_unhandled_input", inputEvent);
            currentScene.GetViewport().SetInputAsHandled();
        }

        // save last input event needs to be merged with next InputEventMouseButton
        LastInputEvent = inputEvent;
        return this;
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private static bool HandleActionEvent(InputEventAction actionEvent)
    {
        if (!InputMap.EventIsAction(actionEvent, actionEvent.Action, true))
            return false;
        if (actionEvent.IsPressed())
            Input.ActionPress(actionEvent.Action, InputMap.ActionGetDeadzone(actionEvent.Action));
        else
            Input.ActionRelease(actionEvent.Action);
        return true;
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
            Console.WriteLine(message, args);
    }

    private void PrintCurrentFocus()
    {
        if (!Verbose)
            return;
        var focusedNode = (currentScene as Control)?.Owner; //.GetFocusOwner();

        if (focusedNode != null)
            Console.WriteLine("	focus on {0}", focusedNode);
        else
            Console.WriteLine("	no focus set");
    }

    private string SceneName()
    {
        if (currentScene.GetScript().Obj is not GDScript sceneScript)
            return currentScene.Name.ToString();
        return sceneScript.ResourceName.GetBaseName();
    }

    private bool PropertyExists(string name)
        => currentScene.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) != null
           || currentScene.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(field => field.Name.Equals(name, StringComparison.Ordinal))
           || currentScene.GetPropertyList().Any(p => p["name"].VariantEquals(name));
}
