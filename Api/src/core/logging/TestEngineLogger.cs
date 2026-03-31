// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Logging;

using Api;

/// <summary>
///     Manages the global logger instance used by all class-scoped loggers.
///     Intended for engine-internal use only; use <see cref="LoggerFactory" /> to obtain loggers.
/// </summary>
/// <remarks>
///     Register the real logger once at engine startup:
///     <code>
///     TestEngineLogger.Register(myLogger);
///     </code>
/// </remarks>
internal static class TestEngineLogger
{
    private static volatile ITestEngineLogger global = NoOpTestEngineLogger.Instance;

    /// <summary>Gets the currently registered global logger.</summary>
    internal static ITestEngineLogger Global => global;

    /// <summary>
    ///     Registers the global logger used as the fallback for all class-scoped loggers.
    ///     Intended to be called once at engine startup.
    /// </summary>
    /// <param name="logger">The logger implementation to register.</param>
    internal static void Register(ITestEngineLogger logger)
        => global = logger ?? throw new ArgumentNullException(nameof(logger));
}
