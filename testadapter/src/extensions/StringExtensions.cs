namespace GdUnit4.TestAdapter.Extensions;

using System;
using System.Linq;
using System.Text;

internal static class StringExtensions
{
    public static string Indent(this string str, int count = 1, string indentWith = "\t")
    {
        if (string.IsNullOrEmpty(str)) return str;

        var indent = string.Concat(Enumerable.Repeat(indentWith, count));
        return indent + str.Replace("\n", "\n" + indent);
    }

    public static string FormatMessageColored(this string message, TestReport.ReportType reportType)
    {
        const string ANSI_RESET = "\u001b[0m";
        const string ANSI_BLUE = "\u001b[34m";
        const string ANSI_YELLOW = "\u001b[33m";
        const string ANSI_BOLD = "\u001b[1m";
        const string ANSI_ITALIC = "\u001b[3m";

        var sb = new StringBuilder();

        // Header line (always visible)
        switch (reportType)
        {
            case TestReport.ReportType.STDOUT:
                sb.AppendLine($"{ANSI_BLUE}{ANSI_BOLD}Standard Output: {ANSI_ITALIC}{ANSI_RESET}");
                sb.AppendLine($"{ANSI_BLUE}──────────────────────────────────────────{ANSI_RESET}");

                break;
            case TestReport.ReportType.WARN:
                sb.AppendLine($"{ANSI_YELLOW}{ANSI_BOLD}Warning:{ANSI_ITALIC}{ANSI_RESET}");

                break;
        }

        sb.Append(message);
        sb.Append(Environment.NewLine);
        return sb.ToString();
    }
}
