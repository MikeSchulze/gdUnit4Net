namespace GdUnit4;

using System;
using System.Collections.Generic;

using Godot;

/// <summary>
/// A extension box/unbox Godot Variants
/// </summary>
public static class GodotVariantExtensions
{
    public static dynamic? UnboxVariant<T>(this T? value)
    {
        if (value is Variant v)
            return v.UnboxVariant();
        if (value is StringName sn)
            return sn.ToString();
        if (value is Godot.Collections.Dictionary gd)
            return gd.UnboxVariant();
        if (value is Godot.Collections.Array ga)
            return ga.UnboxVariant();
        if (value is Variant[] parameters)
            return parameters.UnboxVariant();
        return value;
    }

    private static Dictionary<object, object?> UnboxVariant(this Godot.Collections.Dictionary dict)
    {
        var unboxed = new Dictionary<object, object?>();
        foreach (var kvp in dict)
            unboxed.Add(kvp.Key.UnboxVariant()!, kvp.Value.UnboxVariant());
        return unboxed;
    }

    private static List<object?> UnboxVariant(this Godot.Collections.Array values)
    {
        var unboxed = new List<object?>();
        foreach (var value in values)
            unboxed.Add(value.UnboxVariant());
        return unboxed;
    }

    private static List<object?> UnboxVariant(this Variant[] values)
    {
        var unboxed = new List<object?>();
        foreach (var value in values)
            unboxed.Add(value.UnboxVariant());
        return unboxed;
    }

    internal static Variant ToVariant(this object? obj) =>
        Type.GetTypeCode(obj?.GetType()) switch
        {
            TypeCode.Empty => new Variant(),
            TypeCode.String => Variant.CreateFrom((string)obj!),
            TypeCode.Boolean => Variant.CreateFrom((bool)obj!),
            TypeCode.Char => Variant.CreateFrom(0 + (char)obj!),
            TypeCode.SByte => Variant.CreateFrom((sbyte)obj!),
            TypeCode.Byte => Variant.CreateFrom((byte)obj!),
            TypeCode.Int16 => Variant.CreateFrom((short)obj!),
            TypeCode.UInt16 => Variant.CreateFrom((ushort)obj!),
            TypeCode.Int32 => Variant.CreateFrom((int)obj!),
            TypeCode.UInt32 => Variant.CreateFrom((uint)obj!),
            TypeCode.Int64 => Variant.CreateFrom((long)obj!),
            TypeCode.UInt64 => Variant.CreateFrom((ulong)obj!),
            TypeCode.Single => Variant.CreateFrom((float)obj!),
            TypeCode.Double => Variant.CreateFrom((double)obj!),
            TypeCode.Decimal => Variant.CreateFrom((ulong)obj!),
            TypeCode.Object => ToVariantByType(obj!),
            TypeCode.DBNull => ToVariantByType(obj!),
            TypeCode.DateTime => ToVariantByType(obj!),
            _ => ToVariantByType(obj!)
        };

    private static Variant ToVariantByType(object obj)
    {
        if (obj is System.Collections.IList list)
            return list.ToGodotArray();

        if (obj is IDictionary<string, object> dict)
            return dict.ToGodotTypedDictionary();

        throw new NotImplementedException($"Cannot convert '{obj?.GetType()}' to Variant!");
    }

    private static dynamic? UnboxVariant(this Variant v) => v.VariantType switch
    {
        Variant.Type.Nil => null,
        Variant.Type.Bool => v.AsBool(),
        Variant.Type.Int => v.AsInt64(),
        Variant.Type.Float => v.AsDouble(),
        Variant.Type.String => v.AsString(),
        Variant.Type.Vector2 => v.AsVector2(),
        Variant.Type.Vector2I => v.AsVector2I(),
        Variant.Type.Rect2 => v.AsRect2(),
        Variant.Type.Rect2I => v.AsRect2I(),
        Variant.Type.Vector3 => v.AsVector3(),
        Variant.Type.Vector3I => v.AsVector3I(),
        Variant.Type.Transform2D => v.AsTransform2D(),
        Variant.Type.Vector4 => v.AsVector4(),
        Variant.Type.Vector4I => v.AsVector4I(),
        Variant.Type.Plane => v.AsPlane(),
        Variant.Type.Quaternion => v.AsQuaternion(),
        Variant.Type.Aabb => v.AsAabb(),
        Variant.Type.Basis => v.AsBasis(),
        Variant.Type.Transform3D => v.AsTransform3D(),
        Variant.Type.Projection => v.AsProjection(),
        Variant.Type.Color => v.AsColor(),
        Variant.Type.StringName => v.AsStringName(),
        Variant.Type.NodePath => v.AsNodePath(),
        Variant.Type.Rid => v.AsRid(),
        Variant.Type.Object => v.AsGodotObject(),
        Variant.Type.Callable => v.AsCallable(),
        Variant.Type.Signal => v.AsSignal(),
        Variant.Type.Dictionary => v.AsGodotDictionary(),
        Variant.Type.Array => v.AsGodotArray(),
        Variant.Type.PackedByteArray => v.AsByteArray(),
        Variant.Type.PackedInt32Array => v.AsInt32Array(),
        Variant.Type.PackedInt64Array => v.AsInt64Array(),
        Variant.Type.PackedFloat32Array => v.AsFloat32Array(),
        Variant.Type.PackedFloat64Array => v.AsFloat64Array(),
        Variant.Type.PackedStringArray => v.AsStringArray(),
        Variant.Type.PackedVector2Array => v.AsVector2Array(),
        Variant.Type.PackedVector3Array => v.AsVector3Array(),
        Variant.Type.PackedColorArray => v.AsColorArray(),
        Variant.Type.Max => throw new NotImplementedException(),
        _ => throw new ArgumentOutOfRangeException(nameof(v))
    };
}
