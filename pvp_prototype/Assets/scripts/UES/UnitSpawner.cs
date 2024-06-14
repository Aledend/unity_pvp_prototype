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
        Unit unit = Unit.CreateFromGameObject(gameObject);
        AddExtensions(unit, extensionGroups);
        return unit;
    }

    public Unit SpawnUnit(Vector2 position, Quaternion rotation, Span<ExtensionGroup> extensionGroups = default) {
        Unit unit = SpawnUnit(extensionGroups);
        unit.gameObject.transform.SetPositionAndRotation(position, rotation);
        return unit;
    }

    public Unit SpawnUnit(SpawnTemplates.SpawnTemplateDefinition spawnTemplate) {
        return SpawnUnit(spawnTemplate, Vector2.zero, Quaternion.identity);
    }

    public Unit SpawnUnit(SpawnTemplates.SpawnTemplateDefinition spawnTemplate, Vector2 position, Quaternion rotation) {
        Unit unit = SpawnUnit(position, rotation, spawnTemplate.extensionGroups);
        if(spawnTemplate.visual.AssetGUID != string.Empty) {
            AddExtension(unit, ExtensionGroup.Visual);
            var lazyVisualExtension = ExtensionHandler<LazyVisualExtension, LazyVisualData>.StaticInstance(unit);
            lazyVisualExtension.LoadVisual(unit, spawnTemplate.visual);
        }
        return unit;
    }

    public void AddExtensions(Unit unit, Span<ExtensionGroup> extensionGroups) {
        foreach(var group in extensionGroups) {
            AddExtension(unit, group);
        }
    }

    public void AddExtension(Unit unit, ExtensionGroup extensionGroup) {
        // TODO(theodor.brandt:2024-06-14): this should just be one offs
        extensionInitContext.isHusk = false;
        extensionInitContext.isAuthor = true;

        ExtensionInitiator.Initiators[extensionGroup](unit, extensionInitContext);
    }

    public void PostUpdate() {
        foreach(var unit in unitsMarkedForDeletion) {
            ExtensionList.DestroyAllExtensions(unit);
            UnityEngine.Object.Destroy(unit.gameObject);
        }
        unitsMarkedForDeletion.Clear();
    }

    public void MarkForDeletion(Unit unit) {
        unitsMarkedForDeletion.Add(unit);
    }
}
