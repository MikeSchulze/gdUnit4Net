using System;
using Newtonsoft.Json;


namespace GdUnit4.Api;

class TestAdapterReporter : ITestEventListener
{
    public bool IsFailed { get; set; } = false;

    public void PublishEvent(TestEvent e)
    {
        if (e.IsFailed || e.IsError)
            IsFailed = true;
        string json = JsonConvert.SerializeObject(e);
        Console.WriteLine($"GdUnitTestEvent:{json}");
    }
}
