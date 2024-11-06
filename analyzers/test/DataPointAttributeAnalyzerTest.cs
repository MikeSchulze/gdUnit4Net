namespace GdUnit4.Analyzers.Test;

using System.Globalization;

using Gu.Roslyn.Asserts;

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DataPointAttributeAnalyzerTests
{
    private readonly DiagnosticAnalyzer analyzer = new DataPointAttributeAnalyzer();

    [TestMethod]
    public void SingleTestCaseWithDataPointNoError()
    {
        var source = TestCaseLoader.InstrumentTestCases(
            """
            [TestSuite]
            public class TestClass
            {
              public static object[] TestData => new object[] { new object[] { 1, 2, 3 } };

              [TestCase]
              [DataPoint(nameof(TestData))]
              public void TestMethod(int a, int b, int expected)
              {
              }
            }
            """);

        RoslynAssert.Valid(analyzer, source);
    }

    [TestMethod]
    public void MultipleTestCaseWithDataPointReportsError()
    {
        var source = TestCaseLoader.InstrumentTestCases(
            """
            [TestSuite]
            public class TestClass
            {
                public static object[] TestData => new object[] { new object[] { 1, 2, 3 } };

                [TestCase]
                [TestCase]
                [DataPoint(nameof(TestData))]
                public void TestMethod(int a, int b, int expected)
                {
                }
            }
            """);

        var expectedDiagnostic = ExpectedDiagnostic.Create(
            DiagnosticRules.RuleIds.DataPointWithMultipleTestCase,
            string.Format(CultureInfo.InvariantCulture,
                DiagnosticRules.DataPoint.MultipleTestCaseAttributes.MessageFormat.ToString(),
                "TestMethod"));

        RoslynAssert.Diagnostics(analyzer, expectedDiagnostic, source);
    }

    [TestMethod]
    public void NoDataPointMultipleTestCaseNoError()
    {
        var source = TestCaseLoader.InstrumentTestCases(
            """
            [TestSuite]
            public class TestClass
            {
                [TestCase]
                [TestCase]
                public void TestMethod(int a, int b)
                {
                }
            }
            """);

        RoslynAssert.Valid(analyzer, source);
    }
}
