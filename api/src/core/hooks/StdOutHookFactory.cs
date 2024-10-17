namespace GdUnit4.core.hooks;

using System;

internal static class StdOutHookFactory
{
    public static IStdOutHook CreateStdOutHook()
    {
        if (OperatingSystem.IsWindows())
            return new WindowsStdOutHook();
        if (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
            return new UnixStdOutHook();
        throw new PlatformNotSupportedException("Unsupported operating system");
    }
}
