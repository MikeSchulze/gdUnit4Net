﻿namespace GdUnit4.Core.Runners;

using Execution;

/// <summary>
///     Default test runner implementation that executes tests directly in the current process.
/// </summary>
public sealed class DefaultTestRunner : BaseTestRunner
{
    /// <summary>
    ///     Initializes a new instance of the DefaultTestRunner.
    /// </summary>
    /// <param name="logger">The test engine logger for diagnostic output.</param>
    /// <param name="settings">Test engine configuration settings.</param>
    internal DefaultTestRunner(ITestEngineLogger logger, TestEngineSettings settings)
        : base(new DirectCommandExecutor(), logger, settings)
    {
    }
}
