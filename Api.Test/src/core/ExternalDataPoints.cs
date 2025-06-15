namespace GdUnit4.Tests.Core;

using System.Collections.Generic;

public static class ExternalDataPoints
{
    public static IEnumerable<object[]> PublicArrayDataPointProperty => [[1, 2, 3], [4, 5, 9]];
    private static IEnumerable<object[]> PrivateArrayDataPointProperty => [[1, 2, 3], [4, 5, 9]];

    public static IEnumerable<object[]> PublicArrayDataPointMethod() => [[1, 2, 3], [4, 5, 9]];
#pragma warning disable CA1859 // #warning directive
    private static IEnumerable<object[]> PrivateArrayDataPointMethod() => [[1, 2, 3], [4, 5, 9]];
#pragma warning restore CA1859 // #warning directive
}
