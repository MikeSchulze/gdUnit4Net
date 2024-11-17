namespace GdUnit4.TestAdapter.Test.Utilities;

using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using static TestAdapter.Utilities.Utils;

[TestClass]
public class UtilsTest
{
    [TestMethod]
    public void ProjectDirectory()
        => Assert.IsTrue(GetProjectDirectory.EndsWith("testadapter.test"));

    [TestMethod]
    public void UserDataDirectory()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                Assert.IsTrue(GetUserDataDirectory.EndsWith("Godot")); break;
            case PlatformID.Unix:
                Assert.IsTrue(GetUserDataDirectory.EndsWith("godot")); break;
            case PlatformID.MacOSX:
                Assert.IsTrue(GetUserDataDirectory.EndsWith("Library/Application Support/Godot"));
                break;
        }
    }
}
