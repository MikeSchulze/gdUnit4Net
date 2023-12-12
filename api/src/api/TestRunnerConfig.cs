using System.Collections.Generic;

namespace GdUnit4.Api;

/// <summary>
/// Represents configuration specific to a test case.
/// </summary>
public class TestCaseConfig
{
    /// <summary>
    /// Gets or sets the name of the test case.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the test case is skipped.
    /// </summary>
    public bool Skipped { get; set; } = false;
}

/// <summary>
/// Represents the configuration for the test runner, holding information about which test suites should be run.
/// For each test suite, a set of 'TestCaseConfig' instances is attached to specify included tests.
/// </summary>
public class TestRunnerConfig
{
    /// <summary>
    /// Gets or sets the included test suites along with their associated test case configurations.
    /// </summary>
    public Dictionary<string, IEnumerable<TestCaseConfig>> Included { get; set; } = new();


    /// <summary>
    /// Gets or sets the version of the test runner configuration.
    /// </summary>
    public string Version { get; set; } = "1.0";
}
