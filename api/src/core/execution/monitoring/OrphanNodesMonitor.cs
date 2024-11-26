namespace GdUnit4.Core.Execution.Monitoring;

using static Godot.Performance;

internal class OrphanNodesMonitor
{
    public int OrphanCount { get; private set; }

    private int OrphanNodesStart { get; set; }

    public void Start(bool reset = false)
    {
        if (reset)
            Reset();
        OrphanNodesStart = GetMonitoredOrphanCount();
    }

    public void Stop() => OrphanCount += GetMonitoredOrphanCount() - OrphanNodesStart;

    private int GetMonitoredOrphanCount() => (int)GetMonitor(Monitor.ObjectOrphanNodeCount);

    private void Reset() => OrphanCount = 0;
}
