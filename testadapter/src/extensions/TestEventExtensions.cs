namespace GdUnit4.TestAdapter.Extensions;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

internal static class TestEventExtensions
{
    public static TestOutcome AsTestOutcome(this TestEvent e)
    {
        if (e.IsFailed || e.IsError)
            return TestOutcome.Failed;
        if (e.IsSkipped)
            return TestOutcome.Skipped;
        return TestOutcome.Passed;
    }
}
