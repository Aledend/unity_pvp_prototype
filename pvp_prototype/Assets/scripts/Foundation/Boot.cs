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
        Managers.network = new NetworkManager();
        Managers.asset = new AssetManager();
        await Managers.asset.Init();

        Managers.unitSpawner = new UnitSpawner();
        Managers.player = new PlayerManager();

        Managers.system = new SystemManager();
        Managers.system.RegisterSystem(new MoverSystem());
        Managers.system.RegisterSystem(new ControllerSystem());
        Managers.system.RegisterSystem(new ProjectileSystem());

        ready = true;

        foreach(var monoUnit in preInitHooks) {
            monoUnit.Init();
        }
        preInitHooks.Clear();

        Unit playerUnit = Managers.player.SpawnPlayer(true);
        var itemWielderExtension = ExtensionHandler<ItemWielderExtension, ItemWielderData>.StaticInstance(playerUnit);
        itemWielderExtension.AddSlot(playerUnit, Managers.asset.itemDatabase.sword);
    }

    public void Update() {
        if(!ready) { return; }

        // Update
        Managers.system.Update();
        
        // Post update
        Managers.unitSpawner.PostUpdate();
    }
}