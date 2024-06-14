using System;
using System.Collections.Generic;

public enum ExtensionGroup
{
    Character,
    HumanControlled,
    AIControlled,
    Projectile,
}

public static class ExtensionInitiator {
    public static Dictionary<ExtensionGroup, Action<Unit, List<ExtensionDataReference>>> Initiators = new() {
        {ExtensionGroup.Character, (unit, extensionList) => {
            extensionList.Add(DerivedExtensionHandler<PlayerMoverExtension, MoverExtensionData, MoverExtension>.AddExtension(unit));
        }},
        {ExtensionGroup.HumanControlled, (unit, extensionList) => {
            extensionList.Add(ExtensionHandler<PlayerControllerExtension, PlayerControllerData>.AddExtension(unit));
        }},
        {ExtensionGroup.Projectile, (unit, extensionList) => {
            extensionList.Add(ExtensionHandler<ProjectileExtension, ProjectileData>.AddExtension(unit));
            extensionList.Add(ExtensionHandler<LazyVisualExtension, LazyVisualData>.AddExtension(unit));
        }}
    };
}

public class ExtensionGroupsAssert : StaticAssert {
    public override bool Assert(out string errorMessage) {
        // Ensure all ExtensionGroups are defined
        errorMessage = string.Empty;
        return true;
    }
}