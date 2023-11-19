using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace GdUnit4.TestAdapter;

static class GdUnit4TestAdapterExtensions
{
    public static TestOutcome AsTestOutcome(this TestEvent e)
    {
        if (e.IsFailed)
            return TestOutcome.Failed;
        if (e.IsSkipped)
            return TestOutcome.Skipped;
        return TestOutcome.Passed;
    }
}
