using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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


        private static Dictionary<Type, Func<object?, string>> formatters = new Dictionary<Type, Func<object?, string>> {
                {typeof(string), (value) => $"\"{value?.ToString()}\"" ?? "<Null>"},
                {typeof(object), (value) =>  value?.GetType().Name ?? "<Null>"},
                {typeof(Godot.Vector2), (value) =>  ((Godot.Vector2)value!).ToString()},
                {typeof(Godot.Vector3), (value) =>  ((Godot.Vector3)value!).ToString()},
        };

        private static Func<object?, string> defaultFormatter = value => value?.ToString() ?? "<Null>";

        private static string Format(this object? value)
        {
            var type = value?.GetType();
            var formatter = type != null && formatters.ContainsKey(type) ? formatters[type] : defaultFormatter;
            return formatter(value);
        }


        public static string Formated(this Godot.Collections.Array args) => $"{string.Join(", ", args)}";
        public static string Formated(this object? value) => Format(value);
        public static string Formated(this String? value) => Format(value);
        public static string Formated(this Godot.Variant[] args) => $"{string.Join(", ", args)}";
        public static string Formated(this IEnumerable args) => $"{string.Join(", ", args)}";
        public static string Formated(this ArrayList args) => $"[{string.Join(", ", args.ToArray())}]";
        public static string Formated(this object[] args) => $"{string.Join(", ", args)}";


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
    }
}
