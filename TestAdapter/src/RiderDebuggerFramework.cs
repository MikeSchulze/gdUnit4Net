namespace GdUnit4.TestAdapter;

using System;
using System.Diagnostics;

using Api;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

public class RiderDebuggerFramework : IDebuggerFramework
{
    public RiderDebuggerFramework(IFrameworkHandle frameworkHandle)
        => FrameworkHandle = frameworkHandle;

    private IFrameworkHandle FrameworkHandle { get; }

    public bool IsDebugProcess { get; } = Debugger.IsAttached;
    public bool IsDebugAttach => false;

    public Process LaunchProcessWithDebuggerAttached(ProcessStartInfo processStartInfo)
    {
        var processId = FrameworkHandle
            .LaunchProcessWithDebuggerAttached(
                processStartInfo.FileName,
                processStartInfo.WorkingDirectory,
                processStartInfo.Arguments,
                processStartInfo.Environment);
        return Process.GetProcessById(processId);
    }

    public bool AttachDebuggerToProcess(Process process)
        => throw new NotImplementedException();
}
