namespace Examples;

#if GDUNIT4NET_API_V5
using GdUnit4;

using static GdUnit4.Assertions;

[TestSuite]
public class ExampleTest
{
    public static IEnumerable<object[]> ArrayDataPointProperty => [[1, 2, 3], [4, 5, 9]];
    public static IEnumerable<object[]> ArrayDataPointMethod() => [[1, 2, 3], [4, 5, 9]];

    [TestCase]
    public void Success() => AssertBool(true).IsTrue();


    [TestCase]
    public void Failed() => AssertBool(false).IsTrue();

    [TestCase]
    [DataPoint(nameof(ArrayDataPointProperty))]
    public void WithDataPointProperty(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);

    [TestCase]
    [DataPoint(nameof(ArrayDataPointMethod))]
    public void WithArrayDataPointMethod(int a, int b, int expected) => AssertThat(a + b).IsEqual(expected);


    [TestCase(0, 1, 2, TestName = "TestA")]
    [TestCase(1, 2, 3, TestName = "TestB", Description = "foo ")]
    public void DataRows(int a, int b, int c) => AssertBool(true).IsTrue();
}
#endif
