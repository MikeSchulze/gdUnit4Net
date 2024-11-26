namespace GdUnit4.Core.Execution.Monitoring;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Extensions;

using Godot;

internal class MemoryPool
{
    private static readonly ThreadLocal<MemoryPool?> CurrentPool = new();
    private readonly List<GodotObject> registeredObjects = new();

    public MemoryPool(bool reportOrphanNodesEnabled) => OrphanMonitor = reportOrphanNodesEnabled ? new OrphanNodesMonitor() : null;

    private OrphanNodesMonitor? OrphanMonitor
    {
        get;
    }

    public int OrphanCount => OrphanMonitor?.OrphanCount ?? 0;

    public string Name { get; set; } = "Unknown";

    public void SetActive(string name, bool reset = false)
    {
        Name = name;
        CurrentPool.Value = this;
        OrphanMonitor?.Start(reset);
    }

    public static T? RegisterForAutoFree<T>(T? obj) where T : GodotObject
    {
        if (obj != null)
            CurrentPool.Value?.registeredObjects.Add(obj);
        return obj;
    }

    public async Task Gc()
    {
        var currentPool = CurrentPool.Value;
        currentPool?.registeredObjects.ForEach(FreeInstance);
        currentPool?.registeredObjects.Clear();
        StopMonitoring();
        if (OrphanMonitor != null)
            await GodotObjectExtensions.SyncProcessFrame;
    }

    private void FreeInstance(GodotObject obj)
    {
        // needs to manually exclude JavaClass see https://github.com/godotengine/godot/issues/44932
        if (GodotObject.IsInstanceValid(obj) && obj is not JavaClass)
        {
            if (obj is RefCounted)
                obj.Notification((int)GodotObject.NotificationPredelete);
            else
                obj.Free();
        }
    }

    public void StopMonitoring() => OrphanMonitor?.Stop();
}
