namespace GdUnit4.Core.Events;

using System;

public interface ITestEventListener : IDisposable
{
    bool IsFailed { get; protected set; }

    int CompletedTests { get; protected set; }

    internal void PublishEvent(TestEvent testEvent);
}
