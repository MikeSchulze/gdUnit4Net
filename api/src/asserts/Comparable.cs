namespace GdUnit4.Asserts;

internal static class Comparable
{
    public static Result IsEqual<T>(T? left, T? right, GodotObjectExtensions.MODE compareMode = GodotObjectExtensions.MODE.CaseSensitive, Result? r = null)
        => new(left.VariantEquals(right, compareMode), left, right, r);

    public class Result
    {
        public Result(bool valid, object? left, object? right, Result? parent = null)
        {
            Valid = valid;
            Left = left;
            Right = right;
            Parent = parent;
        }

        public static Result Equal => new(true, null, null);

        public bool Valid
        {
            get;
            private set;
        }

        public Result WithProperty(string propertyName)
        {
            PropertyName = propertyName;
            return this;
        }

#pragma warning disable IDE0052 // Remove unread private members
        // ReSharper disable all UnusedAutoPropertyAccessor.Local
        private object? Left { get; set; }

        private object? Right { get; set; }

        private string? PropertyName { get; set; }
        // ReSharper enable all UnusedAutoPropertyAccessor.Local

        private Result? Parent
        {
            get;
            set;
        }
#pragma warning restore IDE0052 // Remove unread private members
    }
}
