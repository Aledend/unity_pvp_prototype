using UnityEngine;


public class EntryPoint : MonoBehaviour
{
    private static EntryPoint _instance;
    private static EntryPoint Instance => _instance = _instance != null ? _instance : FindFirstObjectByType<EntryPoint>();
    public static void HookInit(MonoUnit monoUnit) => Instance.boot.HookInit(monoUnit);

    readonly Boot boot = new();

    void Start() {
        boot.Init();
    }

    void Update() {
        boot.Update();
    }
}
