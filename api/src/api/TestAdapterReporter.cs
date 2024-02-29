namespace GdUnit4.Api;
using System;

using Newtonsoft.Json;

internal class TestAdapterReporter : ITestEventListener
{
    public bool IsFailed { get; set; }

    public void PublishEvent(TestEvent e)
    {
        if (e.IsFailed || e.IsError)
            IsFailed = true;
        var json = JsonConvert.SerializeObject(e);
        Console.WriteLine($"GdUnitTestEvent:{json}");
    }
}
