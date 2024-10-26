namespace GdUnit4.Core.Execution.Monitoring;

using static Godot.Performance;

internal class OrphanNodesMonitor
{
    public OrphanNodesMonitor(bool reportOrphanNodesEnabled)
        => ReportOrphanNodesEnabled = reportOrphanNodesEnabled;

    private bool ReportOrphanNodesEnabled { get; }

    public int OrphanCount { get; private set; }

    private int OrphanNodesStart { get; set; }


    public void Start(bool reset = false)
    {
        if (ReportOrphanNodesEnabled)
        {
            if (reset)
                Reset();
            OrphanNodesStart = GetMonitoredOrphanCount();
        }
    }

    public void Stop()
    {
        if (ReportOrphanNodesEnabled)
            OrphanCount += GetMonitoredOrphanCount() - OrphanNodesStart;
    }

    private int GetMonitoredOrphanCount() => (int)GetMonitor(Monitor.ObjectOrphanNodeCount);

    public void Reset() => OrphanCount = 0;
}
