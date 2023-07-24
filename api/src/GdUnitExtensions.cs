using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace GdUnit4
{

    /// <summary>
    /// A extension to convert c# types to Godot types
    /// </summary>
    public static class GdUnitExtensions
    {
        public static Godot.Collections.Array<T> ToGodotArray<[Godot.MustBeVariant] T>(this IEnumerable elements) => new Godot.Collections.Array<T>(elements.ToGodotArray());

        public static Godot.Collections.Array<T> ToGodotArray<[Godot.MustBeVariant] T>(this T[] args) => new Godot.Collections.Array<T>(ToGodotArray((IEnumerable)args));

        public static Godot.Collections.Array ToGodotArray(this object[] args) => ToGodotArray((IEnumerable)args);

        public static Godot.Collections.Array ToGodotArray(this IEnumerable<object> elements) => ToGodotArray((IEnumerable)elements);

        public static Godot.Collections.Array ToGodotArray(this IEnumerable elements)
        {
            var converted = new Godot.Collections.Array();
            foreach (var item in elements)
            {
                try
                {
                    if (item is String s)
                        converted.Add(Godot.Variant.CreateFrom(s));
                    else
                        converted.Add(Godot.Variant.From(item));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can't convert {item} to Variant\n {e.StackTrace}");
                    converted.Add(Godot.Variant.CreateFrom("n.a"));
                }
            }
            return converted;
        }

        public static Godot.Collections.Dictionary ToGodotDictionary(this Dictionary<string, object> dict)
        {
            var converted = new Godot.Collections.Dictionary();
            foreach (var item in dict)
            {
                converted.Add(item.Key, (Godot.Variant)item.Value);
            }
            return converted;
        }

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
        public static string Formated(this object[] args, int indentation = 0) => string.Join(", ", args.ToArray().Select(Formated)).Indentation(indentation);
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
