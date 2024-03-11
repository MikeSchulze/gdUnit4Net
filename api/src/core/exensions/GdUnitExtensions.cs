namespace GdUnit4;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using GdUnit4.Asserts;
using System.Threading;
using System.Diagnostics;
using GdUnit4.Executions;

/// <summary>
/// A util extension to format Godot object into a string representation
/// </summary>
public static partial class GdUnitExtensions
{
    public static string ToSnakeCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input!;
        // Use regular expressions to match and replace camel case patterns
        return RegexToSnakeCase().Replace(input, "$1_$2").ToLower();
    }

    internal static string Format(this object? value)
        => value switch
        {
            string asString => asString.Formatted(),
            IEnumerable en => en.Formatted(),
            _ => value?.ToString() ?? "<Null>",
        };

    public static string Formatted(this object? value) => value.Format();
    public static string Formatted(this string? value) => $"\"{value?.ToString()}\"" ?? "<Null>";
    public static string Formatted(this Godot.Variant[] args, int indentation = 0) => string.Join(", ", args.Cast<Godot.Variant>().Select(v => v.Formatted())).Indentation(indentation);
    public static string Formatted(this Godot.Collections.Array args, int indentation = 0) => args.UnboxVariant()?.Formatted(indentation) ?? "<empty>";
    public static string Formatted(this object?[] args, int indentation = 0) => string.Join(", ", args.ToArray().Select(Formatted)).Indentation(indentation);
    public static string Formatted(this IEnumerable args, int indentation = 0) => string.Join(", ", args.Cast<object>().Select(Formatted)).Indentation(indentation);
    public static string UnixFormat(this string value) => value.Replace("\r", string.Empty);

    public static string Indentation(this string value, int indentation)
    {
        if (indentation == 0 || string.IsNullOrEmpty(value))
            return value;
        var indent = new string(' ', indentation * 4);
        var lines = value.UnixFormat().Split("\n");
        return string.Join("\n", lines.Select(line => indent + line));
    }

    public static string Humanize(this TimeSpan t)
    {
        var parts = new List<string>();
        if (t.Hours > 1)
            parts.Add($@"{t:%h}h");
        if (t.Minutes > 0)
            parts.Add($@"{t:%m}min");
        if (t.Seconds > 0)
            parts.Add($@"{t:%s}s");
        if (t.Milliseconds > 0)
            parts.Add($@"{t:fff}ms");
        return string.Join(" ", parts);
    }

    public static string RichTextNormalize(this string? input) => RegexTextNormalize().Replace(input?.UnixFormat() ?? "", string.Empty);

    private static int GetWithTimeoutLineNumber()
    {
        var saveStackTrace = new StackTrace(true);
        return saveStackTrace.FrameCount > 4 ? saveStackTrace.GetFrame(4)!.GetFileLineNumber() : -1;
    }

    public static async Task<ISignalAssert> WithTimeout(this Task<ISignalAssert> task, int timeoutMillis)
    {
        using var token = new CancellationTokenSource();
        var wrapperTask = Task.Run(async () => await task.ConfigureAwait(false));
        var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
        token.Cancel();
        if (completedTask == wrapperTask)
            return await task.ConfigureAwait(false);
        else
        {
            var data = Thread.GetData(Thread.GetNamedDataSlot("SignalCancellationToken"));
            if (data is CancellationTokenSource cancelToken)
                cancelToken.Cancel();
            return await task.ConfigureAwait(false);
        }
    }

    public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMillis)
    {
        var lineNumber = GetWithTimeoutLineNumber();
        var wrapperTask = Task.Run(async () => await task);
        using var token = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
        if (completedTask != wrapperTask)
            throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
        token.Cancel();
        return await task;
    }

    public static async Task WithTimeout(this Task task, int timeoutMillis)
    {
        var lineNumber = GetWithTimeoutLineNumber();
        var wrapperTask = Task.Run(async () => await task);
        using var token = new CancellationTokenSource();
        var completedTask = await Task.WhenAny(wrapperTask, Task.Delay(timeoutMillis, token.Token));
        if (completedTask != wrapperTask)
            throw new ExecutionTimeoutException($"Assertion: Timed out after {timeoutMillis}ms.", lineNumber);
        token.Cancel();
        await task;
    }

    [GeneratedRegex("\\[/?(b|color).*?\\]")]
    private static partial Regex RegexTextNormalize();

    [GeneratedRegex("(\\p{Ll})(\\p{Lu})")]
    private static partial Regex RegexToSnakeCase();
}
