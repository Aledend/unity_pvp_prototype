using UnityEngine;

public class PlayerManager
{
    public Unit SpawnPlayer(bool locallyControlled) {
        Vector2 position = Vector2.zero; Quaternion rotation = Quaternion.identity;
        var spawners = ExtensionHandler<PlayerSpawnerExtension, PlayerSpawnerData>.References;
        if(spawners.Length > 0) {
            var spawner = spawners[Random.Range(0, spawners.Length)];
            position = spawner.gameObject.transform.position;
            rotation = spawner.gameObject.transform.rotation;
        }

        var template = Managers.asset.spawnTemplates.playerCharacter;
        
        Unit unit = Managers.unitSpawner.SpawnUnit(template, position, rotation);

        if(locallyControlled) {
            Managers.unitSpawner.AddExtension(unit, ExtensionGroup.LocallyControlled);
        }

        return unit;
    }
}
