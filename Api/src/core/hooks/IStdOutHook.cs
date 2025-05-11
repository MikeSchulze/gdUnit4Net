// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Hooks;

using System;

internal interface IStdOutHook : IDisposable
{
    public void StartCapture();

    public void StopCapture();

    public string GetCapturedOutput();
}
