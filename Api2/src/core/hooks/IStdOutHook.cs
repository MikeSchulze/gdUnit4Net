namespace GdUnit4.Core.Hooks;

using System;

internal interface IStdOutHook : IDisposable
{
    public void StartCapture();
    public void StopCapture();
    public string GetCapturedOutput();
}
