namespace GdUnit4;

internal sealed class Comparable
{

    public class Result
    {
        public static Result Equal => new(true, null, null);

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

#pragma warning disable IDE0052 // Remove unread private members
        private object? Left { get; set; }

        private object? Right { get; set; }

        private string? PropertyName { get; set; }

        private Result? Parent
        { get; set; }
#pragma warning restore IDE0052 // Remove unread private members

        public bool Valid
        { get; private set; }
    }

    public static Result IsEqual<T>(T? left, T? right, GodotObjectExtensions.MODE compareMode = GodotObjectExtensions.MODE.CASE_SENSITIVE, Result? r = null)
        => new(left.VariantEquals(right, compareMode), left, right, r);
}
