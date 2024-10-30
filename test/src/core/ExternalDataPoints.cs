namespace GdUnit4.Tests.Core;

using System.Collections.Generic;

public static class ExternalDataPoints
{
    public static IEnumerable<object[]> PublicArrayDataPointProperty => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
    private static IEnumerable<object[]> PrivateArrayDataPointProperty => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };

    public static IEnumerable<object[]> PublicArrayDataPointMethod() => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
#pragma warning disable CA1859 // #warning directive
    private static IEnumerable<object[]> PrivateArrayDataPointMethod() => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
#pragma warning restore CA1859 // #warning directive
}
