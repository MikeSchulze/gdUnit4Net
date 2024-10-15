namespace GdUnit4.TestAdapter.Utilities;

using System;
using System.Diagnostics;
using System.Linq;

public enum Ide
{
    Rider,
    VisualStudio,
    VisualStudioCode,
    Unknown
}

public static class IdeDetector
{
    public static Ide Detect()
    {
        // Check for Rider
        if (Environment.GetEnvironmentVariable("RIDER_HOSTED") == "1") return Ide.Rider;

        // Check for Visual Studio
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VisualStudioVersion"))) return Ide.VisualStudio;

        // Check for VS Code
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSCODE_PID"))) return Ide.VisualStudioCode;

        // If no specific IDE is detected, check running processes
        var processes = Process.GetProcesses();

        if (processes.Any(p => p.ProcessName.Contains("rider", StringComparison.OrdinalIgnoreCase))) return Ide.Rider;

        if (processes.Any(p => p.ProcessName.Contains("devenv", StringComparison.OrdinalIgnoreCase))) return Ide.VisualStudio;

        if (processes.Any(p => p.ProcessName.Contains("code", StringComparison.OrdinalIgnoreCase))) return Ide.VisualStudioCode;

        return Ide.Unknown;
    }
}
