using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GdUnit4
{
    internal sealed class Comparable
    {

        public class Result
        {
            public static Result Equal => new Result(true, null, null);

            public Result(bool valid, object? left, object? right, Result? parent = null)
            {
                Valid = valid;
                Left = left;
                Right = right;
                Parent = parent;
            }

            public Result WithProperty(string propertyName)
            {
                PropertyName = propertyName;
                return this;
            }

            private object? Left { get; set; }

            private object? Right { get; set; }

            private string? PropertyName { get; set; }

            private Result? Parent
            { get; set; }

            public bool Valid
            { get; private set; }
        }

        public static Result IsEqual<T>(T? left, T? right, GodotObjectExtensions.MODE compareMode = GodotObjectExtensions.MODE.CASE_SENSITIVE, Result? r = null)
        {
            return new Result(left.VariantEquals(right, compareMode), left, right, r);
        }
    }
}
