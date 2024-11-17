// ReSharper disable MemberCanBePrivate.Global

namespace GdUnit4.TestAdapter.Settings;

using System;
using System.IO;
using System.Linq;

using Utilities;

internal class GodotProjectSettings
{
    internal ApplicationSection Application { get; set; } = new();
    internal DebugSection Debug { get; set; } = new();
    internal EditorPluginsSection EditorPlugins { get; set; } = new();
    internal GdUnit4Section GdUnit4 { get; set; } = new();

    internal static GodotProjectSettings LoadFromFile(string filePath)
    {
        var settings = new GodotProjectSettings();
        var lines = File.ReadAllLines(filePath);

        var currentSection = "";
        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(';'))
                continue;

            if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                continue;
            }

            if (currentSection == "" || !trimmedLine.Contains('='))
                continue;

            var parts = trimmedLine.Split(new[] { '=' }, 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = ParseValue(parts[1].Trim());
                SetPropertyValue(settings, currentSection, key, value);
            }
        }

        // fix godot paths to system paths
        settings.Debug.FileLogging.LogPath =
            GlobalizeGodotPath(settings.Debug.FileLogging.LogPath, settings.Application.Config.Name);

        return settings;
    }

    private static void SetPropertyValue(GodotProjectSettings settings, string section, string key, object value)
    {
        object? sectionObj = section.ToLower() switch
        {
            "application" => settings.Application,
            "debug" => settings.Debug,
            "editor_plugins" => settings.EditorPlugins,
            "gdunit4" => settings.GdUnit4,
            _ => null
        };

        if (sectionObj == null) return;

        var propertyParts = key.Split('/');
        var currentObj = sectionObj;

        for (var i = 0; i < propertyParts.Length - 1; i++)
        {
            var propertyName = ToPascalCase(propertyParts[i]);
            var property = currentObj.GetType().GetProperty(propertyName);

            if (property == null)
                return;

            currentObj = property.GetValue(currentObj);
            if (currentObj == null)
                return;
        }

        var finalPropertyName = ToPascalCase(propertyParts[^1]);
        var finalProperty = currentObj.GetType().GetProperty(finalPropertyName);

        if (finalProperty != null)
            try
            {
                if (finalProperty.PropertyType.IsArray)
                {
                    if (value is string[] arrayValue)
                        finalProperty.SetValue(currentObj, arrayValue);
                }
                else
                {
                    var convertedValue = Convert.ChangeType(value, finalProperty.PropertyType);
                    finalProperty.SetValue(currentObj, convertedValue);
                }
            }
            catch (Exception)
            {
                // Log or handle conversion errors
            }
    }

    private static string ToPascalCase(string input)
    {
        var words = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(words.Select(word =>
            char.ToUpper(word[0]) +
            (word.Length > 1 ? word.Substring(1).ToLower() : "")
        ));
    }

    private static object ParseValue(string value)
    {
        if (value.StartsWith('"') && value.EndsWith('"'))
            return value.Substring(1, value.Length - 2);

        if (value.StartsWith("PackedStringArray(") && value.EndsWith(')'))
        {
            var arrayContent = value.Substring("PackedStringArray(".Length, value.Length - "PackedStringArray(".Length - 1);
            return arrayContent.Split(',')
                .Select(s => s.Trim().Trim('"'))
                .ToArray();
        }

        if (bool.TryParse(value.ToLower(), out var boolResult))
            return boolResult;

        if (double.TryParse(value, out var doubleResult))
            return doubleResult;

        return value;
    }


    internal static string GlobalizeGodotPath(string path, string projectName)
    {
        // Handle user specific paths
        if (path.StartsWith("user://"))
        {
            var userPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return path.Replace("user:/", Path.Combine(userPath, "Godot", "app_userdata", projectName)).Replace("/", Path.DirectorySeparatorChar.ToString());
        }

        return path.StartsWith("res://")
            ? path.Replace("res:/", Utils.GetProjectRoot()).Replace("/", Path.DirectorySeparatorChar.ToString())
            : path;
    }


    public interface ISettingsSection
    {
    }

    public class ApplicationSection : ISettingsSection
    {
        public ConfigSetting Config { get; set; } = new();

        public class ConfigSetting
        {
            public string Name { get; set; } = "";
            public string[] Tags { get; set; } = Array.Empty<string>();
            public string[] Features { get; set; } = Array.Empty<string>();
            public string Icon { get; set; } = "";
        }
    }

    public class DebugSection : ISettingsSection
    {
        public FileLoggingSettings FileLogging { get; set; } = new();

        public class FileLoggingSettings
        {
            public bool EnableFileLogging { get; set; }
            public string LogPath { get; set; } = "user://logs/godot.log";
        }
    }


    public class EditorPluginsSection : ISettingsSection
    {
        public string[] Enabled { get; set; } = Array.Empty<string>();
    }

    public class GdUnit4Section : ISettingsSection
    {
        public TestRunSettings Settings { get; set; } = new();
        public ReportSettings Report { get; set; } = new();


        public class TestRunSettings
        {
            public TestSettings Test { get; set; } = new();

            public class TestSettings
            {
                public bool FlakyCheckEnable { get; set; }
            }
        }

        public class ReportSettings
        {
            public GodotReportSettings Godot { get; set; } = new();

            public class GodotReportSettings
            {
                public bool ScriptError { get; set; }
            }
        }
    }
}
