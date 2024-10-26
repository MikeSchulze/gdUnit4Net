namespace GdUnit4.Core.Execution.Monitoring;

using System.Collections.Generic;
using System.Threading;

using Godot;

internal class MemoryPool
{
    private static readonly ThreadLocal<MemoryPool?> CurrentPool = new();
    private readonly List<GodotObject> registeredObjects = new();

    public string Name { get; set; } = "Unknown";

    public void SetActive(string name)
    {
        Name = name;
        CurrentPool.Value = this;
    }

    public static T? RegisterForAutoFree<T>(T? obj) where T : GodotObject
    {
        if (obj != null)
            CurrentPool.Value?.registeredObjects.Add(obj);
        return obj;
    }

    public void ReleaseRegisteredObjects()
    {
        var currentPool = CurrentPool.Value;
        currentPool?.registeredObjects.ForEach(FreeInstance);
        currentPool?.registeredObjects.Clear();
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
}
