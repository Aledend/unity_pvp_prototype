using UnityEngine;

public class Unit
{
    public GameObject gameObject;
    public static Unit CreateFromGameObject(GameObject go) {
        return new() { gameObject = go };
    }
}