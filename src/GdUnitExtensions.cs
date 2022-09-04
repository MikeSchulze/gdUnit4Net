using System.Collections;
using System.Collections.Generic;

namespace GdUnit3
{
    /// <summary>
    /// A extension to convert c# types to Godot types
    /// </summary>
    public static class GdUnitExtensions
    {
        public static Godot.Collections.Array<T> ToGodotArray<T>(this T[] args) => new Godot.Collections.Array<T>(args);

        public static Godot.Collections.Array ToGodotArray(this object[] args) => new Godot.Collections.Array(args);

        public static string Formated(this IEnumerable args) => new Godot.Collections.Array(args).ToString();

        public static string Formated(this object[] args) => $"{string.Join(", ", args)}";
    }
}
