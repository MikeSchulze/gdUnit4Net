namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.Diagnostics;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

public enum Ide
{
    JetBrainsRider,
    VisualStudio,
    VisualStudioCode,
    DotNet,
    Unknown
}

public static class IdeDetector
{
    public static Ide Detect(IFrameworkHandle frameworkHandle)
    {
        var runningFramework = frameworkHandle.GetType().ToString();
        if (runningFramework.Contains("JetBrains")) return Ide.JetBrainsRider;
        if (runningFramework.Contains("VisualStudio"))
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioVersion"))) return Ide.VisualStudio;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID"))) return Ide.VisualStudioCode;
            if (Process.GetCurrentProcess().ProcessName.Contains("testhost", StringComparison.OrdinalIgnoreCase)) return Ide.DotNet;
        }

        return Ide.Unknown;
    }
}
