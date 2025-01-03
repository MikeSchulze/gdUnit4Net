namespace GdUnit4.Analyzers.Test;

using System.Globalization;

using Gu.Roslyn.Asserts;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static TestSourceBuilder;

[TestClass]
public class GodotNativeCallAnalyzerTest
{
    private readonly GodotNativeCallAnalyzer analyzer = new();

    [TestMethod]
    public void DetectErrorOnMethodExpressionUsingGodotReference()
    {
        var source = Instrument(
            """

            [TestCase]
            public void TestMethod()
            {
                var x = new RefCounted();
            }
            """);

        var errorLine = new LinePosition(12, 12);
        var expected = ExpectedDiagnostic
            .Create(DiagnosticRules.RuleIds.GodotEngineDiagnosticId,
                string.Format(CultureInfo.InvariantCulture,
                    DiagnosticRules.GodotEngine.GodotNativeCallNotAllowed.MessageFormat.ToString(), "TestMethod")
            )
            .WithPosition(new FileLinePositionSpan("TestClass.cs", errorLine, errorLine));

        RoslynAssert.Diagnostics(analyzer, expected, source);
    }


    [TestMethod]
    public void DetectErrorOnMethodExpressionBodyUsingGodotReference()
    {
        var source = Instrument(
            """

            [TestCase]
            public void TestMethod() => new RefCounted();
            """);

        var errorLine = new LinePosition(12, 12);
        var expected = ExpectedDiagnostic
            .Create(DiagnosticRules.RuleIds.GodotEngineDiagnosticId,
                string.Format(CultureInfo.InvariantCulture,
                    DiagnosticRules.GodotEngine.GodotNativeCallNotAllowed.MessageFormat.ToString(), "TestMethod")
            )
            .WithPosition(new FileLinePositionSpan("TestClass.cs", errorLine, errorLine));

        RoslynAssert.Diagnostics(analyzer, expected, source);
    }

    [TestMethod]
    public void OnMethodExpressionWithGodotTestCaseAttribute()
    {
        var source = Instrument(
            """

            [GodotTestCase]
            public void TestMethod()
            {
                var x = new RefCounted();
            }
            """);

        RoslynAssert.Valid(analyzer, source);
    }

    [TestMethod]
    public void OnMethodExpressionBodyWithGodotTestCaseAttribute()
    {
        var source = Instrument(
            """

            [GodotTestCase]
            public void TestMethod() => new RefCounted();
            """);

        RoslynAssert.Valid(analyzer, source);
    }
}
