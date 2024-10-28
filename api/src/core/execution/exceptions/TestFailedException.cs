namespace GdUnit4.Core.Execution.Exceptions;

using System;
using System.Diagnostics;
using System.Reflection;

[Serializable]
internal class TestFailedException : Exception
{
    public TestFailedException(string message, int lineNumber = -1) : base(message)
    {
        LineNumber = lineNumber == -1 ? GetRootCauseLineNumber() : lineNumber;
        var frame = new StackFrame(1, true);
        var st = new StackTrace(frame);
        StackTrace = st.ToString();
    }

    public TestFailedException(string message) : base(message)
    {
        foreach (var frame in new StackTrace(true).GetFrames())
        {
            var mb = frame.GetMethod();
            // we only collect test-suite related stack frames

            // skip GdUnit4 api frames and skip system api frames do only collect test relates frames
            if (mb is MethodInfo mi
                && mi.Module.Assembly != typeof(TestFailedException).Assembly)
            {
                if (frame.GetFileLineNumber() > 0)
                {
                    LineNumber = LineNumber == -1 ? frame.GetFileLineNumber() : LineNumber;
                    FileName ??= frame.GetFileName();
                    StackTrace += new StackTrace(frame).ToString();
                }

                // end collect frames at test case attribute
                if (mi.IsDefined(typeof(TestCaseAttribute)))
                    break;
            }
        }
    }

    public int LineNumber
    {
        get;
        private set;
    } = -1;

    public string? FileName
    {
        get;
        private set;
    }

    public new string? StackTrace { get; private set; }

    private static int GetRootCauseLineNumber()
    {
        // Navigate the stack frames to find the root cause
        for (var i = 0; i <= 15; i++)
        {
            var frame = new StackFrame(i, true);
            // Check is the frame an external assembly
            if (frame.GetFileName() != null && frame.GetMethod()?.Module.Assembly != typeof(TestFailedException).Assembly)
                return frame.GetFileLineNumber();
        }

        return -1;
    }
}
