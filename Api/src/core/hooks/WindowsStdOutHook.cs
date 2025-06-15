// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Core.Hooks;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Win32.SafeHandles;

[SuppressMessage("Style", "IDE1006", Justification = "Unix system call names follow C naming conventions")]
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1300:Element should begin with upper-case letter",
    Justification = "Unix system call names must match libc function names exactly")]
[SuppressMessage(
    "StyleCop.CSharp.OrderingRules",
    "SA1201:Elements should appear in the correct order",
    Justification = "P/Invoke declarations are grouped together for clarity at the end of the class")]
internal sealed class WindowsStdOutHook : IStdOutHook
{
    private const int STD_OUTPUT_HANDLE = -11;

    private readonly IntPtr originalStdOutHandle;
    private readonly SafeFileHandle pipeReadHandle;
    private readonly SafeFileHandle pipeWriteHandle;
    private readonly StdOutConsoleHook stdOutHook = new();
    private bool disposed;
    private IntPtr readEvent;
    private Thread? readThread;

    public WindowsStdOutHook()
    {
        originalStdOutHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        if (originalStdOutHandle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get original stdout handle.");

        if (!CreatePipe(out pipeReadHandle, out pipeWriteHandle, IntPtr.Zero, 0))
            throw new InvalidOperationException("Failed to create pipe.");

        readEvent = CreateEvent(IntPtr.Zero, true, false, string.Empty);
        if (readEvent == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create read event for asynchronous reading.");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void StartCapture()
    {
        // Redirect stdout to the pipe
        if (!SetStdHandle(STD_OUTPUT_HANDLE, pipeWriteHandle.DangerousGetHandle()))
            throw new InvalidOperationException("Failed to redirect stdout to pipe.");
        stdOutHook.StartCapture();
        readThread = new Thread(ReadPipeOutput);
        readThread.Start();
    }

    public void StopCapture()
    {
        stdOutHook.StopCapture();
        readThread?.Interrupt();
        readThread = null;

        // Restore original stdout
        _ = SetStdHandle(STD_OUTPUT_HANDLE, originalStdOutHandle);
    }

    public string GetCapturedOutput() => stdOutHook.GetCapturedOutput();

    // Add finalizer
    ~WindowsStdOutHook()
        => Dispose(false);

    private void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Dispose of managed resources
                stdOutHook.Dispose();
                pipeReadHandle.Dispose();
                pipeWriteHandle.Dispose();
            }

            // Dispose unmanaged resources
            if (readEvent != IntPtr.Zero)
            {
                _ = CloseHandle(readEvent);
                readEvent = IntPtr.Zero;
            }

            disposed = true;
        }
    }

    private void ReadPipeOutput()
    {
        var buffer = new byte[4096];
        var overlapped = new Overlapped { HEvent = readEvent };

        try
        {
            while (true)
            {
                if (ReadFile(pipeReadHandle, buffer, (uint)buffer.Length, out var bytesRead, ref overlapped))
                    ProcessReadData(buffer, bytesRead);
                else
                {
                    var error = Marshal.GetLastWin32Error();

                    // ERROR_IO_PENDING
                    if (error == 997)
                    {
                        // WAIT_OBJECT_0
                        if (WaitForSingleObject(readEvent, 100) == 0)
                        {
                            if (GetOverlappedResult(pipeReadHandle, ref overlapped, out bytesRead, false))
                                ProcessReadData(buffer, bytesRead);
                        }
                    }
                    else
                    {
                        // Handle other errors
                        break;
                    }
                }
            }
        }
        catch (ThreadInterruptedException)
        {
            // Normal exit via StopCapture
        }
    }

    private void ProcessReadData(byte[] buffer, uint bytesRead)
    {
        if (bytesRead > 0)
            Console.Write(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Overlapped
    {
        public IntPtr Internal;
        public IntPtr InternalHigh;
        public int Offset;
        public int OffsetHigh;
        public IntPtr HEvent;
    }

#pragma warning disable SYSLIB1054
    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern bool SetStdHandle(int nStdHandle, IntPtr hHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, uint nSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern bool ReadFile(SafeFileHandle hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, ref Overlapped lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern bool GetOverlappedResult(SafeFileHandle hFile, ref Overlapped lpOverlapped, out uint lpNumberOfBytesTransferred, bool bWait);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.UserDirectories)]
    private static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
#pragma warning restore SYSLIB1054
}
