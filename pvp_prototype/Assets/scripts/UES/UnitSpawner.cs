using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner
{
    private readonly Dictionary<Unit, List<ExtensionDataReference>> unitExtensions = new();
    private readonly List<Unit> unitsMarkedForDeletion = new();
    public Unit SpawnUnit(GameObject gameObject = null, Span<ExtensionGroup> extensionGroups = default) {
        if(!gameObject) gameObject = new GameObject();
        return SpawnUnit(gameObject, gameObject.transform.position, gameObject.transform.rotation, extensionGroups);
    }

    public Unit SpawnUnit(GameObject gameObject, Vector2 position, Quaternion rotation, Span<ExtensionGroup> extensionGroups = default) {
        gameObject.transform.SetPositionAndRotation(position, rotation);

        Unit unit = Unit.CreateFromGameObject(gameObject);
        var extensionList = new List<ExtensionDataReference>();
        unitExtensions[unit] = extensionList;

        foreach(var group in extensionGroups) {
            ExtensionInitiator.Initiators[group](unit, extensionList);
        }

        return unit;
    }

    public void PostUpdate() {
        foreach(var unit in unitsMarkedForDeletion) {
            var extensionList = unitExtensions[unit];
            foreach(var extensionDataReference in extensionList) {
                extensionDataReference.removeHandle(unit);
            }
            unitExtensions.Remove(unit);
            UnityEngine.Object.Destroy(unit.gameObject);
        }
        unitsMarkedForDeletion.Clear();
    }

    public void MarkForDeletion(Unit unit) {
        unitsMarkedForDeletion.Add(unit);
    }
}