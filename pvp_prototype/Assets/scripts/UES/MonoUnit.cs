using UnityEngine;

public class MonoUnit : MonoBehaviour
{
    [SerializeField]
    ExtensionGroup[] extensionGroup = new ExtensionGroup[] {};

    void Start() {
        EntryPoint.HookInit(this);
    }

    public void Init() {
        Managers.unitSpawner.SpawnUnit(gameObject, extensionGroup);
    }
}
