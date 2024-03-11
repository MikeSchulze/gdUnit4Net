namespace GdUnit4.Exceptions;

using System;
using System.Diagnostics;

[Serializable]
public class TestFailedException : Exception
{
    public int LineNumber
    { get; private set; }

    public TestFailedException(string message, int lineNumber = -1) : base(message)
        => LineNumber = lineNumber == -1 ? GetRootCauseLineNumber() : lineNumber;

    private static int GetRootCauseLineNumber()
    {
        // Navigate the stack frames to find the root cause
        for (var i = 0; i <= 15; i++)
        {
            var frame = new StackFrame(i, true);
            // Check if the frame a external assembly
            if (frame.GetFileName() != null && frame.GetMethod()?.Module.Assembly != typeof(TestFailedException).Assembly)
                return frame.GetFileLineNumber();
        }
        return -1;
    }
}
