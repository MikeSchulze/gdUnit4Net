namespace GdUnit4.core.hooks;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

public class UnixStdOutHook : IStdOutHook
{
    private const int STDOUT_FILENO = 1;
    private readonly StringBuilder capturedOutput = new();

    private readonly int[] pipefd = new int[2];
    private bool isCapturing;
    private IntPtr originalStdout;

    public void StartCapture()
    {
        pipe(pipefd);
        originalStdout = dup(STDOUT_FILENO);
        dup2(pipefd[1], STDOUT_FILENO);
        isCapturing = true;

        var captureThread = new Thread(CaptureThread);
        captureThread.Start();
    }

    public void StopCapture()
    {
        isCapturing = false;
        dup2(originalStdout.ToInt32(), STDOUT_FILENO);
    }

    public string GetCapturedOutput() => capturedOutput.ToString();

    public void Dispose()
    {
        // TODO release managed resources here
    }

    public void ClearCapturedOutput() => capturedOutput.Clear();

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

    private void CaptureThread()
    {
        var buffer = malloc(4096);
        while (isCapturing)
        {
            var bytesRead = read(pipefd[0], buffer, 4096);
            if (bytesRead > 0)
            {
                var managedBuffer = new byte[bytesRead];
                Marshal.Copy(buffer, managedBuffer, 0, bytesRead);
                var capturedText = Encoding.UTF8.GetString(managedBuffer);
                capturedOutput.Append(capturedText);

                // Write to original stdout
                write(originalStdout.ToInt32(), managedBuffer, bytesRead);
            }
        }

        free(buffer);
    }
}
