using System.Threading;
using System.Collections.Generic;

namespace GdUnit4.Executions.Monitors
{
    public class MemoryPool
    {
        private List<Godot.GodotObject> _registeredObjects = new List<Godot.GodotObject>();

        public void SetActive(string name)
        {
            //Godot.GD.PrintS("MemoryPool.SetActive", name, GetHashCode());
            Thread.SetData(Thread.GetNamedDataSlot("MemoryPool"), this);
        }

        public static T RegisterForAutoFree<T>(T? obj) where T : Godot.GodotObject
        {
            MemoryPool pool = (MemoryPool)Thread.GetData(Thread.GetNamedDataSlot("MemoryPool"));
            if (obj != null)
                pool._registeredObjects.Add(obj);
            //Godot.GD.PrintS("MemoryPool.RegisterForAutoFree", pool._name, pool.GetHashCode(), "register", obj);
            return obj!;
        }

        public void ReleaseRegisteredObjects()
        {
            //Godot.GD.PrintS("MemoryPool.ReleaseRegisteredObjects", _name, GetHashCode());
            _registeredObjects.ForEach(FreeInstance);
            _registeredObjects.Clear();
        }

        private void FreeInstance(Godot.GodotObject obj)
        {
            // needs to manually exculde JavaClass see https://github.com/godotengine/godot/issues/44932
            if (Godot.GodotObject.IsInstanceValid(obj) && !(obj is Godot.JavaClass))
            {
                if (obj is Godot.RefCounted)
                {
                    //Godot.GD.PrintS("Freeing RefCounted", obj);
                    obj.Notification((int)Godot.GodotObject.NotificationPredelete);
                }
                else
                {
                    //Godot.GD.PrintS("Freeing Object", obj);
                    obj.Free();
                }
            }
        }
    }
}
