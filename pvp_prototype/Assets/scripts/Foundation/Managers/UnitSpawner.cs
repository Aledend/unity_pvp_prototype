using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner
{
    private ExtensionInitContext extensionInitContext = new();
    private readonly List<Unit> unitsMarkedForDeletion = new();
    public Unit SpawnUnit(Span<ExtensionGroup> extensionGroups = default) {
        return SpawnUnit(new GameObject(), extensionGroups);
    }

    public Unit SpawnUnit(GameObject gameObject, Span<ExtensionGroup> extensionGroups = default) {
        Unit unit = new(gameObject);
        AddExtensions(unit, extensionGroups);
        return unit;
    }

    public Unit SpawnUnit(Vector2 position, Quaternion rotation, Span<ExtensionGroup> extensionGroups = default) {
        Unit unit = SpawnUnit(extensionGroups);
        unit.GameObject().transform.SetPositionAndRotation(position, rotation);
        return unit;
    }

    public Unit SpawnUnit(SpawnTemplates.SpawnTemplateDefinition spawnTemplate) {
        return SpawnUnit(spawnTemplate, Vector2.zero, Quaternion.identity);
    }

    public Unit SpawnUnit(SpawnTemplates.SpawnTemplateDefinition spawnTemplate, Vector2 position, Quaternion rotation) {
        Unit unit = SpawnUnit(position, rotation, spawnTemplate.extensionGroups);
        if(spawnTemplate.visual.AssetGUID != string.Empty) {
            AddExtension(unit, ExtensionGroup.Visual);
            var lazyVisualExtension = ExtensionHandler<LazyVisualExtension, VisualData>.StaticInstance(unit);
            lazyVisualExtension.LoadVisual(unit, spawnTemplate.visual);
        }
        return unit;
    }

    public void AddExtensions(Unit unit, Span<ExtensionGroup> extensionGroups) {
        // TODO(theodor.brandt:2024-06-14): this should just be one offs
        extensionInitContext.isHusk = false;
        extensionInitContext.isAuthor = true;

        foreach(var group in extensionGroups) {
            ExtensionInitiator.Initiators[group](unit, extensionInitContext);
        }
    }

    public void AddExtension(Unit unit, ExtensionGroup extensionGroup) {
        AddExtensions(unit, stackalloc ExtensionGroup[1] {extensionGroup});
    }

    public void PostUpdate() {
        foreach(var unit in unitsMarkedForDeletion) {
            ExtensionList.DestroyAllExtensions(unit);
            UnityEngine.Object.Destroy(unit.GameObject());
        }
        unitsMarkedForDeletion.Clear();
    }

    public void MarkForDeletion(Unit unit) {
        unitsMarkedForDeletion.Add(unit);
    }
}
