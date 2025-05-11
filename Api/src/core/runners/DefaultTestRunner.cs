// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Runners;

using System.Collections.Generic;
using System.Threading;

using Api;

using Execution;

/// <summary>
///     Default test runner implementation that executes tests directly in the current process.
/// </summary>
public sealed class DefaultTestRunner : BaseTestRunner
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTestRunner"/> class.
    ///     Initializes a new instance of the DefaultTestRunner.
    /// </summary>
    /// <param name="logger">The test engine logger for diagnostic output.</param>
    /// <param name="settings">Test engine configuration settings.</param>
    internal DefaultTestRunner(ITestEngineLogger logger, TestEngineSettings settings)
        : base(new DirectCommandExecutor(), logger, settings)
    {
    }

    public new void RunAndWait(List<TestSuiteNode> testSuiteNodes, ITestEventListener eventListener, CancellationToken cancellationToken)
    {
        Logger.LogInfo("======== Running GdUnit4 Default Test Runner ========");
        base.RunAndWait(testSuiteNodes, eventListener, cancellationToken);
    }
}
