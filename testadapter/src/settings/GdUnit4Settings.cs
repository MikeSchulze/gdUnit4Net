
namespace GdUnit4.TestAdapter.Settings;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;



[XmlRoot(RunSettingsXmlNode)]
public class GdUnit4Settings : TestRunSettings
{

    public const string RunSettingsXmlNode = "GdUnit4";

    private static readonly XmlSerializer Serializer = new(typeof(GdUnit4Settings));

    public enum DisplayNameOptions
    {
        SimpleName,
        FullyQualifiedName
    }

    public string? Parameters { get; set; }

    public DisplayNameOptions DisplayName { get; set; } = DisplayNameOptions.SimpleName;

    public GdUnit4Settings() : base(RunSettingsXmlNode)
    {
    }

    public override XmlElement ToXml()
    {
        using var stringWriter = new StringWriter();
        Serializer.Serialize(stringWriter, this);

        var document = new XmlDocument();
        document.LoadXml(stringWriter.ToString());

        return document.DocumentElement!;
    }
}
