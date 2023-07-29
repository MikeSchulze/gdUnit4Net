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
                return $"[color={color}]<{SimpleClassName(type)}>[/color]";
            return quoted ? $"'[color={color}]{value.Formated()}[/color]'" : $"[color={color}]{value.Formated()}[/color]";
        }

        private static string FormatCurrent(object? value, bool printType = true) => FormatValue(value, VALUE_COLOR, true, printType).UnixFormat();
        private static string FormatExpected(object? value, bool printType = true) => FormatValue(value, VALUE_COLOR, true, printType).UnixFormat();
        private static string FormatFailure(object value) => FormatValue(value, ERROR_COLOR, false).UnixFormat();

        public static string IsTrue() =>
            $"{FormatFailure("Expecting:")} {FormatExpected(true)} but is {FormatCurrent(false)}";

        public static string IsFalse() =>
            $"{FormatFailure("Expecting:")} {FormatExpected(false)} but is {FormatCurrent(true)}";

        public static string IsEqual(object? current, object? expected) =>
            current is IEnumerable || expected is IEnumerable ? $"""
                    {FormatFailure("Expecting be equal:")}
                    {FormatExpected(expected).Indentation(1)}
                     but is
                    {FormatCurrent(current).Indentation(1)}
                    """
                : $"""
                    {FormatFailure("Expecting be equal:")}
                    {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                    """;

        public static string IsEqual(IEnumerable current, IEnumerable expected) =>
            $"""
                {FormatFailure("Expecting be equal:")}
                {FormatExpected(expected).Indentation(1)}
                  but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsEqualIgnoringCase(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting be equal (ignoring case):")}
                {FormatExpected(expected).Indentation(1)}
                 but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNotEqual(object? current, object? expected) =>
            current is IEnumerable || expected is IEnumerable ? $"""
                    {FormatFailure("Expecting be NOT equal:")}
                    {FormatExpected(expected).Indentation(1)}
                     but is
                    {FormatCurrent(current).Indentation(1)}
                    """
                : $"""
                    {FormatFailure("Expecting be NOT equal:")}
                    {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                    """;

        public static string IsNotEqual(IEnumerable current, IEnumerable expected) =>
            $"""
                {FormatFailure("Expecting be NOT equal:")}
                {FormatExpected(expected).Indentation(1)}
                 but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNotEqualIgnoringCase(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting be NOT equal (ignoring case):")}
                {FormatExpected(expected).Indentation(1)}
                 but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNull(object current) =>
            $"""
                {FormatFailure("Expecting be <Null>:")}
                 but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNotNull(object? current) => "Expecting be NOT <Null>:";

        public static string IsEmpty(int size, bool isNull) =>
            isNull ? $"""
                    {FormatFailure("Expecting be empty:")}
                     but is <Null>
                    """
                : $"""
                    {FormatFailure("Expecting be empty:")}
                     but has size {FormatCurrent(size)}
                    """;

        public static string IsEmpty(string? current) =>
            $"""
                {FormatFailure("Expecting be empty:")}
                 but is
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNotEmpty() =>
            $"""
                {FormatFailure("Expecting being NOT empty:")}
                 but is empty
                """;

        public static string NotInstanceOf(Type? expected) =>
            $"""
                {FormatFailure("Expecting be NOT a instance of:")}
                {FormatExpected(expected).Indentation(1)}
                """;

        public static string IsInstanceOf(Type? current, Type expected) =>
            $"""
                {FormatFailure("Expected be instance of:")}
                {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                """;

        public static string IsSame(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting be same:")}
                {FormatExpected(expected).Indentation(1)}
                 to refer to the same object
                {FormatCurrent(current).Indentation(1)}
                """;

        public static string IsNotSame(object? expected) =>
            $"{FormatFailure("Expecting be NOT same:")} {FormatExpected(expected)}";

        public static string IsBetween(object? current, object from, object to) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current).Indentation(1)}
                 in range between
                {FormatExpected(from).Indentation(1)} <> {FormatExpected(to)}
                """;

        public static string IsNotBetween(object? current, object from, object to) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current).Indentation(1)}
                 be NOT in range between
                {FormatExpected(from).Indentation(1)} <> {FormatExpected(to)}
                """;

        public static string IsEven(object? current) =>
            $"""
                {FormatFailure("Expecting be even:")}
                 but is {FormatCurrent(current)}
                """;

        public static string IsOdd(object? current) =>
            $"""
                {FormatFailure("Expecting be odd:")}
                 but is {FormatCurrent(current)}
                """;

        public static string HasSize(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting size:")}
                {FormatExpected(expected).Indentation(1)} but is {(current == null ? "unknown" : FormatCurrent(current))}
                """;

        public static string IsGreater(object current, object expected) =>
            $"""
                {FormatFailure("Expecting to be greater than:")}
                {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                """;

        public static string IsGreaterEqual(object current, object expected) =>
            $"""
                {FormatFailure("Expecting to be greater than or equal:")}
                {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                """;

        public static string IsLess(object current, object expected) =>
            $"""
                {FormatFailure("Expecting to be less than:")}
                {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                """;

        public static string IsLessEqual(object current, object expected) =>
            $"""
                {FormatFailure("Expecting to be less than or equal:")}
                {FormatExpected(expected).Indentation(1)} but is {FormatCurrent(current)}
                """;

        public static string IsNegative(object current) =>
            $"""
                {FormatFailure("Expecting be negative:")}
                 but is {FormatCurrent(current)}
                """;

        public static string IsNotNegative(object current) =>
            $"""
                {FormatFailure("Expecting be NOT negative:")}
                 but is {FormatCurrent(current)}
                """;

        public static string IsNotZero() =>
            $"""
                {FormatFailure("Expecting be NOT zero:")}
                 but is '0'
                """;

        public static string IsZero(object? current) =>
            $"""
                {FormatFailure("Expecting be zero:")}
                 but is {FormatCurrent(current)}
                """;

        public static string IsIn(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current).Indentation(1)}
                 is in
                {FormatExpected(expected).Indentation(1)}
                """;

        public static string IsNotIn(object? current, object expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current).Indentation(1)}
                 is not in
                {FormatExpected(expected).Indentation(1)}
                """;

        public static string Contains<T>(IEnumerable<T>? current, IEnumerable<T> expected, List<T> notFound) =>
            $"""
                {FormatFailure("Expecting contains elements:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do contains (in any order)
                {FormatExpected(expected, false).Indentation(1)}
                 but could not find elements:
                {FormatExpected(notFound, false).Indentation(1)}
                """;

        public static string ContainsExactly(IEnumerable<object?>? current, IEnumerable<object?> expected, List<object?> notFound, List<object?> notExpected)
        {
            if (notExpected.Count == 0 && notFound.Count == 0)
                return $"""
                    {FormatFailure("Expecting contains exactly elements:")}
                    {FormatCurrent(current, false).Indentation(1)}
                     do contains (in same order)
                    {FormatExpected(expected, false).Indentation(1)}
                     but has different order {FindFirstDiff(current, expected)}
                    """;

            var message = $"""
                {FormatFailure("Expecting contains exactly elements:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do contains (in same order)
                {FormatExpected(expected, false).Indentation(1)}
                """;

            if (notExpected.Count > 0)
                message += $"""
                    
                     but some elements where not expected:
                    {FormatExpected(notExpected, false).Indentation(1)}
                    """;
            if (notFound.Count > 0)
                message += $"""
                    
                     {(notExpected.Count == 0 ? "but" : "and")} could not find elements:
                    {FormatExpected(notFound, false).Indentation(1)}
                    """;
            return message.UnixFormat();
        }

        public static string ContainsExactlyInAnyOrder(IEnumerable<object?>? current, IEnumerable<object?> expected, List<object?> notFound, List<object?> notExpected)
        {
            if (notExpected.Count == 0 && notFound.Count == 0)
                return $"""
                    {FormatFailure("Expecting contains exactly elements:")}
                    {FormatCurrent(current, false).Indentation(1)}
                     do contains (in any order)
                    {FormatExpected(expected, false).Indentation(1)}
                     but has different order {FindFirstDiff(current, expected)}
                    """;

            var message = $"""
                {FormatFailure("Expecting contains exactly elements:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do contains (in any order)
                {FormatExpected(expected, false).Indentation(1)}
                """;
            if (notExpected.Count > 0)
                message += $"""
                    
                     but some elements where not expected:
                    {FormatExpected(notExpected, false).Indentation(1)}
                    """;
            if (notFound.Count > 0)
                message += $"""
                    
                     {(notExpected.Count == 0 ? "but" : "and")} could not find elements:
                    {FormatExpected(notFound, false).Indentation(1)}
                    """;
            return message;
        }

        public static string ContainsKeyValue(IDictionary expected) =>
            $"""
                {FormatFailure("Expecting do contain entry:")}
                {FormatCurrent(expected, false).Indentation(1)}
                """;


        public static string ContainsKeyValue(IDictionary expected, object? currentValue) =>
            $"""
                {FormatFailure("Expecting do contain entry:")}
                {FormatCurrent(expected, true).Indentation(1)}
                 found key but value is
                {FormatCurrent(currentValue, true).Indentation(1)}
                """;

        public static string NotContains(string current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do not contain
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string NotContains<T>(IEnumerable<T>? current, IEnumerable<T> expected, List<T> found) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do NOT contains (in any order)
                {FormatExpected(expected, false).Indentation(1)}
                 but found elements:
                {FormatExpected(found, false).Indentation(1)}
                """;

        public static string NotContainsIgnoringCase(string current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do not contain (ignoring case)
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string HasValue(string methodName, object? current, object expected) =>
            $"""
                {FormatFailure("Expecting Property:")}
                {FormatCurrent(methodName, false).Indentation(1)} to be {FormatCurrent(expected, false)} but is {FormatCurrent(current, false)}
                """;

        public static string Contains(string? current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do contains
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string ContainsIgnoringCase(string? current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 do contains (ignoring case)
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string EndsWith(string? current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 to end with
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string StartsWith(string? current, string expected) =>
            $"""
                {FormatFailure("Expecting:")}
                {FormatCurrent(current, false).Indentation(1)}
                 to start with
                {FormatExpected(expected, false).Indentation(1)}
                """;

        public static string HasLength(string? current, int currentLength, int expectedLength, IStringAssert.Compare comparator)
        {
            string errorMessage;
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
            $"""
                {FormatFailure("Expecting do emitting signal:")}
                {FormatExpected($"{signal}({args.Formated()})").Indentation(1)}
                 by
                {FormatCurrent(current, true).Indentation(1)}
                """;
        internal static string IsNotEmitted(object? current, string signal, Godot.Variant[] args) =>
            $"""
                {FormatFailure("Expecting do NOT emitting signal:")}
                {FormatExpected($"{signal}({args.Formated()})").Indentation(1)}
                 by
                {FormatCurrent(current, true).Indentation(1)}
                """;

        internal static string IsSignalExists(object current, string signal) =>
            $"""
                {FormatFailure("Expecting signal exists:")}
                {FormatExpected($"{signal}()").Indentation(1)}
                 on
                {FormatCurrent(current, true).Indentation(1)}
                """;

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
