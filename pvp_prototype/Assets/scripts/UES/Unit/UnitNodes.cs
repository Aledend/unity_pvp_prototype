using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

class NodeData
{
    private Unit[] linkedUnits;
    public int numLinkedUnits;

    public Unit Get(int index) => linkedUnits[index];
    public void Add(Unit unit) {
        var newLinkedUnits = ArrayPool<Unit>.Shared.Rent(numLinkedUnits+1);
        if(linkedUnits != null) {
            Array.Copy(linkedUnits, newLinkedUnits, numLinkedUnits);
            ArrayPool<Unit>.Shared.Return(linkedUnits);
        }
        linkedUnits = newLinkedUnits;
        linkedUnits[numLinkedUnits] = unit;
        ++numLinkedUnits;
    }
    public void Remove(Unit unit) {
        var newLinkedUnits = ArrayPool<Unit>.Shared.Rent(numLinkedUnits-1);
        int i=0;
        for(;i<numLinkedUnits && linkedUnits[i] != unit; ++i) {
            newLinkedUnits[i] = linkedUnits[i];
        }
        for(;i<numLinkedUnits-1; ++i) {
            newLinkedUnits[i] = linkedUnits[i+1];
        }
        ArrayPool<Unit>.Shared.Return(linkedUnits);
        linkedUnits = newLinkedUnits;
        --numLinkedUnits;
    }
    public void Clear() {
        if(linkedUnits != null) {
            ArrayPool<Unit>.Shared.Return(linkedUnits);
        }
        linkedUnits = null;
    }
}

class UnitNodes
{
    private Transform visual;
    private Dictionary<NodeName, NodeData> nodeDatas = null;
    public Dictionary<NodeName, Transform> cache = null;
    public Dictionary<Unit, NodeName> linkedUnitLookup = null;

    public UnitNodes(Transform visual)
    {
        this.visual = visual;
    }

    ~UnitNodes()
    {
        UpdateVisualGameObject(null);
        if(nodeDatas != null) {
            foreach((NodeName _, NodeData nodeData) in nodeDatas) {
                GenericPool<NodeData>.Release(nodeData);
            }
            DictionaryPool<NodeName, NodeData>.Release(nodeDatas);
        }
        if(linkedUnitLookup != null)
            DictionaryPool<Unit, NodeName>.Release(linkedUnitLookup);
    }

    public void Link(Unit unitToLink, NodeName nodeName) {
        if(linkedUnitLookup?.ContainsKey(unitToLink) ?? false) {
            UnLink(unitToLink);
        }

        Transform linkTo = GetNode(nodeName);
        nodeDatas ??= DictionaryPool<NodeName, NodeData>.Get();
        linkedUnitLookup ??= DictionaryPool<Unit, NodeName>.Get();
        if(!nodeDatas.TryGetValue(nodeName, out NodeData data)) {
            data = GenericPool<NodeData>.Get();
            data.Clear();
            data.Add(unitToLink);
            nodeDatas[nodeName] = data;
        } else {
            data.Add(unitToLink);
        }

        linkedUnitLookup[unitToLink] = nodeName;
        unitToLink.GameObject().transform.SetParent(linkTo, false);
    }

    public void UnLink(Unit unitToUnlink) {
        if(linkedUnitLookup?.TryGetValue(unitToUnlink, out NodeName linkedNode) ?? false) {
            NodeData nodeData = nodeDatas[linkedNode];
            nodeData.Remove(unitToUnlink);
            linkedUnitLookup.Remove(unitToUnlink);
        }
    }

    public void UpdateVisualGameObject(Transform newVisual) {
        visual = newVisual;
        cache?.Clear();

        var currentlyLinkedUnits = nodeDatas;
        if(currentlyLinkedUnits != null) {
            foreach((var nodeName, var nodeData) in currentlyLinkedUnits) {
                for(int i = 0; i < nodeData.numLinkedUnits; ++i) {
                    Unit unit = nodeData.Get(i);
                    Link(unit, nodeName);
                }
            }
        }
    }

    public Transform Root() {
        return visual;
    }

    public Transform GetNode(NodeName nodeName) {
        cache ??= new();
        if(!cache.TryGetValue(nodeName, out Transform transform)) {
            Transform matchingTransform = Root().Find(NodeNameString.GetString(nodeName));
            if(matchingTransform) {
                transform = matchingTransform;
                cache[nodeName] = transform;
            } else if(FallbackNodeNames.TryGet(nodeName, out NodeName fallbackName)) {
                Transform fallbackObject = GetNode(fallbackName);
                if(fallbackObject) {
                    transform = fallbackObject;
                    cache[nodeName] = transform;
                }
            }

            if(!transform) {
                transform = Root();
                cache[nodeName] = transform;
            }
        }

        return transform;
    }
}