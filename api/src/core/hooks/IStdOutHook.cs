namespace GdUnit4.core.hooks;

using System;

internal interface IStdOutHook : IDisposable
{
    public void StartCapture();
    public void StopCapture();
    public string GetCapturedOutput();
}
