namespace GdUnit4.Core.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Godot;
using Godot.Collections;

using Array = Godot.Collections.Array;

/// <summary>
///     An extension to compare C# Objects and Godot Objects by unboxing Variants.
/// </summary>
public static class GodotObjectExtensions
{
    internal static SceneTree Instance =>
        Engine.GetMainLoop() as SceneTree ?? throw new InvalidOperationException("SceneTree is not initialized");

    /// <summary>
    ///     A utility to synchronize the current thread with the Godot physics thread.
    ///     This can be used to await the completion of a single physics frame in Godot.
    /// </summary>
    internal static SignalAwaiter SyncProcessFrame =>
        Instance.ToSignal(Instance, SceneTree.SignalName.ProcessFrame);

    /// <summary>
    ///     A util to synchronize the current thread with the Godot physics thread
    /// </summary>
    internal static SignalAwaiter SyncPhysicsFrame =>
        Instance.ToSignal(Instance, SceneTree.SignalName.PhysicsFrame);

    internal static bool VariantEquals<T>([NotNullWhen(true)] this T? inLeft, T? inRight, Mode compareMode = Mode.CaseSensitive)
    {
        object? left = inLeft.UnboxVariant();
        object? right = inRight.UnboxVariant();
        if (left == null && right == null)
            return true;

        if (left == null || right == null)
            return false;

        if (ReferenceEquals(left, right))
            return true;

        var type = left.GetType();
        if (type.IsPrimitive || typeof(string) == type || left is IEquatable<T>)
        {
            if (compareMode == Mode.CaseInsensitive && left is string ls && right is string rs)
                return ls.ToLower().Equals(rs.ToLower(), StringComparison.Ordinal);
            return left.Equals(right);
        }

        return DeepEquals(left, right, compareMode);
    }

    private static bool VariantEquals([NotNullWhen(true)] this IEnumerable? left, IEnumerable? right, Mode compareMode)
    {
        // Handle cases where both collections are null
        if (left is null && right is null)
            return true;

        // Handle cases where one collection is null
        if (left is null || right is null)
            return false;

        var itLeft = left.GetEnumerator();
        var itRight = right.GetEnumerator();

        try
        {
            while (itLeft.MoveNext() && itRight.MoveNext())
            {
                var keyEquals = itLeft.Current.VariantEquals(itRight.Current, compareMode);
                if (!keyEquals)
                    return false;
            }

            return !(itLeft.MoveNext() || itRight.MoveNext());
        }
        finally
        {
            (itLeft as IDisposable)?.Dispose();
            (itRight as IDisposable)?.Dispose();
        }
    }

    private static bool VariantEquals(this IDictionary left, IDictionary right, Mode compareMode)
    {
        if (left.Count != right.Count)
            return false;

        foreach (var key in left.Keys)
            if (!right.Contains(key) || !left[key]!.VariantEquals(right[key], compareMode))
                return false;

        return true;
    }

    private static bool DeepEquals<T>(T left, T right, Mode compareMode)
    {
        if (left is GodotObject lo && right is GodotObject ro)
        {
            var l = GodotObject2Dictionary(lo, new System.Collections.Generic.Dictionary<object, bool>());
            var r = GodotObject2Dictionary(ro, new System.Collections.Generic.Dictionary<object, bool>());
            return l.VariantEquals(r, compareMode);
        }

        if (left is IDictionary ld && right is IDictionary rd)
            return ld.VariantEquals(rd, compareMode);
        if (left is IEnumerable le && right is IEnumerable re)
            return le.VariantEquals(re, compareMode);
        if (left is ValueType && right is ValueType)
            return left.Equals(right);
        return left!.Equals(right);
    }

    public static Array<TVariant> ToGodotArray<[MustBeVariant] TVariant>(this IEnumerable elements) where TVariant : notnull
        => new(elements.ToGodotArray());

    public static Array<TVariant> ToGodotArray<[MustBeVariant] TVariant>(this TVariant[] args) where TVariant : notnull
        => new(ToGodotArray((IEnumerable)args));

    public static Array ToGodotArray(this object[] args)
        => ToGodotArray((IEnumerable)args);

    public static Array ToGodotArray(this IEnumerable<object> elements)
        => ToGodotArray((IEnumerable)elements);

    public static Array ToGodotArray(this IEnumerable elements)
    {
        var converted = new Array();
        foreach (var item in elements)
            try
            {
                converted.Add(item.ToVariant());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Can't convert {item} to Variant\n {e.StackTrace}");
                converted.Add(Variant.CreateFrom("n.a"));
            }

        return converted;
    }

    public static Godot.Collections.Dictionary<Variant, Variant> ToGodotTypedDictionary<[MustBeVariant] TKey, [MustBeVariant] TValue>(this IDictionary<TKey, TValue> dict)
        where TKey : notnull
        where TValue : notnull
    {
        var converted = new Godot.Collections.Dictionary<Variant, Variant>();
        foreach (var (key, value) in dict)
            converted[key.ToVariant()] = value.ToVariant();
        return converted;
    }

    public static Dictionary ToGodotDictionary(this IDictionary dict)
    {
        var converted = new Dictionary();
        foreach (var key in dict.Keys)
            converted[key.ToVariant()] = dict[key].ToVariant();
        return converted;
    }

    private static System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object?>> GodotObject2Dictionary(GodotObject? obj,
        System.Collections.Generic.Dictionary<object, bool> hashedObjects)
    {
        var r = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object?>>();
        if (obj == null)
            return r;

        var dict = new System.Collections.Generic.Dictionary<string, object?>();
        var type = obj.GetType();
        dict["@path"] = type.AssemblyQualifiedName;

        // collect custom fields
        foreach (var propertyName in type
                     .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                     .Select(e => e.Name)
                     .Where(name => !name.Equals("NativePtr", StringComparison.Ordinal)))
        {
            var propertyValue = obj.Get(propertyName);
            if (propertyValue.VariantType == Variant.Type.Object)
            {
                // prevent recursion
                if (!hashedObjects.TryAdd(obj, true))
                {
                    dict[propertyName] = propertyValue.UnboxVariant();
                    continue;
                }

                dict[propertyName] = GodotObject2Dictionary(propertyValue.AsGodotObject(), hashedObjects);
            }
            else
                dict[propertyName] = propertyValue.UnboxVariant();
        }

        // collect other fields
        foreach (var property in obj.GetPropertyList())
        {
            var propertyName = property["name"].AsStringName();
            var propertyType = property["type"];
            var propertyUsage = (PropertyUsageFlags)property["usage"].AsInt64();
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
                    if (!hashedObjects.TryAdd(obj, true))
                    {
                        dict[propertyName] = propertyValue.UnboxVariant();
                        continue;
                    }

                    dict[propertyName] = GodotObject2Dictionary(propertyValue.AsGodotObject(), hashedObjects);
                }
                else
                    dict[propertyName] = propertyValue.UnboxVariant();
            }
        }

        r[type.FullName!] = dict;
        return r;
    }

    internal static async Task<object?> Invoke(object instance, string methodName, params Variant[] args)
    {
        // if the instance has an GDScript attached we have to use the Godot `Call` method
        if (instance is GodotObject goi && goi.GetScript().UnboxVariant() is GDScript)
        {
            if (!goi.HasMethod(methodName))
                throw new MissingMethodException($"The method '{methodName}' not exist on this instance.");
            var current = goi.Call(methodName, args);
            if (current.VariantType == Variant.Type.Object && current.As<GodotObject>().GetClass() == "GDScriptFunctionState")
            {
                var results = await goi.ToSignal(current.As<GodotObject>(), "completed");
                return results[0].UnboxVariant();
            }

            return current;
        }

        // for C# implementations we use Invoke
        var mi = instance.GetType().GetMethod(methodName)
                 ?? throw new MissingMethodException($"The method '{methodName}' not exist on this instance.");
        object?[]? parameters = args.Length == 0 ? System.Array.Empty<object>() : args.UnboxVariant()?.ToArray();
        object? result;
        var parameterInfo = mi.GetParameters();
        if (mi.IsStatic == false)
            result = parameterInfo.Length == 0
                ? mi.Invoke(instance, null)
                : mi.Invoke(instance, parameters);
        else
            result = parameterInfo.Length == 0
                ? mi.Invoke(null, null)
                : mi.Invoke(null, parameters);
        if (result is Task task)
        {
            await task.ConfigureAwait(false);
            var resultProperty = task.GetType().GetProperty("Result")
                                 ?? throw new InvalidOperationException("Task does not have a 'Result' property.");
            return resultProperty.GetValue(task);
        }

        return result;
    }


    internal enum Mode
    {
        CaseSensitive,
        CaseInsensitive
    }
}
