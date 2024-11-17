namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

internal static class Utils
{
    internal static string GetUserDataDirectory => Environment.OSVersion.Platform switch
    {
        PlatformID.Win32NT => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Godot"),
        PlatformID.Unix => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "godot"),
        PlatformID.MacOSX => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Application Support/Godot"),
        _ => throw new PlatformNotSupportedException("Unsupported operating system")
    };

    internal static string GetProjectDirectory
    {
        get
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null &&
                   !directory.EnumerateFiles("*.sln").Any() &&
                   !directory.EnumerateFiles("*.csproj").Any())
                directory = directory.Parent;

            return directory?.FullName ?? throw new FileNotFoundException($"Could not find project root directory {directory}");
        }
    }

    internal static bool CheckGdUnit4ApiMinimumRequiredVersion(IMessageLogger logger, Version minVersion)
    {
        var gdUnit4ApiAssembly = Assembly.Load("gdUnit4Api") ?? throw new InvalidOperationException("No 'gdUnit4Api' is installed!");
        var version = gdUnit4ApiAssembly.GetName().Version;
        logger.SendMessage(TestMessageLevel.Informational, $"CheckGdUnit4ApiVersion gdUnit4Api, Version={version}");
        if (version < minVersion)
        {
            logger.SendMessage(TestMessageLevel.Error, $"Wrong gdUnit4Api, Version={version} found, you need to upgrade to minimum version: '{minVersion}'");
            return false;
        }

        return true;
    }
}
