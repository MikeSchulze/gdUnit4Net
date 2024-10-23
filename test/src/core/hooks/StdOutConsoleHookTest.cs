namespace GdUnit4.Tests.Core.Hooks;

using System;

using GdUnit4.core.hooks;

using static Assertions;

[TestSuite]
public class StdOutConsoleHookTest
{
    [TestCase]
    public void CaptureStdOut()
    {
        using var hook = new StdOutConsoleHook();
        Console.WriteLine("Before capture.");

        hook.StartCapture();

        Console.WriteLine("Hello World A!");
        Console.WriteLine("Hello World B!");

        hook.StopCapture();

        Console.WriteLine("After capture.");

        var capturedMessages = hook.GetCapturedOutput();
        AssertThat(capturedMessages)
            .IsEqual("""
                     Hello World A!
                     Hello World B!

                     """);
    }
}
