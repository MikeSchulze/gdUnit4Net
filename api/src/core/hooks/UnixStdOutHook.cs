namespace GdUnit4.core.hooks;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public sealed class UnixStdOutHook : IStdOutHook
{
    private const int PIPE_READ = 0;
    private const int PIPE_WRITE = 1;
    private const int BUFFER_SIZE = 4096;
    private const int STDOUT_FILENO = 1;
    private readonly int[] aStdoutPipe = new int[2];
    private readonly StringBuilder capturedOutput = new();
    private bool isCapturing;
    private IntPtr originalStdout;
    private Thread? readThread;


    public void StartCapture()
    {
        capturedOutput.Clear();
        if (pipe(aStdoutPipe) < 0)
            throw new InvalidOperationException("Failed to create pipe.");
        originalStdout = dup(STDOUT_FILENO);
        if (originalStdout == IntPtr.Zero)
            throw new InvalidOperationException("Failed to duplicate stdout.");
        if (dup2(aStdoutPipe[PIPE_WRITE], STDOUT_FILENO) == -1)
            throw new InvalidOperationException("Failed to redirect stdout to pipe.");
        isCapturing = true;
        readThread = new Thread(ReadPipeOutput);
        readThread.Start();
    }

    public void StopCapture()
    {
        isCapturing = false;
        readThread?.Interrupt();
        readThread = null;

        // Restore original stdout
        if (dup2(originalStdout.ToInt32(), STDOUT_FILENO) == -1)
            throw new InvalidOperationException("Failed to restore stdout.");

#pragma warning disable CA1806 // Do ignore method results
        close(aStdoutPipe[PIPE_READ]);
        close(aStdoutPipe[PIPE_WRITE]);
#pragma warning restore CA1806
    }

    public string GetCapturedOutput() => capturedOutput.ToString();

    public void Dispose() => StopCapture();

    private void ReadPipeOutput()
    {
        var buffer = malloc(BUFFER_SIZE);
        while (isCapturing)
        {
            var bytesRead = read(aStdoutPipe[PIPE_READ], buffer, BUFFER_SIZE);
            if (bytesRead > 0)
            {
                var managedBuffer = new byte[bytesRead];
                Marshal.Copy(buffer, managedBuffer, 0, bytesRead);
                var capturedText = Encoding.UTF8.GetString(managedBuffer);
                capturedOutput.Append(capturedText);
#pragma warning disable CA1806 // Do ignore method results
                write(originalStdout.ToInt32(), managedBuffer, bytesRead);
#pragma warning restore CA1806
            }
        }

        FreeReadBuffer(buffer);
    }

    private void FreeReadBuffer(IntPtr buffer)
    {
        if (buffer != IntPtr.Zero)
        {
#pragma warning disable CA1806 // Do ignore method results
            free(buffer);
#pragma warning restore CA1806
        }
    }
#pragma warning disable SYSLIB1054
    [DllImport("libc", SetLastError = true)]
    private static extern IntPtr dup(int fd);

    [DllImport("libc", SetLastError = true)]
    private static extern int dup2(int oldfd, int newfd);

    [DllImport("libc", SetLastError = true)]
    private static extern int pipe(int[] pipefd);

    [DllImport("libc", SetLastError = true)]
    private static extern IntPtr malloc(int size);

    [DllImport("libc", SetLastError = true)]
    private static extern int free(IntPtr ptr);

    [DllImport("libc", SetLastError = true)]
    private static extern int read(int fd, IntPtr buf, int count);

    [DllImport("libc", SetLastError = true)]
    private static extern int write(int fd, byte[] buf, int count);

    [DllImport("libc", SetLastError = true)]
    private static extern int close(int fd);
#pragma warning restore SYSLIB1054
}
