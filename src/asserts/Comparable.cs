using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace GdUnit4
{
    internal sealed class Comparable
    {
        public enum MODE
        {
            CASE_SENSITIVE,
            CASE_INSENSITIVE
        }

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

        public static Result IsEqual<T>(T? left, T? right, MODE compareMode = MODE.CASE_SENSITIVE, Result? r = null)
        {
            if (compareMode == MODE.CASE_INSENSITIVE
                && left is String ls
                && right is String rs)
                return new Result(ls.ToLower().Equals(rs.ToLower()), left, right, r);

            return new Result(left.VariantEquals(right), left, right, r);
        }
    }
}
