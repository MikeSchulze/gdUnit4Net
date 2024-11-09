namespace GdUnit4.Core.Hooks;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

using Execution.Exceptions;

using Godot;

using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
///     A thread-safe singleton hook for intercepting and handling Godot exceptions.
///     This class allows multiple subscribers to handle exceptions that occur within Godot,
///     while maintaining thread safety and proper cleanup.
/// </summary>
/// <remarks>
///     Thread Safety:
///     - Uses AsyncLocal for thread-specific exception handling state
///     - Thread-safe singleton initialization using double-check locking
///     Disposal:
///     - Implements IDisposable for proper cleanup of AppDomain event subscriptions
///     - Thread-safe disposal of the singleton instance
///     Exception Handling:
///     - Captures FirstChanceException events from the AppDomain
///     - Supports multiple exception handlers through a subscription model
///     - Prevents recursive exception handling within the same thread
///     - Allows certain exception types to be ignored (e.g., TestFailedException)
/// </remarks>
public sealed class GodotExceptionHook : IDisposable
{
    private static GodotExceptionHook? instance;
    private static readonly object Lock = new();

    // Types of exceptions that should be ignored during test execution
    private static readonly HashSet<Type> IgnoredExceptionTypes = new()
    {
        typeof(TestFailedException),
        typeof(AssertFailedException),
        typeof(UnitTestAssertException)
    };

    // Storage for exception handlers
    private static readonly List<Action<Exception>> ExceptionHandlers = new();

    // Thread-local flag to prevent recursive exception handling
    private readonly AsyncLocal<bool> isHandlingException = new();

    private GodotExceptionHook()
    {
        try
        {
            AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
            GD.Print("GodotExceptionHook initialized successfully");
        }
        catch (Exception ex)
        {
            GD.PushError($"Failed to initialize GodotExceptionHook: {ex}");
            throw;
        }
    }

    /// <summary>
    ///     Gets the singleton instance of GodotExceptionHook.
    ///     Creates a new instance if one doesn't exist.
    /// </summary>
    /// <returns>The singleton instance</returns>
    /// <remarks>Thread-safe through double-check locking pattern</remarks>
    public static GodotExceptionHook Instance
    {
        get
        {
            if (instance != null) return instance;
            lock (Lock)
                instance ??= new GodotExceptionHook();

            return instance;
        }
    }

    public void Dispose()
    {
        if (instance == null)
            return;

        AppDomain.CurrentDomain.FirstChanceException -= OnFirstChanceException;

        lock (Lock)
            instance = null;
    }

    /// <summary>
    ///     Subscribes a handler to receive exception notifications.
    /// </summary>
    /// <param name="handler">The action to execute when an exception occurs</param>
    /// <returns>An IDisposable that, when disposed, unsubscribes the handler</returns>
    /// <remarks>
    ///     Each handler is stored in a list.
    ///     The returned IDisposable ensures proper cleanup of handlers.
    /// </remarks>
    public static IDisposable Subscribe(Action<Exception> handler)
    {
        lock (ExceptionHandlers) ExceptionHandlers.Add(handler);

        return new DisposableScope(() =>
        {
            lock (ExceptionHandlers)
                if (ExceptionHandlers.Contains(handler))
                    ExceptionHandlers.Remove(handler);
        });
    }

    private void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs e)
    {
        if (isHandlingException.Value)
            return;

        try
        {
            if (IsSceneProcessing())
            {
                isHandlingException.Value = true;
                HandleException(e.Exception);
            }
        }
        finally
        {
            isHandlingException.Value = false;
        }
    }

    private static bool IsSceneProcessing()
    {
        // Look for scene processing methods in the stack trace
        var stackTrace = new StackTrace(true);
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            if (method == null) continue;

            var declaringType = method.DeclaringType;
            if (declaringType == null) continue;

            // Check for scene processing methods
            if (IsSceneProcessingMethod(method.Name, declaringType))
                return true;
        }

        return false;
    }

    private static bool IsSceneProcessingMethod(string methodName, Type declaringType)
    {
        // Check for common Godot processing methods
        if (methodName is "_Process" or "_PhysicsProcess" or "_Input" or "_UnhandledInput" or "_Ready")
            // Check if the declaring type is or inherits from Node
            return typeof(Node).IsAssignableFrom(declaringType) || typeof(RefCounted).IsAssignableFrom(declaringType);
        return false;
    }

    private void HandleException(Exception ex)
    {
        try
        {
            lock (ExceptionHandlers)
                if (ExceptionHandlers.Count == 0)
                    return;

            if (ShouldIgnoreException(ex))
                return;

            lock (ExceptionHandlers)
                foreach (var handler in ExceptionHandlers)
                    handler(ex);
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error in 'GodotExceptionHook' exception handler: {e}");
        }
    }

    private static bool ShouldIgnoreException(Exception ex)
    {
        if (ex is TargetInvocationException)
        {
            var ei = ExceptionDispatchInfo.Capture(ex.InnerException ?? ex);
            return ShouldIgnoreException(ei.SourceException);
        }

        return IgnoredExceptionTypes.Contains(ex.GetType());
    }

    /// <summary>
    ///     Provides debug information about current subscribers.
    /// </summary>
    /// <returns>A string containing information about all registered handlers</returns>
    public static string DebugSubscribersInfo()
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (ExceptionHandlers.Count == 0)
            return "No handlers registered";

        // Take a snapshot to prevent modification during string building
        Action<Exception>[] handlersSnapshot;
        lock (ExceptionHandlers) handlersSnapshot = ExceptionHandlers.ToArray();

        var info = new StringBuilder();
        info.AppendLine($"Total handlers: {handlersSnapshot.Length}");
        for (var i = 0; i < handlersSnapshot.Length; i++)
        {
            var handler = handlersSnapshot[i];
            var className = handler.Target?.GetType().ReflectedType?.FullName;
            info.AppendLine($"[{i}] {className ?? "static"}.{handler.Method.Name}");
        }

        return info.ToString();
    }

    /// <summary>
    ///     A value type that implements IDisposable for efficient cleanup of subscribed handlers.
    /// </summary>
    private readonly struct DisposableScope : IDisposable
    {
        private readonly Action cleanupAction;

        public DisposableScope(Action cleanupAction) => this.cleanupAction = cleanupAction;

        public void Dispose() => cleanupAction?.Invoke();
    }
}
