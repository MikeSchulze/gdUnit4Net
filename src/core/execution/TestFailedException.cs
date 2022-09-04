using System.Diagnostics;

namespace GdUnit3.Exceptions
{
    internal sealed class TestFailedException : System.Exception
    {
        public TestFailedException(string message, int frameOffset = 0, int lineNumber = -1) : base(message)
        {
            LineNumber = lineNumber == -1 ? ScanFailureLineNumber(frameOffset, 15) : lineNumber;
        }

        private static int ScanFailureLineNumber(int frameOffset, int stackOffset)
        {
            bool isFound = false;
            StackFrame frame;
            for (var i = stackOffset; i >= 0; i--)
            {
                frame = new StackFrame(i, true);
                var fileName = frame.GetFileName();
                if (fileName == null)
                    continue;
                //Godot.GD.PrintS("StackFrame", i, fileName, frame.GetFileLineNumber());
                if (fileName.Replace('\\', '/').EndsWith("core/execution/ExecutionStage.cs"))
                {
                    isFound = true;
                    continue;
                }
                if (isFound)
                    return frame.GetFileLineNumber();
            }
            frame = new StackFrame(3 + frameOffset, true);
            // fix stack offset if the assert is delegated
            if (frame.GetFileName() != null && frame.GetFileName().Replace('\\', '/').Contains("src/asserts/"))
                frame = new StackFrame(4 + frameOffset, true);
            return frame.GetFileLineNumber();
        }

        public int LineNumber
        { get; private set; }
    }
}
