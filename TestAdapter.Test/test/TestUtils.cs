namespace GdUnit4.TestAdapter.Test;

using System;
using System.IO;

public static class TestUtils
{
    public static string GetResourcePath(string resourcePath)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        return Path.Combine(projectRoot, "test", "resources", resourcePath);
    }
}
