// Copyright (c) 2025 Mike Schulze
// MIT License - See LICENSE file in the repository root for full license text

namespace GdUnit4.Asserts;

internal static class CompareExtensions
{
    internal static bool IsEquals<T>(this T? c, T? e) => Comparable.IsEqual(c, e).Valid;

    internal static bool IsSame<T>(this T? c, T? e) => AssertBase<T>.IsSame(c, e);
}
