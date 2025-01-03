namespace GdUnit4;

/// <summary>
///     Configuration settings for the GdUnit4 test engine.
///     Contains runtime parameters and execution options that control test behavior.
/// </summary>
public sealed class TestEngineSettings
{
    /// <summary>
    ///     Additional Godot runtime parameters. These are passed to the Godot executable when running tests.
    /// </summary>
    public string? Parameters { get; init; }


    /// <summary>
    ///     When set to true, standard output (stdout) from test cases is captured and included in the test result. This can be
    ///     useful for debugging.
    ///     Default: false
    /// </summary>
    public bool CaptureStdOut { get; init; }

    public int MaxCpuCount { get; set; }
}
