namespace GdUnit4.TestAdapter.Settings;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

[XmlRoot(RunSettingsXmlNode)]
public class GdUnit4Settings : TestRunSettings
{
    public enum DisplayNameOptions
    {
        SimpleName,
        FullyQualifiedName
    }

    public const string RunSettingsXmlNode = "GdUnit4";

    private static readonly XmlSerializer Serializer = new(typeof(GdUnit4Settings));

    public GdUnit4Settings() : base(RunSettingsXmlNode)
    {
    }

    /// <summary>
    ///     Additional Godot runtime parameters. These are passed to the Godot executable when running tests.
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    ///     Controls the display name format of test cases in the test results.
    ///     Allowed values:
    ///     - SimpleName: Uses only the method name (e.g., "TestMethod")
    ///     - FullyQualifiedName: Uses the full path including class and method name (e.g., "MyNamespace.MyClass.TestMethod")
    ///     Default: SimpleName
    /// </summary>
    public DisplayNameOptions DisplayName { get; set; } = DisplayNameOptions.SimpleName;

    /// <summary>
    ///     When set to true, standard output (stdout) from test cases is captured and included in the test result. This can be
    ///     useful for debugging.
    ///     Default: false
    /// </summary>
    public bool CaptureStdOut { get; set; }

    /// <summary>
    ///     The maximum duration allowed for a compilation process in milliseconds.
    /// </summary>
    /// <remarks>
    ///     After this timeout period expires, the compilation process is forcefully terminated.
    ///     Default value is 20000 milliseconds (20 seconds)
    ///     Set to a higher value for projects that require more compilation time.
    /// </remarks>
    public int CompileProcessTimeout { get; init; } = 20000;

    public override XmlElement ToXml()
    {
        using var stringWriter = new StringWriter();
        Serializer.Serialize(stringWriter, this);

        var document = new XmlDocument();
        document.LoadXml(stringWriter.ToString());

        return document.DocumentElement!;
    }
}
