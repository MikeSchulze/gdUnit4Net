namespace GdUnit4.Executions.Monitors;

using System.Threading;
using System.Collections.Generic;

public class MemoryPool
{
    private readonly List<Godot.GodotObject> registeredObjects = new();
    private static readonly ThreadLocal<MemoryPool?> CurrentPool = new();

    public string Name { get; private set; } = "Unknown";

    public void SetActive(string name)
    {
        Name = name;
        CurrentPool.Value = this;
    }

    public static T? RegisterForAutoFree<T>(T? obj) where T : Godot.GodotObject
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

    private void FreeInstance(Godot.GodotObject obj)
    {
        // needs to manually exclude JavaClass see https://github.com/godotengine/godot/issues/44932
        if (Godot.GodotObject.IsInstanceValid(obj) && obj is not Godot.JavaClass)
        {
            if (obj is Godot.RefCounted)
                obj.Notification((int)Godot.GodotObject.NotificationPredelete);
            else
                obj.Free();
        }
    }
}
