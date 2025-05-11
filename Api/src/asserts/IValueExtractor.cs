// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

public interface IValueExtractor
{
    /// <summary>
    ///     Extracts a value by given implementation.
    /// </summary>
    /// <param name="value">The object containing the value to be extracted.</param>
    /// <returns></returns>
    public object? ExtractValue(object? value);
}
