namespace GdUnit4.Asserts;

public interface IValueExtractor
{
    /// <summary>
    ///     Extracts a value by given implementation
    /// </summary>
    /// <param name="value">The object containing the value to be extracted</param>
    /// <returns></returns>
    public object? ExtractValue(object? value);
}
