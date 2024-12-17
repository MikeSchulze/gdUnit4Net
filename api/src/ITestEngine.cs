namespace GdUnit4;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Core;
using Core.Discovery;

/// <summary>
///     Interface for the GdUnit4 test engine responsible for discovering and executing tests.
///     Provides core functionality for test discovery, execution, and version information.
/// </summary>
public interface ITestEngine
{
    /// <summary>
    ///     Creates a new instance of the test engine with specified settings and logger.
    /// </summary>
    /// <param name="settings">Configuration settings for the test engine</param>
    /// <param name="logger">Logger for test engine operations</param>
    /// <returns>A new ITestEngine instance</returns>
    static ITestEngine GetInstance(TestEngineSettings settings, ITestEngineLogger logger) => new GdUnit4TestEngine(settings, logger);

    /// <summary>
    ///     Discovers test cases in the specified test assembly.
    /// </summary>
    /// <param name="testAssembly">Path to the test assembly to scan</param>
    /// <returns>List of discovered test cases</returns>
    public List<TestCaseDescriptor> Discover(string testAssembly);

    /// <summary>
    ///     Executes the specified test cases asynchronously.
    /// </summary>
    /// <param name="testCases">List of test cases to execute</param>
    /// <returns>Task representing the execution result count</returns>
    public Task<int> Execute(List<ITestCase> testCases);

    /// <summary>
    ///     Gets the version of the GdUnit4 test engine.
    /// </summary>
    /// <returns>Version of the engine, or null if the gdUnit4Api is not installed</returns>
    /// <exception cref="InvalidOperationException">Thrown when gdUnit4Api is not installed</exception>
    static Version? EngineVersion()
    {
        var assembly = typeof(GdUnit4TestEngine).Assembly ?? throw new InvalidOperationException("No 'gdUnit4Api' is installed!");
        return assembly.GetName().Version;
    }
}
