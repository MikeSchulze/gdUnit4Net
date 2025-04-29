namespace GdUnit4.TestAdapter.Extensions;

using Api;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

internal static class TestEventExtensions
{
    public static TestOutcome AsTestOutcome(this ITestEvent e)
    {
        if (e.IsFailed || e.IsError)
            return TestOutcome.Failed;
        if (e.IsWarning)
            return TestOutcome.Passed;
        if (e.IsSkipped)
            return TestOutcome.Skipped;
        return TestOutcome.Passed;
    }
}
