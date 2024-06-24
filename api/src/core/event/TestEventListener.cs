namespace GdUnit4;

using System;

internal interface ITestEventListener : IDisposable
{
    bool IsFailed { get; protected set; }

    void PublishEvent(TestEvent testEvent);
}
