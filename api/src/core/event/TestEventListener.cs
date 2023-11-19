
namespace GdUnit4
{
    public interface ITestEventListener
    {
        bool IsFailed { get; protected set; }

        void PublishEvent(TestEvent testEvent);
    }
}
