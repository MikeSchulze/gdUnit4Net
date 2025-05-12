// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Hooks;

using System;

internal interface IStdOutHook : IDisposable
{
    void StartCapture();

    void StopCapture();

    string GetCapturedOutput();
}
