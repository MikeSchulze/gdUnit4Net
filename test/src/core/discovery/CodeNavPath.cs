namespace GdUnit4.Tests.Core.Discovery;

using System.IO;
using System.Reflection;

internal static class CodeNavPath
{
    internal static string GetSourceFilePath(string relativeSourcePath)
    {
        // Get the directory of the executing assembly
        var assemblyLocation = Assembly.GetExecutingAssembly().Location;
        var projectDir = Path.GetDirectoryName(assemblyLocation)!;

        // Navigate up to find the test file
        // Note: Adjust the path based on your project structure
        while (Directory.GetFiles(projectDir, "*.csproj").Length == 0 && Directory.GetParent(projectDir) != null)
            projectDir = Directory.GetParent(projectDir)!.FullName;

        // Find the test file in the project directory
        var sourceFile = Path.Combine(projectDir.Replace('\\', Path.DirectorySeparatorChar), relativeSourcePath.Replace('/', Path.DirectorySeparatorChar));
        return Path.GetFullPath(sourceFile);
    }
}
