// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using System.Runtime.CompilerServices;
using System.Threading;

using Api;

/// <summary>
///     Manages the logging pipeline: holds the root and task-local logger registry,
///     and provides class-scoped logger instances for internal use.
/// </summary>
/// <remarks>
///     Register the engine logger once at startup:
///     <code>
///     LoggerFactory.Init(myLogger);
///     </code>
///     Obtain a class-scoped logger (typically as a static field):
///     <code>
///     private static readonly ITestEngineLogger Logger = LoggerFactory.GetLogger&lt;MyClass&gt;();
///     </code>
/// </remarks>
internal static class LoggerFactory
{
    private static readonly AsyncLocal<ScopeLogger?> Contextual = new();
    private static ITestEngineLogger root = NoOpTestEngineLogger.Instance;

    /// <summary>Gets the engine-wide root logger set by <see cref="Init" />. Never null; defaults to a no-op sentinel.</summary>
    internal static ITestEngineLogger Root
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => root;
    }

    /// <summary>Gets the active logger for the current task: the registered context logger if one is active, otherwise <see cref="Root" />.</summary>
    internal static ITestEngineLogger Current
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Contextual.Value ?? root;
    }

    /// <summary>
    ///     Initializes the root logger used as the fallback for all class-scoped loggers.
    ///     Called once per engine instance at startup; replaces any previously set root logger.
    /// </summary>
    internal static void Init(ITestEngineLogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _ = Interlocked.Exchange(ref root, logger);
    }

    /// <summary>
    ///     Creates a task-local log scope for the current async execution context.
    ///     The scope logger wraps the immutable <see cref="Root" /> and prefixes every message with
    ///     <c>[<paramref name="scopeId" />]</c>, leaving other parallel tasks unaffected.
    ///     Dispose the returned <see cref="LogScope" /> to end the scope automatically.
    /// </summary>
    internal static LogScope CreateScope(string scopeId)
    {
        Contextual.Value = new ScopeLogger(root, scopeId ?? throw new ArgumentNullException(nameof(scopeId)));
        return default;
    }

    /// <summary>Gets the active scope logger for the current task, or <see langword="null" /> if none is active.</summary>
    internal static ScopeLogger? GetScope() => Contextual.Value;

    /// <summary>Returns a class-scoped logger for <typeparamref name="T" /> that delegates to <see cref="Current" /> at dispatch time.</summary>
    internal static ITestEngineLogger GetLogger<T>() => new TypedLogger(typeof(T));

    /// <summary>Returns a class-scoped logger for <paramref name="type" /> that delegates to <see cref="Current" /> at dispatch time.</summary>
    internal static ITestEngineLogger GetLogger(Type type) => new TypedLogger(type);

    /// <summary>Resets the root logger to the no-op sentinel. Call when the engine instance is disposed.</summary>
    internal static void Dispose()
        => Interlocked.Exchange(ref root, NoOpTestEngineLogger.Instance);

    private static void EndScope() => Contextual.Value = null;

    /// <summary>Ends the task-local log scope created by <see cref="CreateScope" /> when disposed.</summary>
    internal readonly struct LogScope : IDisposable
    {
        public void Dispose() => EndScope();
    }
}
