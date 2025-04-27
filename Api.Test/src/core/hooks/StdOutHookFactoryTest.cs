namespace GdUnit4.Tests.Core.Hooks;

using System;
using System.Threading.Tasks;

using GdUnit4.Core.Execution;
using GdUnit4.Core.Hooks;

using Godot;

using static Assertions;

using Environment = System.Environment;

[RequireGodotRuntime]
[TestSuite]
public class StdOutHookFactoryTest
{
    private bool savedStdOutCaptureState;

    [Before]
    public void Before()
    {
        // We disable the possible enabled stdout capture otherwise we run into unexpected behaviors
        savedStdOutCaptureState = ExecutionContext.Current!.IsCaptureStdOut;
        ExecutionContext.Current!.IsCaptureStdOut = false;
    }

    [After]
    public void After() => ExecutionContext.Current!.IsCaptureStdOut = savedStdOutCaptureState;

    [TestCase]
    public void CreateStdHook()
    {
        var stdOutHook = StdOutHookFactory.CreateStdOutHook();
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            AssertObject(stdOutHook).IsInstanceOf<UnixStdOutHook>();
        if (OperatingSystem.IsWindows())
            AssertObject(stdOutHook).IsInstanceOf<WindowsStdOutHook>();
    }

    [TestCase]
    public void CaptureStdOutConsole()
    {
        var stdOutHook = StdOutHookFactory.CreateStdOutHook();
        // it should not be captured before `StartCapture`
        Console.WriteLine("Console: Short before 'StartCapture'");

        // start the capturing
        stdOutHook.StartCapture();
        Console.WriteLine("Console: Short after 'StartCapture'");
        Console.WriteLine("Console: A message");
        Console.WriteLine("Console: Short before 'StopCapture'");
        // stop it
        stdOutHook.StopCapture();
        // it should not be captured after `StopCapture`
        Console.WriteLine("Console: Short after 'StopCapture'");

        // verify
        AssertThat(stdOutHook.GetCapturedOutput())
            .IsEqual($"Console: Short after 'StartCapture'{Environment.NewLine}" +
                     $"Console: A message{Environment.NewLine}" +
                     $"Console: Short before 'StopCapture'{Environment.NewLine}");
    }

    [TestCase]
    public async Task CaptureStdOutGodot()
    {
        var stdOutHook = StdOutHookFactory.CreateStdOutHook();
        // it should not be captured before `StartCapture`
        GD.PrintS("Godot: Short before 'StartCapture'");

        // start the capturing
        stdOutHook.StartCapture();
        GD.PrintS("Godot: Short after 'StartCapture'");
        GD.PrintS("Godot: A message");
        GD.PrintS("Godot: Short before 'StopCapture'");

        // need to await sync stdout from Godot engine is written
        await ISceneRunner.SyncProcessFrame;
        // stop it
        stdOutHook.StopCapture();
        // it should not be captured after `StopCapture`
        GD.PrintS("Godot: Short after 'StopCapture'");

        // verify
        AssertThat(stdOutHook.GetCapturedOutput())
            .IsEqual($"Godot: Short after 'StartCapture'{Environment.NewLine}" +
                     $"Godot: A message{Environment.NewLine}" +
                     $"Godot: Short before 'StopCapture'{Environment.NewLine}");
    }

    [TestCase]
    public async Task CaptureStdOutConsoleAndGodot()
    {
        var stdOutHook = StdOutHookFactory.CreateStdOutHook();

        // it should not be captured before `StartCapture`
        Console.WriteLine("Console: Do not be captured");
        GD.PrintS("Godot: Do not be captured");

        // start the capturing
        stdOutHook.StartCapture();
        Console.WriteLine("Console: Short after 'StartCapture'");
        GD.PrintS("Godot: Short after 'StartCapture'");
        Console.WriteLine("Console: A message");
        GD.PrintS("Godot: A message");
        Console.WriteLine("Console: Short before 'StopCapture'");
        GD.PrintS("Godot: Short before 'StopCapture'");
        // need to await sync stdout from Godot engine is written
        await ISceneRunner.SyncProcessFrame;
        // stop it
        stdOutHook.StopCapture();

        // it should not be captured after `StopCapture`
        Console.WriteLine("Console: Do not be captured");
        GD.PrintS("Godot: Do not be captured");


        // verify the captured output contains all lines, but we cant guaranty the same order
        var capturedOutput = stdOutHook.GetCapturedOutput();
        AssertThat(capturedOutput)
            .Contains("Console: Short after 'StartCapture'")
            .Contains("Godot: Short after 'StartCapture'")
            .Contains("Console: A message")
            .Contains("Godot: A message")
            .Contains("Console: Short before 'StopCapture'")
            .Contains("Godot: Short before 'StopCapture'")
            // and verify before and after capture messages are not caught
            .NotContains("Console: Do not be captured")
            .NotContains("Godot: Do not be captured");
    }
}
