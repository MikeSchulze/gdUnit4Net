// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4;

using System;
using System.Threading;
using System.Threading.Tasks;

using Core;
using Core.Execution.Exceptions;
using Core.Extensions;

using Godot;

/// <summary>
///     Scene runner to test interactions like keyboard/mouse inputs on a Godot scene.
/// </summary>
public interface ISceneRunner : IDisposable
{
    /// <summary>
    ///     Gets a utility to synchronize the current thread with the Godot physics thread.
    ///     This can be used to await the completion of a single physics frame in Godot.
    /// </summary>
    static SignalAwaiter SyncProcessFrame => GodotObjectExtensions.SyncProcessFrame;

    /// <summary>
    ///     Gets a util to synchronize the current thread with the Godot physics thread.
    /// </summary>
    public static SignalAwaiter SyncPhysicsFrame => GodotObjectExtensions.SyncPhysicsFrame;

    /// <summary>
    ///     Loads a scene into the SceneRunner to be simulated.
    /// </summary>
    /// <param name="resourcePath">The path to the scene resource.</param>
    /// <param name="autoFree">If true the loaded scene will be automatic freed when the runner is freed.</param>
    /// <param name="verbose">Prints detailed infos on scene simulation.</param>
    /// <returns></returns>
    static ISceneRunner Load(string resourcePath, bool autoFree = false, bool verbose = false) => new SceneRunner(resourcePath, autoFree, verbose);

    static ISceneRunner Load(Node currentScene, bool autoFree = false, bool verbose = false) => new SceneRunner(currentScene, autoFree, verbose);

    /// <summary>
    ///     Simulates that an action has been pressed.
    /// </summary>
    /// <param name="action">The name of the action, e.g., "ui_up".</param>
    /// <returns>The SceneRunner instance.</returns>
    ISceneRunner SimulateActionPressed(string action);

    /// <summary>
    ///     Simulates that an action is press.
    /// </summary>
    /// <param name="action">The name of the action, e.g., "ui_up".</param>
    /// <returns>The SceneRunner instance.</returns>
    ISceneRunner SimulateActionPress(string action);

    /// <summary>
    ///     Simulates that an action has been released.
    /// </summary>
    /// <param name="action">The name of the action, e.g., "ui_up".</param>
    /// <returns>The SceneRunner instance.</returns>
    ISceneRunner SimulateActionRelease(string action);

    /// <summary>
    ///     Simulates that a key has been pressed.
    /// </summary>
    /// <param name="keyCode">the key code e.g. 'Key.Enter'.</param>
    /// <param name="shift">false by default set to true if simulate shift is press.</param>
    /// <param name="control">false by default set to true if simulate control is press.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateKeyPressed(Key keyCode, bool shift = false, bool control = false);

    /// <summary>
    ///     Simulates that a key is pressed.
    /// </summary>
    /// <param name="keyCode">the key code e.g. 'Key.Enter'.</param>
    /// <param name="shift">false by default set to true if simulate shift is press.</param>
    /// <param name="control">false by default set to true if simulate control is press.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateKeyPress(Key keyCode, bool shift = false, bool control = false);

    /// <summary>
    ///     Simulates that a key has been released.
    /// </summary>
    /// <param name="keyCode">the key code e.g. 'Key.Enter'.</param>
    /// <param name="shift">false by default set to true if simulate shift is press.</param>
    /// <param name="control">false by default set to true if simulate control is press.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateKeyRelease(Key keyCode, bool shift = false, bool control = false);

    /// <summary>
    ///     Simulates a mouse moved to final position.
    /// </summary>
    /// <param name="position">The position in x/y coordinates.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateMouseMove(Vector2 position);

    /// <summary>
    ///     Simulates a mouse move to the absolute coordinates.
    /// </summary>
    /// <param name="position">The final position of the mouse.</param>
    /// <param name="time">The time to move the mouse to the final position in seconds (default is 1 second).</param>
    /// <param name="transitionType">Sets the type of transition used (default is Linear).</param>
    /// <returns>SceneRunner.</returns>
    Task SimulateMouseMoveAbsolute(Vector2 position, double time = 1.0, Tween.TransitionType transitionType = Tween.TransitionType.Linear);

    /// <summary>
    ///     Simulates a mouse move to the relative coordinates (offset).
    /// </summary>
    /// <param name="relative">The relative position, e.g. the mouse position offset.</param>
    /// <param name="time">The time to move the mouse by the relative position in seconds (default is 1 second).</param>
    /// <param name="transitionType">Sets the type of transition used (default is Linear).</param>
    /// <returns>SceneRunner.</returns>
    Task SimulateMouseMoveRelative(Vector2 relative, double time = 1.0, Tween.TransitionType transitionType = Tween.TransitionType.Linear);

    /// <summary>
    ///     Simulates a mouse button pressed.
    /// </summary>
    /// <param name="button">The mouse button identifier, one of the MouseButton or button wheel constants.</param>
    /// <param name="doubleClick">Indicates the button was double-clicked.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateMouseButtonPressed(MouseButton button, bool doubleClick = false);

    /// <summary>
    ///     Simulates a mouse button press. (holding).
    /// </summary>
    /// <param name="button">The mouse button identifier, one of the MouseButton or button wheel constants.</param>
    /// <param name="doubleClick">Indicates the button was double-clicked.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateMouseButtonPress(MouseButton button, bool doubleClick = false);

    /// <summary>
    ///     Simulates a mouse button released.
    /// </summary>
    /// <param name="button">The mouse button identifier, one of the MouseButton or button wheel constants.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SimulateMouseButtonRelease(MouseButton button);

    /// <summary>
    ///     Sets the mouse cursor to given position relative to the viewport.
    /// </summary>
    /// <param name="position">The absolute mouse position.</param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SetMousePos(Vector2 position);

    /// <summary>
    ///     Gets the current mouse position of the current viewport.
    /// </summary>
    /// <returns>Vector2.</returns>
    Vector2 GetMousePosition();

    /// <summary>
    ///     Gets the current global mouse position of the current window.
    /// </summary>
    /// <returns>Vector2.</returns>
    Vector2 GetGlobalMousePosition();

    /// <summary>
    ///     Sets how fast or slow the scene simulation is processed (clock ticks versus the real).
    ///     <code>
    ///     // It defaults to 1.0. A value of 2.0 means the game moves twice as fast as real life,
    ///     // whilst a value of 0.5 means the game moves at half the regular speed
    /// </code>
    /// </summary>
    /// <param name="timeFactor"></param>
    /// <returns>SceneRunner.</returns>
    ISceneRunner SetTimeFactor(double timeFactor = 1.0);

    /// <summary>
    ///     Simulates scene processing for a certain number of frames by given delta peer frame by ignoring the current time
    ///     factor.
    ///     <code>
    ///     // Waits until 100 frames are processed with a delta of 20ms peer frame
    ///     await runner.SimulateFrames(100, 20);
    /// </code>
    /// </summary>
    /// <param name="frames">amount of frames to process.</param>
    /// <param name="deltaPeerFrame">the time delta between a frame in milliseconds.</param>
    /// <returns>Task to wait.</returns>
    Task SimulateFrames(uint frames, uint deltaPeerFrame);

    /// <summary>
    ///     Simulates scene processing for a certain number of frames.
    ///     <example>
    ///         <code>
    ///     // Waits until 100 frames are processed
    ///     await runner.SimulateFrames(100);
    /// </code>
    ///     </example>
    /// </summary>
    /// <param name="frames">amount of frames to process.</param>
    /// <returns>Task to wait.</returns>
    Task SimulateFrames(uint frames);

    /// <summary>
    ///     Waits until next frame is processed (signal idle_frame).
    ///     <example>
    ///         <code>
    ///     // Waits until next frame is processed
    ///     await runner.AwaitIdleFrame();
    /// </code>
    ///     </example>
    /// </summary>
    /// <returns>Task to wait.</returns>
    Task AwaitIdleFrame();

    /// <summary>
    ///     Returns a method awaiter to wait for a specific method result.
    ///     <example>
    ///         <code>
    ///     // Waits until '10' is returned by the method 'calculateX()' or will be interrupted after a timeout of 3s
    ///     await runner.AwaitMethod("calculateX").IsEqual(10).WithTimeout(3000);
    /// </code>
    ///     </example>
    /// </summary>
    /// <typeparam name="TValue">The expected result type.</typeparam>
    /// <param name="methodName">The name of the method to wait.</param>
    /// <returns>GodotMethodAwaiter.</returns>
    GdUnitAwaiter.GodotMethodAwaiter<TValue> AwaitMethod<[MustBeVariant] TValue>(string methodName)
        where TValue : notnull;

    /// <summary>
    ///     Waits for given signal is emitted.
    ///     <example>
    ///         <code>
    ///     // Waits for signal "mySignal" is emitted by the scene.
    ///     await runner.AwaitSignal("mySignal");
    /// </code>
    ///     </example>
    /// </summary>
    /// <param name="signal">The name of the signal to wait.</param>
    /// <param name="args">An optional set of signal arguments.</param>
    /// <returns>Task to wait.</returns>
    Task AwaitSignal(string signal, params Variant[] args);

    /// <summary>
    ///     Waits for a specific amount of milliseconds.
    ///     <example>
    ///         <code>
    ///     // Waits for two seconds
    ///     await runner.AwaitMillis(2000);
    /// </code>
    ///     </example>
    /// </summary>
    /// <param name="timeMillis">Seconds to wait. 1.0 for one Second.</param>
    /// <returns>Task to wait.</returns>
    Task AwaitMillis(uint timeMillis);

    /// <summary>
    ///     Waits for all input events to be processed by flushing any buffered input events and then awaiting a full cycle of
    ///     both the process and physics frames.
    ///     This is typically used to ensure that any simulated or queued inputs are fully processed before proceeding with the
    ///     next steps in the scene.
    ///     It's essential for reliable input simulation or when synchronizing logic based on inputs.
    ///     <example>
    ///         <code>
    ///     runner.SetMousePos(new Vector2(60, 20))
    ///         .SimulateMouseButtonPressed(MouseButton.Left);
    ///     await runner.AwaitInputProcessed()  // Ensure all inputs are processed before continuing
    ///     </code>
    ///     </example>
    /// </summary>
    /// <returns>
    ///     <placeholder>A <see cref="Task" /> representing the asynchronous operation.</placeholder>
    /// </returns>
    async Task AwaitInputProcessed()
    {
        if (Scene().ProcessMode != Node.ProcessModeEnum.Disabled)
            Input.FlushBufferedEvents();

        await SyncProcessFrame;
        await SyncPhysicsFrame;
    }

    /// <summary>
    ///     Access to current running scene.
    /// </summary>
    /// <returns>Node.</returns>
    Node Scene();

    /// <summary>
    ///     Shows the running scene and moves the window to the foreground.
    /// </summary>
    void MaximizeView();

    /// <summary>
    ///     Invokes the method by given name and arguments.
    /// </summary>
    /// <param name="name">The name of method to invoke.</param>
    /// <param name="args">The function arguments.</param>
    /// <returns>The return value of invoked method.</returns>
    /// <exception cref="MissingMethodException" />
    Variant Invoke(string name, params Variant[] args);

    /// <summary>
    ///     Invokes an async method by given name and arguments.
    /// </summary>
    /// <param name="name">The name of method to invoke.</param>
    /// <param name="args">The function arguments.</param>
    /// <returns>The return value of invoked method.</returns>
    /// <exception cref="MissingMethodException" />
    Task<Variant> InvokeAsync(string name, params Variant[] args);

    /// <summary>
    ///     Returns the value of the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <returns>The value of the property.</returns>
    /// <exception cref="MissingFieldException">Thrown when the property is not found.</exception>
    dynamic? GetProperty(string name);

    /// <summary>
    ///     Returns the value of the property with the specified name.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="name">The name of the property.</param>
    /// <returns>The value of the property.</returns>
    /// <exception cref="MissingFieldException">Thrown when the property is not found.</exception>
    T? GetProperty<T>(string name);

    /// <summary>
    ///     Sets the value of the property with the specified name.
    /// </summary>
    /// <param name="name">The name of the property.</param>
    /// <param name="value">The value to set for the property.</param>
    void SetProperty(string name, Variant value);

    /// <summary>
    ///     Finds the node by given name.
    /// </summary>
    /// <param name="name">The name of node to find.</param>
    /// <param name="recursive">Allow recursive search.</param>
    /// <param name="owned">If owned is true, only descendants with a valid owner node are checked.</param>
    /// <returns>The node if found or Null.</returns>
    Node FindChild(string name, bool recursive = true, bool owned = false);
}

public static class SceneRunnerExtensions
{
    public static async Task WithTimeout(this Task task, int timeoutMillis)
    {
        using var timeoutCts = new CancellationTokenSource();
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token);
        try
        {
            var timeoutTask = Task.Delay(timeoutMillis, timeoutCts.Token);
            var completedTask = await Task.WhenAny(task, timeoutTask);
            if (completedTask != task)
            {
                // if a signal task token registered we need to be cancel first
                var data = Thread.GetData(Thread.GetNamedDataSlot("SignalCancellationToken"));
                if (data is CancellationTokenSource cancelToken)
                    cancelToken.Cancel();
                var lineNumber = GdUnitExtensions.GetWithTimeoutLineNumber();
                throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
            }

            await task; // Propagate any exceptions from the task
        }
        finally
        {
            timeoutCts.Cancel();
            linkedCts.Cancel();
        }
    }
}
