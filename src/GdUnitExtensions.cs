using System;
using System.Collections;
using System.Collections.Generic;

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

        public static Godot.Collections.Array ToGodotArray(this IEnumerable elements)
        {
            var converted = new Godot.Collections.Array();
            foreach (var item in elements)
            {
                converted.Add(Godot.Variant.From(item));
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


        public static string Formated(this Godot.Collections.Array args) => $"{string.Join(", ", args)}";
        public static string Formated(this Godot.Variant[] args) => $"{string.Join(", ", args)}";
        public static string Formated(this IEnumerable args) => $"{string.Join(", ", args)}";
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
