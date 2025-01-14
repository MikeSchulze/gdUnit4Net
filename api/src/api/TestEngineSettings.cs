﻿namespace GdUnit4.Api;

/// <summary>
///     Configuration settings for the GdUnit4 test engine.
///     Contains runtime parameters and execution options that control test behavior.
/// </summary>
/// <remarks>
///     Example usage:
///     <code>
/// var settings = new TestEngineSettings
/// {
///     Parameters = "--verbose",
///     CaptureStdOut = true,
///     MaxCpuCount = 4,
///     SessionTimeout = 60000  // 1 minute timeout
/// };
/// </code>
/// </remarks>
public sealed class TestEngineSettings
{
    /// <summary>
    ///     Additional Godot runtime parameters. These are passed to the Godot executable when running tests.
    /// </summary>
    /// <remarks>
    ///     These parameters are appended to the Godot command line when launching the test process.
    ///     Can include any valid Godot command line arguments.
    /// </remarks>
    public string? Parameters { get; init; }

    /// <summary>
    ///     Controls whether standard output (stdout) from test cases is captured.
    /// </summary>
    /// <remarks>
    ///     When set to true, stdout is captured and included in the test result.
    ///     This can be useful for debugging test failures or analyzing test behavior.
    ///     Default value is false to optimize performance.
    /// </remarks>
    public bool CaptureStdOut { get; init; }

    /// <summary>
    ///     Specifies the maximum number of CPU cores to use for parallel test execution.
    /// </summary>
    /// <remarks>
    ///     Controls the degree of test parallelization. A value of 0 or 1 runs tests sequentially.
    ///     Higher values enable parallel test execution up to the specified core count.
    /// </remarks>
    public int MaxCpuCount { get; init; }

    /// <summary>
    ///     The maximum duration allowed for a complete test session in milliseconds.
    /// </summary>
    /// <remarks>
    ///     After this timeout period expires, the test session is forcefully terminated.
    ///     Default value is 30000 milliseconds (30 seconds).
    ///     Set to a higher value for test suites that require more time to complete.
    /// </remarks>
    public int SessionTimeout { get; init; } = 30000;
}