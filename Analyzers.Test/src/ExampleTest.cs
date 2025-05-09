namespace GdUnit4.Analyzers.Test;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
[SuppressMessage("Style", "IDE0060", Justification = "Required for DynamicData test method pattern")]
public class ExampleTest
{
    [TestMethod]
    public void SingeTest()
    {
    }

    [TestMethod("Test with display name")]
    public void SingeTestNamed()
    {
    }

    [TestMethod]
    [DataRow(1, 2, DisplayName = "DataRow1")]
    [DataRow(2, 2)]
    [DataRow(3, 2)]
    public void DataRowTest(int a, int b)
    {
    }

    [TestMethod]
    [DynamicData(nameof(TestDataProvider.GetTestData), typeof(TestDataProvider), DynamicDataSourceType.Method)]
    public void TestWithDynamicData(int a, int b, int expected)
    {
    }

    [TestMethod]
    [DynamicData(nameof(TestDataProvider.GetTestData), typeof(TestDataProvider), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(GetDisplayName))]
    public void TestWithDisplayName(int a, int b, int expected)
    {
    }

    [TestMethod]
    [DynamicData(nameof(TestDataProvider.GetTestData), typeof(TestDataProvider), DynamicDataSourceType.Method)]
    public void TestWithDisplayName2(int a, int b, int expected)
    {
    }

    // Display name provider method
    public static string GetDisplayName(MethodInfo methodInfo, object[] data)
        => $"{methodInfo.Name} with {data[0]} + {data[1]} = {data[2]}";

    private sealed class TestDataProvider
    {
        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] { 1, 2, 3 };
            yield return new object[] { 5, 5, 10 };
            yield return new object[] { -1, 1, 0 };
        }
    }
}
