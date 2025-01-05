namespace GdUnit4.Api;

using System.Diagnostics;

public interface IDebuggerFramework
{
    bool IsDebugProcess { get; }
    bool IsDebugAttach { get; }

    Process LaunchProcessWithDebuggerAttached(ProcessStartInfo processStartInfo);
    bool AttachDebuggerToProcess(Process process);
}
