namespace GdUnit4.Tests.Core;

using System.Collections.Generic;
using System.Threading.Tasks;

using static Assertions;

/// <summary>
///     The test are similar to https://github.com/microsoft/testfx/blob/cdc374674477f23502f58b5be503ed609ba4057c/docs/RFCs/006-DynamicData-Attribute.md
/// </summary>
[TestSuite]
public class TestSuiteWithDynamicDataPoints
{
    public static IEnumerable<object[]> ArrayDataPointProperty => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
    public static IEnumerable<int> SingleDataPointProperty => new[] { 1, 2, 3 };
    public static IEnumerable<object[]> ArrayDataPointMethod() => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
    public static IEnumerable<object[]> PublicTestDataFactory(int factor) => new[] { new object[] { 1 * factor, 1 * factor }, new object[] { 2 * factor, 2 * factor } };

#pragma warning disable CA1859 // #warning directive
    private static IEnumerable<object[]> PrivateArrayDataPointProperty => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
    private static IEnumerable<object[]> PrivateArrayDataPointMethod() => new[] { new object[] { 1, 2, 3 }, new object[] { 4, 5, 9 } };
    private static IEnumerable<object[]> PrivateTestDataFactory(int factor) => new[] { new object[] { 1 * factor, 1 * factor }, new object[] { 2 * factor, 2 * factor } };
#pragma warning restore CS1030 // #warning directive


    internal static IEnumerable<object[]> YieldedDataPointMethod()
    {
        yield return new object[] { 1, 2, 3 };
        yield return new object[] { 4, 5, 9 };
    }

    #region private_data_points

    [TestCase]
    [DataPoint(nameof(PrivateArrayDataPointProperty))]
    public void OnPrivateArrayDataPointProperty(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(PrivateArrayDataPointMethod))]
    public void OnPrivateArrayDataPointMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint("PrivateArrayDataPointProperty", typeof(ExternalDataPoints))]
    public void OnExternalPrivateArrayDataPointProperty(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint("PrivateArrayDataPointMethod", typeof(ExternalDataPoints))]
    public void OnExternalPrivateArrayDataPointMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(PrivateTestDataFactory), 2)]
    public void ParameterizedPrivateDataPoint(int value, int expected) => AssertThat(value).IsEqual(expected);

    #endregion

    #region public_data_points

    [TestCase]
    [DataPoint(nameof(ArrayDataPointProperty))]
    public void OnPublicArrayDataPointProperty(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(SingleDataPointProperty))]
    public void OnPublicSingleDataPointProperty(int value) => AssertThat(value).IsBetween(1, 3);

    [TestCase]
    [DataPoint(nameof(ArrayDataPointMethod))]
    public void OnPublicArrayDataPointMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(YieldedDataPointMethod))]
    public void DataPointYieldedMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(ExternalDataPoints.PublicArrayDataPointProperty), typeof(ExternalDataPoints))]
    public void OnExternalPublicArrayDataPointProperty(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(ExternalDataPoints.PublicArrayDataPointMethod), typeof(ExternalDataPoints))]
    public void OnExternalPublicArrayDataPointMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(PublicTestDataFactory), 2)]
    public void ParameterizedPublicDataPoint(int value, int expected) => AssertThat(value).IsEqual(expected);

    #endregion

    #region public_async_data_points

    public static async IAsyncEnumerable<object?[]> AsyncArrayDataPoint()
    {
        for (var i = 0; i < 3; i++)
        {
            // Simulate async work for each item
            await Task.Delay(10);
            yield return new object?[] { i, i + 1, i + i + 1 };
        }
    }

    public static async IAsyncEnumerable<object?[]> AsyncArrayDataPointBlocked()
    {
        await Task.Delay(500);
        yield return new object?[] { 1, 2, 3 };
    }

    public static async IAsyncEnumerable<int> AsyncSingleDataPoint()
    {
        yield return 1;
        await Task.Delay(10);
        yield return 2;
        await Task.Delay(10);
        yield return 3;
    }

    [TestCase]
    [DataPoint(nameof(AsyncArrayDataPoint))]
    public void OnAsyncArrayDataPoint(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(AsyncSingleDataPoint))]
    public void OnAsyncSingleDataPoint(int a) => AssertThat(a).IsBetween(1, 3);


    [TestCase(Timeout = 100)]
    [DataPoint(nameof(AsyncArrayDataPointBlocked))]
    public void OnAsyncArrayDataPointFailByTimeout(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    #endregion
}
