
using UnityEngine;

public class Unit
{
    private readonly GameObject gameObject;
    private readonly UnitNodes unitNodes;
    public bool visible = true;

    public Unit(GameObject gameObject) {
        this.gameObject = gameObject;
        unitNodes = new(gameObject.transform);
    }

    ~Unit() {}

    public GameObject GameObject() => gameObject;

    public void SetVisibility(bool visible) {
        if(!gameObject) return;
        gameObject.SetActive(visible);
    }

    public void SetVisual(GameObject visual) {
        if(visual) {
            visual.transform.SetParent(gameObject.transform, false);
        }

        visual = visual ? visual : gameObject;
        unitNodes.UpdateVisualGameObject(visual.transform);
    }
    
    public void LinkChild(Unit unit, NodeName nodeName) => unitNodes.Link(unit, nodeName);
}