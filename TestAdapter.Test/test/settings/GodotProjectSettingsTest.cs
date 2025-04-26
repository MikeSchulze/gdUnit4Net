namespace GdUnit4.TestAdapter.Test.Settings;

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestAdapter.Settings;

using static TestUtils;

using static TestAdapter.Utilities.Utils;

[TestClass]
public class GodotProjectSettingsTest
{
    [TestMethod]
    public void GlobalizeGodotPath()
    {
        // user specific path
        Assert.AreEqual(
            Path.Combine(GetUserDataDirectory, "app_userdata", "gdUnit4Test", "logs", "godot.log"),
            GodotProjectSettings.GlobalizeGodotPath("user://logs/godot.log", "gdUnit4Test"));
        // project specific path
        Assert.AreEqual(
            Path.Combine(GetProjectDirectory, "logs", "godot.log"),
            GodotProjectSettings.GlobalizeGodotPath("res://logs/godot.log", "gdUnit4Test"));
    }

    [TestMethod]
    public void LoadGodotProjectSettingsWithoutDebugSection()
    {
        var projectFile = GetResourcePath("project.godot");
        var settings = GodotProjectSettings.LoadFromFile(projectFile);
        Assert.IsNotNull(settings);

        Assert.AreEqual("gdUnit4Test", settings.Application.Config.Name);
        CollectionAssert.Contains(settings.Application.Config.Features, "4.3");
        CollectionAssert.Contains(settings.Application.Config.Features, "C#");
        Assert.AreEqual("res://icon.svg", settings.Application.Config.Icon);

        // validate the log file path points to the default user dir
        Assert.IsNotNull(settings.Debug);
        var expectedLogFilePath = GodotProjectSettings.GlobalizeGodotPath("user://logs/godot.log", "gdUnit4Test");
        Assert.AreEqual(expectedLogFilePath, settings.Debug.FileLogging.LogPath);
    }

    [TestMethod]
    public void LoadGodotProjectSettingsWithCustomLogPath()
    {
        var projectFile = GetResourcePath("project.godot2");
        var settings = GodotProjectSettings.LoadFromFile(projectFile);
        Assert.IsNotNull(settings);

        Assert.AreEqual("gdUnit4", settings.Application.Config.Name);
        CollectionAssert.Contains(settings.Application.Config.Features, "4.3");
        CollectionAssert.Contains(settings.Application.Config.Features, "C#");
        Assert.AreEqual("res://icon.png", settings.Application.Config.Icon);

        // validate the log file path points to project root
        Assert.IsNotNull(settings.Debug);
        var expectedLogFilePath = Path.Combine(GetProjectDirectory, "godot_session.log");
        Assert.AreEqual(expectedLogFilePath, settings.Debug.FileLogging.LogPath);
    }
}
