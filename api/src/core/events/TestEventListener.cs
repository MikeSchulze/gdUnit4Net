using System;

namespace GdUnit4.Core.Events;

internal interface ITestEventListener : IDisposable
{
    bool IsFailed { get; protected set; }

    internal void PublishEvent(TestEvent testEvent);
}
