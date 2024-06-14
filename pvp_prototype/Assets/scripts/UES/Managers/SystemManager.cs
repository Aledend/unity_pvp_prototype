using System.Collections.Generic;
public class SystemManager
{
    private static class SystemIndex<T> {
        public static int Idx {get; private set;}
        public static void Set(int i) => Idx = i;
    }

    public List<IExtensionSystem> systems = new();

    public void RegisterSystem<T>(T system) where T : IExtensionSystem {
        systems.Add(system);
        SystemIndex<T>.Set(systems.Count-1);
    }

    public T GetSystem<T>() where T : class, IExtensionSystem {
        return systems[SystemIndex<T>.Idx] as T;
    }

    public void Update() {
        foreach(var system in systems) {
            system.Update();
        }
    }
}
