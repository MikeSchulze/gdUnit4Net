namespace GdUnit4.Analyzers.Test;

public static class TestCaseLoader
{
    public static string InstrumentTestCases(string sourceCode) =>
        """
            using System;
            using System.Collections.Generic;
            using GdUnit4;

            namespace GdUnit4.Analyzers.Test.TestCaseRun
            {
                $sourceCode
            }
            """.Replace("$sourceCode", sourceCode);
}
