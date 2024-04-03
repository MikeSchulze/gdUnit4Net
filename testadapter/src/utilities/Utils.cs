namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

internal static class Utils
{

    internal static bool CheckGdUnit4ApiVersion(IMessageLogger logger, Version minVersion)
    {
        var dependencies = Assembly
            .GetExecutingAssembly()
            .GetReferencedAssemblies()
            .Where(assemblyName => "gdUnit4Api".Equals(assemblyName.Name, StringComparison.Ordinal));
        if (!dependencies.Any())
            throw new InvalidOperationException($"No 'gdUnit4Api' is installed!");
        var version = dependencies.First().Version;
        logger.SendMessage(TestMessageLevel.Informational, $"CheckGdUnit4ApiVersion gdUnit4Api, Version={version}");
        if (version < minVersion)
        {
            logger.SendMessage(TestMessageLevel.Error, $"Wrong gdUnit4Api, Version={version} found, you need to upgrade to minimum version: '{minVersion}'");
            return false;
        }
        return true;
    }
}
