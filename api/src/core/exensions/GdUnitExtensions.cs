using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace GdUnit4
{

    /// <summary>
    /// A util extension to format Godot object into a string representation
    /// </summary>
    public static class GdUnitExtensions
    {

        public static string ToSnakeCase(this string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input!;
            // Use regular expressions to match and replace camel case patterns
            return Regex.Replace(input, @"(\p{Ll})(\p{Lu})", "$1_$2").ToLower();
        }

        private static string Format(this object? value)
        {
            if (value is String asString)
                return asString.Formated();
            if (value is IEnumerable en)
                return en.Formated();
            if (value is Godot.Variant asVariant)
                return asVariant.Formated();

            return value?.ToString() ?? "<Null>";
        }


        public static string Formated(this object? value) => Format(value);
        public static string Formated(this string? value) => $"\"{value?.ToString()}\"" ?? "<Null>";
        public static string Formated(this Godot.Variant value) => value.ToString();
        public static string Formated(this Godot.Variant[] args, int indentation = 0) => string.Join(", ", args.Cast<Godot.Variant>().Select(v => v.Formated())).Indentation(indentation);
        public static string Formated(this Godot.Collections.Array args, int indentation = 0) => args.Cast<IEnumerable>().Formated(indentation);
        public static string Formated(this object?[] args, int indentation = 0) => string.Join(", ", args.ToArray().Select(Formated)).Indentation(indentation);
        public static string Formated(this IEnumerable args, int indentation = 0) => string.Join(", ", args.Cast<object>().Select(Formated)).Indentation(indentation);

        public static string UnixFormat(this string value) => value.Replace("\r", string.Empty);

        public static string Indentation(this string value, int indentation)
        {
            if (indentation == 0 || string.IsNullOrEmpty(value))
                return value;
            var indent = new string(' ', indentation * 4);
            string[] lines = value.UnixFormat().Split("\n");
            return string.Join("\n", lines.Select(line => indent + line));
        }

        public static string Humanize(this TimeSpan t)
        {
            var parts = new List<String>();
            if (t.Hours > 1)
                parts.Add($@"{t:%h}h");
            if (t.Minutes > 0)
                parts.Add($@"{t:%m}min");
            if (t.Seconds > 0)
                parts.Add($@"{t:%s}s");
            if (t.Milliseconds > 0)
                parts.Add($@"{t:fff}ms");
            return String.Join(" ", parts);
        }

        public static string RichTextNormalize(this string? input) => Regex.Replace(input?.UnixFormat() ?? "", "\\[/?(b|color).*?\\]", string.Empty);
    }
}
