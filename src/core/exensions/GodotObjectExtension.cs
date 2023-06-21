using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Linq;

using Godot;

namespace GdUnit4
{

    /// <summary>
    /// A extension to convert c# types to Godot types
    /// </summary>
    public static class GodotObjectExtensions
    {

        public enum MODE
        {
            CASE_SENSITIVE,
            CASE_INSENSITIVE
        }


        public static bool VariantEquals<T>([NotNullWhen(true)] this T? inLeft, T? inRight, MODE compareMode = MODE.CASE_SENSITIVE)
        {
            object? left = inLeft.UnboxVariant();
            object? right = inRight.UnboxVariant();
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            if (object.ReferenceEquals(left, right))
                return true;

            var type = left.GetType();
            if (type.IsPrimitive || typeof(string).Equals(type) || left is IEquatable<T>)
            {
                if (compareMode == MODE.CASE_INSENSITIVE && left is String ls && right is String rs)
                    return ls.ToLower().Equals(rs.ToLower());
                return left.Equals(right);
            }
            return DeepEquals(left, right, compareMode);
        }


        public static bool VariantEquals([NotNullWhen(true)] this KeyValuePair<object?, object?> left, KeyValuePair<object?, object?> right)
        {
            return false;
        }

        public static bool VariantEquals([NotNullWhen(true)] this IEnumerable left, IEnumerable right, MODE compareMode)
        {
            // Handle cases where both collections are null
            if (left is null && right is null)
                return true;

            // Handle cases where one collection is null
            if (left is null || right is null)
                return false;

            IEnumerator itLeft = left.GetEnumerator();
            IEnumerator itRight = right.GetEnumerator();

            while (itLeft.MoveNext() && itRight.MoveNext())
            {
                var keyEquals = itLeft.Current.VariantEquals(itRight.Current, compareMode);
                if (!keyEquals)
                    return false;
            }
            return !(itLeft.MoveNext() || itRight.MoveNext());
        }

        public class CustomerComparer<TKey> : IComparer<TKey>
        {
            public int Compare(TKey? l, TKey? r)
            {
                return Comparer<TKey>.Default.Compare(l, r);
            }
        }

        public static bool VariantEquals<TKey, TValue>([NotNullWhen(true)] this IDictionary<TKey, TValue>? left, IDictionary<TKey, TValue>? right, MODE compareMode)
        {
            if (left!.Count != right?.Count)
                return false;

            // do sort by key first
            IComparer<TKey> comparer = new CustomerComparer<TKey>();

            IEnumerator<KeyValuePair<TKey, TValue>> itLeft = left!.OrderBy(kvs => kvs.Key, comparer).GetEnumerator();
            IEnumerator<KeyValuePair<TKey, TValue>> itRight = right?.OrderBy(kvp => kvp.Key, comparer).GetEnumerator() ?? Enumerable.Empty<KeyValuePair<TKey, TValue>>().GetEnumerator();

            while (itLeft.MoveNext() && itRight.MoveNext())
            {
                var keyEquals = itLeft.Current.Key.VariantEquals(itRight.Current.Key, compareMode);
                var valueEquals = itLeft.Current.Value.VariantEquals(itRight.Current.Value, compareMode);
                if (!keyEquals || !valueEquals)
                    return false;
            }
            return true;
        }

        public static bool VariantEquals([NotNullWhen(true)] this IDictionary left, IDictionary right, MODE compareMode)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var key in left.Keys)
            {
                if (!right.Contains(key) || !left[key]!.VariantEquals(right[key], compareMode))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool DeepEquals<T>(T left, T right, MODE compareMode)
        {
            if (left is GodotObject lo && right is GodotObject ro)
            {
                var l = GodotObject2Dictionary(lo, new Dictionary<object, bool>());
                var r = GodotObject2Dictionary(ro, new Dictionary<object, bool>());
                return l.VariantEquals(r, compareMode);
            }
            if (left is IDictionary ld && right is IDictionary rd)
                return ld.VariantEquals(rd, compareMode);
            if (left is IEnumerable le && right is IEnumerable re)
                return le.VariantEquals(re, compareMode);
            if (left is System.ValueType lvt && right is System.ValueType rvt)
                return left!.Equals(right);
            return left!.Equals(right);
        }

        private static Dictionary<String, Dictionary<String, object?>> GodotObject2Dictionary(GodotObject? obj, Dictionary<object, bool> hashedObjects)
        {
            var r = new Dictionary<String, Dictionary<String, object?>>();
            if (obj == null)
                return r;

            var dict = new Dictionary<String, object?>();
            Type type = obj.GetType();
            dict["@path"] = type.AssemblyQualifiedName;

            // collect custom fields
            foreach (var propertyName in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Select(e => e.Name).Where(name => !name.Equals("NativePtr")))
            {
                Variant propertyValue = obj.Get(propertyName);
                if (propertyValue.VariantType == Variant.Type.Object)
                {
                    // prevent recursion
                    if (hashedObjects.ContainsKey(obj))
                    {
                        dict[propertyName] = propertyValue.UnboxVariant();
                        continue;
                    }
                    hashedObjects[obj] = true;
                    dict[propertyName] = GodotObject2Dictionary(propertyValue.AsGodotObject(), hashedObjects);
                }
                else
                {
                    dict[propertyName] = propertyValue.UnboxVariant();
                }
            }

            // collect other fields
            foreach (Godot.Collections.Dictionary property in obj.GetPropertyList())
            {
                var propertyName = property["name"].AsStringName();
                var propertyType = property["type"];
                PropertyUsageFlags propertyUsage = (PropertyUsageFlags)property["usage"].AsInt64();
                var propertyValue = obj.Get(propertyName);
                if (propertyValue.VariantType == Variant.Type.Callable)
                    continue;

                //System.Console.WriteLine($"Property: {propertyName}:{propertyValue.VariantType}, {propertyUsage} {propertyValue.Obj}");
                var isScriptOrDefault = (propertyUsage & (PropertyUsageFlags.ScriptVariable | PropertyUsageFlags.Default)) != 0;
                var isCategory = (propertyUsage & PropertyUsageFlags.Category) != 0;
                if (isScriptOrDefault
                    && !isCategory
                    && propertyUsage != PropertyUsageFlags.None)
                {
                    if (propertyType.VariantType == Variant.Type.Object)
                    {
                        // prevent recursion
                        if (hashedObjects.ContainsKey(obj))
                        {
                            dict[propertyName] = propertyValue.UnboxVariant();
                            continue;
                        }
                        hashedObjects[obj] = true;
                        dict[propertyName] = GodotObject2Dictionary(propertyValue.AsGodotObject(), hashedObjects);
                    }
                    else
                    {
                        dict[propertyName] = propertyValue.UnboxVariant();
                    }
                }
            }
            r[type.FullName!] = dict;
            return r;
        }

        public static object? UnboxVariant<T>(this T? value)
        {
            if (value is Variant v)
            {
                if (v.VariantType == Variant.Type.Dictionary)
                {
                    return v.AsGodotDictionary().UnboxVariant();
                }
                return v.Obj;
            }
            if (value is Godot.Collections.Dictionary gd)
                return gd.UnboxVariant();
            return value;
        }

        private static IDictionary<object, object?> UnboxVariant(this Godot.Collections.Dictionary dict)
        {
            var unboxed = new Dictionary<object, object?>();
            foreach (KeyValuePair<Variant, Variant> kvp in dict)
                unboxed.Add(kvp.Key.UnboxVariant()!, kvp.Value.UnboxVariant());
            return unboxed;
        }
    }
}
