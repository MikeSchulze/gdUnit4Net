using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GdUnit4.Asserts
{
    internal sealed class AssertFailures
    {
        public const string WARN_COLOR = "#EFF883";
        public const string ERROR_COLOR = "#CD5C5C";
        public const string VALUE_COLOR = "#1E90FF";

        private static Func<object, string> classFormatter = value => value?.GetType().Name ?? "<Null>";

        private static Func<object, string> godotClassFormatter = value =>
        {
            if (value != null)
            {
                return ((Godot.GodotObject)value).GetClass();
            }
            return "<Null>";
        };

        private static string SimpleClassName(Type type)
        {
            string name = type.FullName?.Replace("[", "")?.Replace("]", "")!;
            if (!type.IsGenericType) return name;

            var genericArguments = string.Join(", ", type.GetGenericArguments().Select(t => SimpleClassName(t)));
            return $"{name.Substring(0, name.IndexOf('`'))}<{genericArguments}>";
        }

        private static string FormatDictionary(IDictionary dict, string color)
        {
            if (dict.Keys.Count == 0)
                return string.Format($"[color={color}]<Empty>[/color]");

            var keyValues = new ArrayList();
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key;
                var value = entry.Value;
                keyValues.Add($"{{{key.Formated()}, {value.Formated()}}}");
            }
            string pairs = string.Join("; ", keyValues.ToArray());
            return $"[color={color}]{pairs}[/color]";
        }

        private static string FormatDictionary(IDictionary<Godot.Variant, Godot.Variant> dict, string color)
        {
            if (dict.Keys.Count == 0)
                return string.Format($"[color={color}]<Empty>[/color]");

            var keyValues = new ArrayList();
            foreach (KeyValuePair<Godot.Variant, Godot.Variant> entry in dict)
            {
                object? key = entry.Key.UnboxVariant();
                object? value = entry.Value.UnboxVariant();
                keyValues.Add($"{{{key.Formated()}, {value.Formated()}}}");
            }
            string pairs = string.Join("; ", keyValues.ToArray());
            return $"[color={color}]{pairs}[/color]";
        }

        private static string FormatEnumerable(IEnumerable enumerable, string color)
        {
            var enumerator = enumerable.GetEnumerator();
            if (enumerator.MoveNext() == false)
                return string.Format($"[color={color}]<Empty>[/color]");

            var keyValues = new ArrayList();
            do
            {
                keyValues.Add(enumerator.Current);
            } while (enumerator.MoveNext());
            return $"[color={color}][{keyValues.Formated()}][/color]";
        }

        public static string FormatValue(object? value, string color, bool quoted, bool printType = true)
        {
            if (value == null)
                return "<Null>";

            if (value is string vs)
                return quoted ? vs.Formated() : vs;

            if (value is Type)
                return string.Format($"[color={color}]<{value}>[/color]");


            if (value is IDictionary dict)
                return FormatDictionary(dict, color);

            if (value is Godot.Collections.Dictionary gDict)
                return FormatDictionary(gDict, color);

            if (value is IEnumerable values)
                return FormatEnumerable(values, color);

            Type type = value.GetType();
            if (type.IsArray)
            {
                var asArray = ((IEnumerable)value).Cast<object>()
                             .Select(x => x?.ToString() ?? "<Null>")
                             .ToArray();
                return asArray.Length == 0
                    ? (type.IsArray || value is Godot.Collections.Array ? "[]" : "")
                    : "[color=" + color + "][" + string.Join(", ", asArray) + "][/color]";
            }

            if (type.IsClass && !(value is string) || value is Type)
                return string.Format("[color={0}]<{1}>[/color]", color, SimpleClassName(type));
            return quoted ? $"'[color={color}]{value.Formated()}[/color]'" : $"[color={color}]{value.Formated()}[/color]";
        }

        private static string FormatCurrent(object? value, bool printType = true) => FormatValue(value, VALUE_COLOR, true, printType).UnixFormat();
        private static string FormatExpected(object? value, bool printType = true) => FormatValue(value, VALUE_COLOR, true, printType).UnixFormat();
        private static string FormatFailure(object value) => FormatValue(value, ERROR_COLOR, false).UnixFormat();

        public static string IsTrue() =>
            string.Format("{0} {1} but is {2}",
                FormatFailure("Expecting:"),
                FormatExpected(true),
                FormatCurrent(false));

        public static string IsFalse() =>
            string.Format("{0} {1} but is {2}",
                FormatFailure("Expecting:"),
                FormatExpected(false),
                FormatCurrent(true));

        public static string IsEqual(object? current, object? expected) =>
            current is IEnumerable || expected is IEnumerable ?
                string.Format("""
                    {0}
                    {1}
                     but is
                    {2}
                    """,
                    FormatFailure("Expecting be equal:"),
                    FormatExpected(expected).Indentation(1),
                    FormatCurrent(current).Indentation(1))
                :
                string.Format("""
                    {0}
                    {1} but is {2}
                    """,
                    FormatFailure("Expecting be equal:"),
                    FormatExpected(expected).Indentation(1),
                    FormatCurrent(current));

        public static string IsEqual(IEnumerable current, IEnumerable expected) =>
            string.Format("""
                {0}
                {1}
                  but is
                {2}
                """,
                FormatFailure("Expecting be equal:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current).Indentation(1));

        public static string IsEqualIgnoringCase(object? current, object expected) =>
            string.Format("""
                {0}
                {1}
                 but is
                {2}
                """,
                FormatFailure("Expecting be equal (ignoring case):"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current).Indentation(1));

        public static string IsNotEqual(object? current, object? expected) =>
            current is IEnumerable || expected is IEnumerable ?
                string.Format("""
                    {0}
                    {1}
                     but is
                    {2}
                    """,
                    FormatFailure("Expecting be NOT equal:"),
                    FormatExpected(expected).Indentation(1),
                    FormatCurrent(current).Indentation(1))
                :
                string.Format("""
                    {0}
                    {1} but is {2}
                    """,
                    FormatFailure("Expecting be NOT equal:"),
                    FormatExpected(expected).Indentation(1),
                    FormatCurrent(current));

        public static string IsNotEqual(IEnumerable current, IEnumerable expected) =>
            string.Format("""
                {0}
                {1}
                 but is
                {2}
                """,
                FormatFailure("Expecting be NOT equal:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current).Indentation(1));

        public static string IsNotEqualIgnoringCase(object? current, object expected) =>
            string.Format("""
                {0}
                {1}
                 but is
                {2}
                """,
                FormatFailure("Expecting be NOT equal (ignoring case):"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current).Indentation(1));

        public static string IsNull(object current) =>
            string.Format("""
                {0}
                 but is
                {1}
                """,
                FormatFailure("Expecting be <Null>:"),
                FormatCurrent(current).Indentation(1));

        public static string IsNotNull(object? current) => "Expecting be NOT <Null>:";

        public static string IsEmpty(int size, bool isNull) =>
            isNull ?
                string.Format("""
                    {0}
                     but is <Null>
                    """, FormatFailure("Expecting be empty:"))
                :
                string.Format("""
                    {0}
                     but has size {1}
                    """, FormatFailure("Expecting be empty:"), FormatCurrent(size));

        public static string IsEmpty(string? current) =>
            string.Format("""
                {0}
                 but is
                {1}
                """,
                FormatFailure("Expecting be empty:"),
                FormatCurrent(current).Indentation(1));

        public static string IsNotEmpty() =>
            string.Format("""
                {0}
                 but is empty
                """,
                FormatFailure("Expecting being NOT empty:"));

        public static string NotInstanceOf(Type? expected) =>
            string.Format("""
                {0}
                {1}
                """,
                FormatFailure("Expecting be NOT a instance of:"),
                FormatExpected(expected).Indentation(1));

        public static string IsInstanceOf(Type? current, Type expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expected be instance of:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current));

        public static string IsSame(object? current, object expected) =>
            string.Format("""
                {0}
                {1}
                 to refer to the same object
                {2}
                """,
                FormatFailure("Expecting be same:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current).Indentation(1));

        public static string IsNotSame(object? expected) =>
            string.Format("{0} {1}",
                FormatFailure("Expecting be NOT same:"),
                FormatExpected(expected));

        public static string IsBetween(object? current, object from, object to) =>
            string.Format("""
                {0}
                {1}
                 in range between
                {2} <> {3}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current).Indentation(1),
                FormatExpected(from).Indentation(1),
                FormatExpected(to));

        public static string IsNotBetween(object? current, object from, object to) =>
            string.Format("""
                {0}
                {1}
                 be NOT in range between
                {2} <> {3}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current).Indentation(1),
                FormatExpected(from).Indentation(1),
                FormatExpected(to));

        public static string IsEven(object? current) =>
            string.Format("""
                {0}
                 but is {1}
                """,
                FormatFailure("Expecting be even:"),
                FormatCurrent(current));

        public static string IsOdd(object? current) =>
            string.Format("""
                {0}
                 but is {1}
                """,
                FormatFailure("Expecting be odd:"),
                FormatCurrent(current));

        public static string HasSize(object? current, object expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expecting size:"),
                FormatExpected(expected).Indentation(1),
                current == null ? "unknown" : FormatCurrent(current));

        public static string IsGreater(object current, object expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expecting to be greater than:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current));

        public static string IsGreaterEqual(object current, object expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expecting to be greater than or equal:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current));

        public static string IsLess(object current, object expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expecting to be less than:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current));

        public static string IsLessEqual(object current, object expected) =>
            string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure("Expecting to be less than or equal:"),
                FormatExpected(expected).Indentation(1),
                FormatCurrent(current));

        public static string IsNegative(object current) =>
            string.Format("""
                {0}
                 but is {1}
                """,
                FormatFailure("Expecting be negative:"),
                FormatCurrent(current));

        public static string IsNotNegative(object current) =>
            string.Format("""
                {0}
                 but is {1}
                """,
                FormatFailure("Expecting be NOT negative:"),
                FormatCurrent(current));

        public static string IsNotZero() =>
            string.Format("""
                {0}
                 but is '0'
                """,
                FormatFailure("Expecting be NOT zero:"));

        public static string IsZero(object? current) =>
            string.Format("""
                {0}
                 but is {1}
                """,
                FormatFailure("Expecting be zero:"),
                FormatCurrent(current));

        public static string IsIn(object? current, object expected) =>
            string.Format("""
                {0}
                {1}
                 is in
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current).Indentation(1),
                FormatExpected(expected).Indentation(1));

        public static string IsNotIn(object? current, object expected) =>
            string.Format("""
                {0}
                {1}
                 is not in
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current).Indentation(1),
                FormatExpected(expected).Indentation(1));

        public static string Contains<T>(IEnumerable<T>? current, IEnumerable<T> expected, List<T> notFound) =>
            string.Format("""
                {0}
                {1}
                 do contains (in any order)
                {2}
                 but could not find elements:
                {3}
                """,
                FormatFailure("Expecting contains elements:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1),
                FormatExpected(notFound, false).Indentation(1));

        public static string ContainsExactly(IEnumerable<object?>? current, IEnumerable<object?> expected, List<object?> notFound, List<object?> notExpected)
        {
            if (notExpected.Count == 0 && notFound.Count == 0)
                return string.Format("""
                    {0}
                    {1}
                     do contains (in same order)
                    {2}
                     but has different order {3}
                    """,
                    FormatFailure("Expecting contains exactly elements:"),
                    FormatCurrent(current, false).Indentation(1),
                    FormatExpected(expected, false).Indentation(1),
                    FindFirstDiff(current, expected));

            var message = string.Format("""
                {0}
                {1}
                 do contains (in same order)
                {2}
                """,
                FormatFailure("Expecting contains exactly elements:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

            if (notExpected.Count > 0)
                message += $"""
                    
                     but some elements where not expected:
                    {FormatExpected(notExpected, false).Indentation(1)}
                    """;
            if (notFound.Count > 0)
                message += string.Format("""
                    
                     {0} could not find elements:
                    {1}
                    """, notExpected.Count == 0 ? "but" : "and", FormatExpected(notFound, false).Indentation(1));
            return message.UnixFormat();
        }

        public static string ContainsExactlyInAnyOrder(IEnumerable<object?>? current, IEnumerable<object?> expected, List<object?> notFound, List<object?> notExpected)
        {
            if (notExpected.Count == 0 && notFound.Count == 0)
                return string.Format("""
                    {0}
                    {1}
                     do contains (in any order)
                    {2}
                     but has different order {3}
                    """,
                    FormatFailure("Expecting contains exactly elements:"),
                    FormatCurrent(current, false).Indentation(1),
                    FormatExpected(expected, false).Indentation(1),
                    FindFirstDiff(current, expected));

            var message = string.Format("""
                {0}
                {1}
                 do contains (in any order)
                {2}
                """,
                FormatFailure("Expecting contains exactly elements:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));
            if (notExpected.Count > 0)
                message += $"""
                    
                     but some elements where not expected:
                    {FormatExpected(notExpected, false).Indentation(1)}
                    """;
            if (notFound.Count > 0)
                message += string.Format("""
                    
                     {0} could not find elements:
                    {1}
                    """, notExpected.Count == 0 ? "but" : "and", FormatExpected(notFound, false).Indentation(1));
            return message;
        }

        public static string ContainsKeyValue(IDictionary expected) =>
            string.Format("""
                {0}
                {1}
                """,
                FormatFailure("Expecting do contain entry:"),
                FormatCurrent(expected, false).Indentation(1));


        public static string ContainsKeyValue(IDictionary expected, object? currentValue) =>
             string.Format("""
                {0}
                {1}
                 found key but value is
                {2}
                """,
                FormatFailure("Expecting do contain entry:"),
                FormatCurrent(expected, true).Indentation(1),
                FormatCurrent(currentValue, true).Indentation(1));

        public static string NotContains(string current, string expected) =>
            string.Format("""
                {0}
                {1}
                 do not contain
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string NotContains<T>(IEnumerable<T>? current, IEnumerable<T> expected, List<T> found) =>
            string.Format("""
                {0}
                {1}
                 do NOT contains (in any order)
                {2}
                 but found elements:
                {3}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1),
                FormatExpected(found, false).Indentation(1));

        public static string NotContainsIgnoringCase(string current, string expected) =>
            string.Format("""
                {0}
                {1}
                 do not contain (ignoring case)
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string HasValue(string methodName, object? current, object expected) =>
            string.Format("""
                {0}
                {1} to be {2} but is {3}
                """,
                FormatFailure("Expecting Property:"),
                FormatCurrent(methodName, false).Indentation(1),
                FormatCurrent(expected, false),
                FormatCurrent(current, false));

        public static string Contains(string? current, string expected) =>
            string.Format("""
                {0}
                {1}
                 do contains
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string ContainsIgnoringCase(string? current, string expected) =>
            string.Format("""
                {0}
                {1}
                 do contains (ignoring case)
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string EndsWith(string? current, string expected) =>
            string.Format("""
                {0}
                {1}
                 to end with
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string StartsWith(string? current, string expected) =>
            string.Format("""
                {0}
                {1}
                 to start with
                {2}
                """,
                FormatFailure("Expecting:"),
                FormatCurrent(current, false).Indentation(1),
                FormatExpected(expected, false).Indentation(1));

        public static string HasLength(string? current, int currentLength, int expectedLength, IStringAssert.Compare comparator)
        {
            var errorMessage = "";
            switch (comparator)
            {
                case IStringAssert.Compare.EQUAL:
                    errorMessage = "Expecting length:";
                    break;
                case IStringAssert.Compare.LESS_THAN:
                    errorMessage = "Expecting length to be less than:";
                    break;
                case IStringAssert.Compare.LESS_EQUAL:
                    errorMessage = "Expecting length to be less than or equal:";
                    break;
                case IStringAssert.Compare.GREATER_THAN:
                    errorMessage = "Expecting length to be greater than:";
                    break;
                case IStringAssert.Compare.GREATER_EQUAL:
                    errorMessage = "Expecting length to be greater than or equal:";
                    break;
                default:
                    errorMessage = "Invalid comperator";
                    break;
            }
            return string.Format("""
                {0}
                {1} but is {2}
                """,
                FormatFailure(errorMessage),
                FormatExpected(expectedLength).Indentation(1),
                currentLength == -1 ? "unknown" : FormatCurrent(currentLength),
                FormatCurrent(current));
        }

        internal static string IsEmitted(object? current, string signal, Godot.Variant[] args) =>
            string.Format("""
                {0}
                {1}
                 by
                {2}
                """,
                FormatFailure("Expecting do emitting signal:"),
                FormatExpected($"{signal}({args.Formated()})").Indentation(1),
                FormatCurrent(current, true).Indentation(1));
        internal static string IsNotEmitted(object? current, string signal, Godot.Variant[] args) =>
            string.Format("""
                {0}
                {1}
                 by
                {2}
                """,
                FormatFailure("Expecting do NOT emitting signal:"),
                FormatExpected($"{signal}({args.Formated()})").Indentation(1),
                FormatCurrent(current, true).Indentation(1));

        internal static string IsSignalExists(object current, string signal) =>
             string.Format("""
                {0}
                {1}
                 on
                {2}
                """,
                FormatFailure("Expecting signal exists:"),
                FormatExpected($"{signal}()").Indentation(1),
                FormatCurrent(current, true).Indentation(1));

        static string? FindFirstDiff(IEnumerable<object?>? left, IEnumerable<object?>? right)
        {
            if (left is null || right is null)
                return null;
            foreach (var it in left.Select((value, i) => new { Value = value, Index = i }))
            {
                var l = it.Value;
                var r = right?.ElementAt(it.Index);
                if (!Comparable.IsEqual(l, r).Valid)
                    return $"at position {FormatCurrent(it.Index)}\n    {FormatCurrent(l)} vs {FormatExpected(r)}";
            }
            return null;
        }

    }
}
