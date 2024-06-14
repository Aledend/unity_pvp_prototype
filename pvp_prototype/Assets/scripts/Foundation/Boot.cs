using System.Collections.Generic;
using System.Threading.Tasks;

public class Boot
{
    private List<MonoUnit> preInitHooks = new();
    public void HookInit(MonoUnit unit) {
        if(ready) {
            unit.Init();
        } else {
            preInitHooks.Add(unit);
        }
    }

    private bool ready = false;
    public async void Init(){
        Managers.asset = new AssetManager();
        await Managers.asset.Init();

        Managers.unitSpawner = new UnitSpawner();

        Managers.system = new SystemManager();
        Managers.system.RegisterSystem(new MoverSystem());
        Managers.system.RegisterSystem(new ControllerSystem());
        Managers.system.RegisterSystem(new ProjectileSystem());

        ready = true;

        foreach(var monoUnit in preInitHooks) {
            monoUnit.Init();
        }
        preInitHooks.Clear();
    }

    public void Update() {
        if(!ready) { return; }

        // Update
        Managers.system.Update();
        
        // Post update
        Managers.unitSpawner.PostUpdate();
    }
}