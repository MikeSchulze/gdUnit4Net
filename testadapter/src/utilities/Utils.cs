namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

internal static class Utils
{

    internal static bool CheckGdUnit4ApiVersion(IMessageLogger logger, Version minVersion)
    {
        var gdUnit4ApiAssembly = Assembly.Load("gdUnit4Api") ?? throw new InvalidOperationException($"No 'gdUnit4Api' is installed!");
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
