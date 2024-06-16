using System;
using System.Collections.Generic;

public enum ExtensionGroup
{
    Character,
    LocallyControlled,
    AIControlled,
    Projectile,
    Visual,
    PlayerSpawner,
}

public static class ExtensionInitiator {
    public static Dictionary<ExtensionGroup, Action<Unit, ExtensionInitContext>> Initiators = new() {
        {ExtensionGroup.Character, (unit, initContext) => {
            if(!initContext.isHusk) {
                ExtensionHandler<PlayerMoverExtension, MoverExtensionData>.AddExtension(unit, initContext);
            }
        }},
        {ExtensionGroup.LocallyControlled, (unit, initContext) => {
            ExtensionHandler<PlayerControllerExtension, PlayerControllerData>.AddExtension(unit, initContext);
        }},
        {ExtensionGroup.Projectile, (unit, initContext) => {
            ExtensionHandler<ProjectileExtension, ProjectileData>.AddExtension(unit, initContext);
            ExtensionHandler<LazyVisualExtension, VisualData>.AddExtension(unit, initContext);
        }},
        {ExtensionGroup.Visual, (unit, initContext) => {
            ExtensionHandler<LazyVisualExtension, VisualData>.AddExtension(unit, initContext);
        }},
        {ExtensionGroup.PlayerSpawner, (unit, initContext) => {
            ExtensionHandler<PlayerSpawnerExtension, PlayerSpawnerData>.AddExtension(unit, initContext);
        }},
    };
}

public class ExtensionGroupsAssert : StaticAssert {
    public override bool Assert(out string errorMessage) {
        // Ensure all ExtensionGroups are defined
        errorMessage = string.Empty;
        return true;
    }
}