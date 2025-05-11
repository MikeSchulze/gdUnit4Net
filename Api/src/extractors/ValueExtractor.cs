// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Extractors;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using Asserts;

using Core.Extensions;

using Godot;

internal sealed class ValueExtractor : IValueExtractor
{
    private readonly IEnumerable<object> args;
    private readonly IEnumerable<string> methodNames;

    public ValueExtractor(string methodName, params object[] args)
    {
        methodNames = methodName.Split('.');
        this.args = args.ToList();
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Method purpose is to capture and handle value as 'n.a.' if it fails.")]
    public object? ExtractValue(object? value)
    {
        if (value == null)
            return null;

        foreach (var methodName in methodNames)
        {
            try
            {
                value = Extract(value, methodName).UnboxVariant();
                if (value == null || value.Equals("n.a."))
                    return value;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Warning: Can't ExtractValue {methodName}:nameof({value})\n {e.StackTrace}");
                return "n.a.";
            }
        }

        return value;
    }

    private object? Extract(object instance, string name)
    {
        var type = instance.GetType();
        var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method != null)
        {
            if (args.Any() && instance is GodotObject go)
                return go.Callv(GdUnitExtensions.ToSnakeCase(method.Name), args.ToGodotArray()).UnboxVariant();
            return method.Invoke(instance, args.ToArray());
        }

        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null)

            // if GdUnitSettings.is_verbose_assert_warnings():
            //    Godot.GD.PushWarning("Extracting value from element '%s' by func '%s' failed! Converting to \"n.a.\"" % [instance, func_name])
            return "n.a.";
        return property.GetValue(instance);
    }
}
