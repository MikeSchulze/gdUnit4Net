namespace GdUnit4.core.hooks;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal sealed class UnixStdOutHook : IStdOutHook
{
    private const int STD_OUTPUT_HANDLE = 1; // STDOUT_FILENO in Unix
    private const int PIPE_READ = 0;
    private const int PIPE_WRITE = 1;

    private const int F_GETFL = 3;
    private const int F_SETFL = 4;
    private const int O_NONBLOCK = 0x0004;
    private readonly int originalFlags;

    private readonly IntPtr originalStdOutHandle;
    private readonly int[] pipeHandles = new int[2];
    private readonly StdOutConsoleHook stdOutHook = new();
    private bool isCapturing;
    private Thread? readThread;

    public UnixStdOutHook()
    {
        // Get original stdout handle
        originalStdOutHandle = dup(STD_OUTPUT_HANDLE);
        if (originalStdOutHandle == IntPtr.Zero)
            throw new InvalidOperationException("Failed to get original stdout handle.");

        // Create pipe
        if (pipe(pipeHandles) != 0)
            throw new InvalidOperationException("Failed to create pipe.");

        // Store original flags and set non-blocking mode
        originalFlags = fcntl(pipeHandles[PIPE_READ], F_GETFL, 0);
        var hResult = fcntl(pipeHandles[PIPE_READ], F_SETFL, originalFlags | O_NONBLOCK);
        if (hResult != 0)
            throw new InvalidOperationException($"Failed to create fcntl. Error: {hResult}");
    }

    public void Dispose()
    {
        StopCapture();
#pragma warning disable CA1806 // Do ignore method results
        // Reset pipe read end blocking mode to its original state
        fcntl(pipeHandles[PIPE_READ], F_SETFL, originalFlags);

        close(pipeHandles[PIPE_READ]);
        close(pipeHandles[PIPE_WRITE]);
#pragma warning restore CA1806
        stdOutHook.Dispose();
    }

    public void StartCapture()
    {
        // Redirect stdout to the pipe
        if (dup2(pipeHandles[PIPE_WRITE], STD_OUTPUT_HANDLE) == -1)
            throw new InvalidOperationException("Failed to redirect stdout to pipe.");

        stdOutHook.StartCapture();
        isCapturing = true;
        readThread = new Thread(ReadPipeOutput);
        readThread.Start();
    }

    public void StopCapture()
    {
        stdOutHook.StopCapture();
        isCapturing = false;
        readThread?.Join(100);
        readThread = null;

        // Restore original stdout
#pragma warning disable CA1806 // Do ignore method results
        dup2(originalStdOutHandle.ToInt32(), STD_OUTPUT_HANDLE);
#pragma warning restore CA1806
    }

    public string GetCapturedOutput() => stdOutHook.GetCapturedOutput();

    private void ReadPipeOutput()
    {
        try
        {
            var buffer = new byte[4096];
            var pollFd = new PollFd
            {
                fd = pipeHandles[PIPE_READ],
                events = 0x0001 // POLLIN
            };

            while (isCapturing)
            {
                var ready = poll(ref pollFd, 1, 10); // 10ms timeout
                if (ready > 0)
                {
                    var bytesRead = read(pipeHandles[PIPE_READ], buffer, buffer.Length);
                    if (bytesRead > 0) ProcessReadData(buffer, (uint)bytesRead);
                }
                else if (ready < 0)
                    // Error occurred
                    break;
            }
        }
        catch (ThreadInterruptedException)
        {
            // Normal exit via StopCapture
        }
    }

    private void ProcessReadData(byte[] buffer, uint bytesRead)
    {
        if (bytesRead > 0) Console.Write(Encoding.UTF8.GetString(buffer, 0, (int)bytesRead));
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd
    {
        public int fd;
        public short events;
        public short revents;
    }

#pragma warning disable SYSLIB1054
    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int[] pipefd);

    [DllImport("libc", SetLastError = true)]
    private static extern int dup(int oldfd);

    [DllImport("libc", SetLastError = true)]
    private static extern int dup2(int oldfd, int newfd);

    [DllImport("libc", SetLastError = true)]
    private static extern int read(int fd, byte[] buf, int count);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int fcntl(int fd, int cmd, int arg);

    [DllImport("libc", SetLastError = true)]
    private static extern int poll(ref PollFd fds, uint nfds, int timeout);
#pragma warning restore SYSLIB1054
}
