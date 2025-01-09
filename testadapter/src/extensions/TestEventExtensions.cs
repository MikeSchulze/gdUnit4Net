namespace GdUnit4.TestAdapter.Extensions;

using Core.Events;

using Microsoft.VisualStudio.TestPlatform.ObjectModel;

internal static class TestEventExtensions
{
    public static TestOutcome AsTestOutcome(this TestEvent e)
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
