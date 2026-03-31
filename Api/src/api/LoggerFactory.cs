// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Api;

using Core.Logging;

/// <summary>
///     Factory for obtaining class-scoped loggers that integrate with <see cref="LogCapture" />.
/// </summary>
/// <remarks>
///     Obtain a scoped logger in any class:
///     <code>
///     private static readonly ITestEngineLogger Logger = LoggerFactory.GetLogger&lt;MyClass&gt;();
///     </code>
///     Log messages are forwarded to any active <see cref="LogCapture" /> scopes watching that class,
///     and to the global engine logger.
/// </remarks>
public static class LoggerFactory
{
    /// <summary>
    ///     Returns a class-scoped logger for <typeparamref name="T" />.
    ///     Log messages are forwarded to any active <see cref="LogCapture" /> scopes registered for
    ///     <typeparamref name="T" /> and to the global logger.
    /// </summary>
    /// <typeparam name="T">The class the logger is associated with.</typeparam>
    /// <returns>A class-scoped <see cref="ITestEngineLogger" /> for <typeparamref name="T" />.</returns>
    public static ITestEngineLogger GetLogger<T>() => new TypedLogger(typeof(T));

    /// <summary>
    ///     Returns a class-scoped logger for <paramref name="type" />.
    /// </summary>
    /// <param name="type">The class the logger is associated with.</param>
    /// <returns>A class-scoped <see cref="ITestEngineLogger" /> for <paramref name="type" />.</returns>
    public static ITestEngineLogger GetLogger(Type type) => new TypedLogger(type);
}
