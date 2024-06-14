using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProjectileSystem : IExtensionSystem
{
    public void Update()
    {
        ExtensionHandler<ProjectileExtension, ProjectileData>.Update();
    }

    public Unit SpawnProjectile(AssetReferenceGameObject visual, Vector2 position, Vector2 direction, float speed, float maxDistance) {
        if(direction == Vector2.zero) {
            direction = Vector2.up;
        }
        
        Unit unit = Managers.unitSpawner.SpawnUnit(position, Utils2D.LookRotation(direction), stackalloc[] {ExtensionGroup.Projectile});
        ref var projectileData = ref ExtensionHandler<ProjectileExtension, ProjectileData>.GetData(unit);
        projectileData.speed = speed;
        projectileData.direction = direction;
        projectileData.maxDistance = maxDistance;

        var visualExtension = ExtensionHandler<LazyVisualExtension, LazyVisualData>.StaticInstance(unit);
        visualExtension.LoadVisual(unit, visual);
        return unit;
    }
}
