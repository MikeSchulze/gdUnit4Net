namespace GdUnit4.Asserts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public sealed class ValueExtractor : IValueExtractor
{
    private readonly IEnumerable<string> methodNames;

    private readonly IEnumerable<object> args;

    public ValueExtractor(string methodName, params object[] args)
    {
        methodNames = methodName.Split('.');
        this.args = args.ToList();
    }

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
                Console.WriteLine($"Can't ExtractValue {methodName}:{value}\n {e.StackTrace}");
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
            if (args.Any() && instance is Godot.GodotObject go)
                return go.Callv(method.Name.ToSnakeCase(), args.ToGodotArray()).UnboxVariant();
            return method.Invoke(instance, args.ToArray());
        }
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (property == null)
        {
            //if GdUnitSettings.is_verbose_assert_warnings():
            //    Godot.GD.PushWarning("Extracting value from element '%s' by func '%s' failed! Converting to \"n.a.\"" % [instance, func_name])
            return "n.a.";
        }
        return property.GetValue(instance);
    }
}
