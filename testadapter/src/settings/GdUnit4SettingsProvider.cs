namespace GdUnit4.TestAdapter.Settings;

using System;
using System.Xml;
using System.Xml.Serialization;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

[SettingsName(GdUnit4Settings.RunSettingsXmlNode)]
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class GdUnit4SettingsProvider : ISettingsProvider
{
    private XmlSerializer Serializer { get; } = new(typeof(GdUnit4Settings));

    public GdUnit4Settings Settings { get; private set; } = new();

    public void Load(XmlReader reader)
    {
        try
        {
            if (reader.Read() && reader.Name == GdUnit4Settings.RunSettingsXmlNode)
            {
                var settings = Serializer.Deserialize(reader) as GdUnit4Settings;
                Settings = settings ?? new GdUnit4Settings();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Loading GdUnit4 Adapter settings failed! {e}");
        }
    }

    public static GdUnit4Settings LoadSettings(IDiscoveryContext discoveryContext)
    {
        var gdUnitSettingsProvider = discoveryContext.RunSettings?.GetSettings(GdUnit4Settings.RunSettingsXmlNode) as GdUnit4SettingsProvider;
        return gdUnitSettingsProvider?.Settings ?? new GdUnit4Settings();
    }
}
