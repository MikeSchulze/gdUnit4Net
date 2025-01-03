namespace GdUnit4.Analyzers.Test;

using System;
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
    public void ShowErrorOnMethodUsingGodotReference()
    {
        var source = Instrument(
            """

            [TestCase]
            public void TestMethod()
            {
                var x = new RefCounted();
            }
            """);

        Console.WriteLine(source);

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
    public void OnGodotTestCaseAttribute()
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
}
