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
    internal static string ToSnakeCase(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return input!;
        // Use regular expressions to match and replace camel case patterns
        return RegexToSnakeCase().Replace(input, "$1_$2").ToLower();
    }

    internal static string FormatAsValueOrClass(object value)
    {
        // is class than we render the object with an identifier
        if ((value.GetType().IsClass && value is not string) || value is Type)
            return AssertFailures.AsObjectId(value);
        // fallback to default formatting
        return value.ToString() ?? "<Null>";
    }

    internal static string Formatted(this object? value)
    {
        if (value is Godot.Variant v)
            value = v.UnboxVariant();
        if (value == null)
            return "<Null>";
        if (value is string asString)
            return asString.Formatted();
        return value switch
        {
            IEnumerable en => en.Formatted(),
            Asserts.Tuple tuple => tuple.ToString(),
            _ => FormatAsValueOrClass(value!),
        };
    }

    internal static string Formatted(this Godot.Variant value)
        => Formatted(value as object);
    internal static string Formatted(this Asserts.Tuple? value)
        => value?.ToString() ?? "<Null>";
    internal static string Formatted(this string? value)
        => $"\"{value?.ToString()}\"" ?? "<Null>";

    internal static string Formatted(this Godot.Collections.Array args, int indentation = 0)
        => args.ToArray().Formatted(indentation);
    internal static string Formatted<[Godot.MustBeVariant] TValue>(this Godot.Collections.Array<TValue> args, int indentation = 0)
        => args.ToArray().Formatted(indentation);
    internal static string Formatted<TValue>(this TValue?[] args, int indentation = 0)
        => args.Length == 0
            ? "<Empty>"
            : "[" + string.Join(", ", args.ToArray().Select(v => Formatted(v))).Indentation(indentation) + "]";
    internal static string Formatted(this IEnumerable args, int indentation = 0)
        => Formatted(args.Cast<object?>().ToArray(), indentation);

    internal static string UnixFormat(this string value) => value.Replace("\r", string.Empty);

    internal static string Indentation(this string value, int indentation)
    {
        if (indentation == 0 || string.IsNullOrEmpty(value))
            return value;
        var indent = new string(' ', indentation * 4);
        var lines = value.UnixFormat().Split("\n");
        return string.Join("\n", lines.Select(line => indent + line));
    }

    internal static string Humanize(this TimeSpan t)
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

    internal static string RichTextNormalize(this string? input) => RegexTextNormalize().Replace(input?.UnixFormat() ?? "", string.Empty);

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
