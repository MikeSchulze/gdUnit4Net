using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GdUnit4.Asserts
{
    public sealed class ValueExtractor : IValueExtractor
    {
        private readonly IEnumerable<string> _methodNames;

        private readonly IEnumerable<object> _args;

        public ValueExtractor(string methodName, params object[] args)
        {
            _methodNames = methodName.Split('.');
            _args = args.ToList<object>();
        }

        public object? ExtractValue(object? value)
        {
            if (value == null)
                return null;

            foreach (var methodName in _methodNames)
            {
                try
                {
                    value = Extract(value, methodName);
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
                if (_args.Count() > 0 && instance is Godot.GodotObject go)
                    return go.Callv(method.Name.ToSnakeCase(), _args.ToGodotArray()).UnboxVariant();
                return method.Invoke(instance, _args.ToArray());
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
}
