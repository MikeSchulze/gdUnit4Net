using System;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GdUnit4.TestAdapter.Settings;


[SettingsName(GdUnit4Settings.RunSettingsXmlNode)]
public class GdUnit4SettingsProvider : ISettingsProvider
{
    private XmlSerializer Serializer { get; set; } = new XmlSerializer(typeof(GdUnit4Settings));

    public GdUnit4Settings Settings { get; private set; } = new GdUnit4Settings();


    public void Load(XmlReader reader)
    {
        try
        {
            if (reader?.Read() == true && reader.Name == GdUnit4Settings.RunSettingsXmlNode)
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
}
