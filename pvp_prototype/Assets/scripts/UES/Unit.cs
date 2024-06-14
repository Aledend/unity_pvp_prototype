using System.Collections.Generic;
using UnityEngine;

public class Unit
{
    public GameObject gameObject;
    public bool visible = true;

    public static Unit CreateFromGameObject(GameObject go) {
        return new() { gameObject = go };
    }

    public void SetVisibility(bool visible) {
        if(!gameObject) return;
        gameObject.SetActive(visible);
    }
}