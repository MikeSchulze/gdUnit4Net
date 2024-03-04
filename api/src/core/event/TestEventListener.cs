
namespace GdUnit4;

internal interface ITestEventListener
{
    bool IsFailed { get; protected set; }

    void PublishEvent(TestEvent testEvent);
}
