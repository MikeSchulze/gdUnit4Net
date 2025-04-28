namespace GdUnit4.Core.Execution.Exceptions;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Godot;

[Serializable]
public partial class TestFailedException : Exception
{
    private static readonly Regex PushErrorFileInfo = PushErrorFileInfoRegex();

    public TestFailedException(string message, int lineNumber = -1) : base(message)
    {
        LineNumber = lineNumber == -1 ? GetRootCauseLineNumber() : lineNumber;
        var frame = new StackFrame(1, true);
        var st = new StackTrace(frame);
        OriginalStackTrace = st.ToString();
    }

    private TestFailedException(string message, string details) : base(message)
    {
        var stackFrames = new StringBuilder();
        foreach (var stackTraceLine in details.Split("\n"))
        {
            var match = PushErrorFileInfo.Match(stackTraceLine);
            if (match.Success)
            {
                var methodInfo = match.Groups[1].Value;
                FileName = NormalizedPath(match.Groups[2].Value);
                LineNumber = int.Parse(match.Groups[3].Value);
                stackFrames.Append($"  at: {methodInfo} in {FileName}:line {LineNumber}");
                stackFrames.AppendLine();
            }
        }

        OriginalStackTrace = stackFrames.ToString();
    }


    public TestFailedException(string message) : base(message)
    {
        var stackFrames = new StringBuilder();
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
                    stackFrames.Append(new StackTrace(frame));
                }

                // end collect frames at test case attribute
                if (mi.IsDefined(typeof(TestCaseAttribute)))
                    break;
            }
        }

        OriginalStackTrace = stackFrames.ToString();
    }

    public string? OriginalStackTrace { get; set; }

    public override string? StackTrace => OriginalStackTrace ?? base.StackTrace;

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

    public static TestFailedException FromPushError(string message, string details) => new(message, details);

    private static string NormalizedPath(string path) =>
        path.StartsWith("res://") || path.StartsWith("user://") ? ProjectSettings.GlobalizePath(path) : path;

    private void ApplyStackTrace(string stackTrace) => typeof(Exception)
        .GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic)?
        .SetValue(this, stackTrace);

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


    [GeneratedRegex(@"at: (.*) \((.*\.cs):(\d+)\)$", RegexOptions.Compiled)]
    private static partial Regex PushErrorFileInfoRegex();
}
