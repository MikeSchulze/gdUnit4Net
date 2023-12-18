using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace GdUnit4
{

    /// <summary>
    /// A extension to compare C# Objects and Godot Objects by unboxing Variants.
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

        public static bool VariantEquals([NotNullWhen(true)] this IEnumerable? left, IEnumerable? right, MODE compareMode)
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
            if (left is System.ValueType && right is System.ValueType)
                return left!.Equals(right);
            return left!.Equals(right);
        }


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
                    converted.Add(item.ToVariant());
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Can't convert {item} to Variant\n {e.StackTrace}");
                    converted.Add(Godot.Variant.CreateFrom("n.a"));
                }
            }
            return converted;
        }

        public static Godot.Collections.Dictionary<Godot.Variant, Godot.Variant> ToGodotTypedDictionary<K, V>(this IDictionary<K, V> dict)
        {
            var converted = new Godot.Collections.Dictionary<Godot.Variant, Godot.Variant>();
            foreach (var (key, value) in dict)
                converted[key.ToVariant()] = value.ToVariant();
            return converted;
        }

        public static Godot.Collections.Dictionary ToGodotDictionary(this IDictionary dict)
        {
            var converted = new Godot.Collections.Dictionary();
            foreach (var key in dict.Keys)
                converted[key.ToVariant()] = dict[key].ToVariant();
            return converted;
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

        internal static async Task<object?> Invoke(object Instance, string MethodName, params Variant[] Args)
        {
            if (Instance is Godot.GodotObject goi && goi.HasMethod(MethodName))
            {
                Variant current = goi.Call(MethodName, Args);
                if (current.VariantType != Variant.Type.Nil && current.As<GodotObject>().GetClass() == "GDScriptFunctionState")
                {
                    Variant[] results = await goi.ToSignal(current.As<GodotObject>(), "completed");
                    return results[0].UnboxVariant();
                }
                return current;
            }

            object?[] parameters = Args.UnboxVariant()?.ToArray() ?? new object[] { };
            MethodInfo? methodInfo = Instance.GetType().GetMethod(MethodName);

            object? result = methodInfo!.Invoke(Instance, parameters);
            if (result is Task task)
            {
                await task.ConfigureAwait(false);
                Type resultType = task.GetType().GetTypeInfo().GetGenericArguments()[0];
                PropertyInfo? resultProperty = task.GetType().GetProperty("Result");
                if (resultProperty is null)
                    throw new InvalidOperationException("Task does not have a 'Result' property.");
                return resultProperty.GetValue(task);
            }
            return result;
        }
    }
}
