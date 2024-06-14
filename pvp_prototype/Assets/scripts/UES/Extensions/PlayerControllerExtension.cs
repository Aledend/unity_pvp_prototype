using UnityEngine;

public struct PlayerControllerData {
    public Unit unit;
    public float speed;
}

public class PlayerControllerExtension : IExtension<PlayerControllerData>
{

    public void Init(Unit unit, ExtensionInitContext extensionInitContext, ref PlayerControllerData extensionData)
    {
        extensionData.unit = unit;
        extensionData.speed = 20;
    }

    public void Update(ref PlayerControllerData extensionData)
    {
        Unit unit = extensionData.unit;

        Vector2 move = new();
        if(Input.GetKey(KeyCode.A)) {
            --move.x;
        }
        if(Input.GetKey(KeyCode.D)) {
            ++move.x;
        }
        if(Input.GetKey(KeyCode.W)) {
            ++move.y;
        }
        if(Input.GetKey(KeyCode.S)) {
            --move.y;
        }

        move = move.normalized * extensionData.speed;

        ExtensionHandler<MoverExtension, MoverExtensionData>.StaticInstance(unit).Move(unit, move);

        if(Input.GetKeyDown(KeyCode.Space)) {
            Vector2 direction = Utils2D.LookDirection(unit.gameObject.transform.rotation);
            Managers.system.GetSystem<ProjectileSystem>().SpawnProjectile(Managers.asset.assetRegistry.bullet, unit.gameObject.transform.position, direction, 10, 5);
        }
    }
    public void Destroy(Unit unit) {}
}