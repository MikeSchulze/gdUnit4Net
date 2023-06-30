using System.Threading;
using System.Collections.Generic;

namespace GdUnit4.Executions.Monitors
{
    public class MemoryPool
    {
        private List<Godot.GodotObject> _registeredObjects = new List<Godot.GodotObject>();
        private static readonly ThreadLocal<MemoryPool?> _currentPool = new ThreadLocal<MemoryPool?>();

        public string Name { get; private set; } = "Unknown";

        public void SetActive(string name)
        {
            Name = name;
            _currentPool.Value = this;
        }

        public static T? RegisterForAutoFree<T>(T? obj) where T : Godot.GodotObject
        {
            if (obj != null)
                _currentPool.Value?._registeredObjects.Add(obj);
            return obj;
        }

        public void ReleaseRegisteredObjects()
        {
            var currentPool = _currentPool.Value;
            currentPool?._registeredObjects.ForEach(FreeInstance);
            currentPool?._registeredObjects.Clear();
        }

        private void FreeInstance(Godot.GodotObject obj)
        {
            // needs to manually exculde JavaClass see https://github.com/godotengine/godot/issues/44932
            if (Godot.GodotObject.IsInstanceValid(obj) && !(obj is Godot.JavaClass))
            {
                if (obj is Godot.RefCounted)
                    obj.Notification((int)Godot.GodotObject.NotificationPredelete);
                else
                    obj.Free();
            }
        }
    }
}
