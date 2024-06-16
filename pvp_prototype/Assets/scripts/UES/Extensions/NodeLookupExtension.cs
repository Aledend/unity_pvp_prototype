using System.Collections.Generic;
using UnityEngine;

public struct NodeLookupDefinition {
    public Transform transform;
    public bool isFallback;
}
public struct NodeLookupData {
    public Dictionary<NodeName, NodeLookupDefinition> cache;
}

public class NodeLookupExtension : Extension<NodeLookupData>
{
    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref NodeLookupData extensionData)
    {
        extensionData.cache = new();
    }

    public static Transform StaticFind(Unit unit, NodeName nodeName, bool allowFallback = true)
    {
        Managers.unitSpawner.AddExtension(unit, ExtensionGroup.NodeLookup);
        var extension = ExtensionHandler<NodeLookupExtension, NodeLookupData>.StaticInstance(unit);
        return extension.GetNode(unit, nodeName, allowFallback);
    }

    public Transform GetNode(Unit unit, NodeName nodeName, bool allowFallback = true) {
        ref var data = ref ExtensionHandler<NodeLookupExtension, NodeLookupData>.GetData(unit);
        if(!data.cache.TryGetValue(nodeName, out NodeLookupDefinition def)) {
            Transform matchingObject = unit.gameObject.transform.Find(NodeNameString.GetString(nodeName));
            if(matchingObject) {
                def = new NodeLookupDefinition() {
                    transform = matchingObject,
                    isFallback = false,
                };
                data.cache[nodeName] = def;
            } else if(FallbackNodeNames.TryGet(nodeName, out NodeName fallbackName)) {
                Transform fallbackObject = GetNode(unit, fallbackName);
                if(fallbackObject) {
                    def = new NodeLookupDefinition() {
                        transform = fallbackObject,
                        isFallback = true,
                    };
                    data.cache[nodeName] = def;
                }
            }

            if(!def.transform) {
                def.transform = unit.gameObject.transform;
                def.isFallback = true;
                data.cache[nodeName] = def;
            }
        }

        if(!allowFallback && def.isFallback) {
            return null;
        }

        return def.transform;
    }

    public override void Destroy(Unit unit) {}
    public override void Update(ref NodeLookupData extensionData) {}
}
