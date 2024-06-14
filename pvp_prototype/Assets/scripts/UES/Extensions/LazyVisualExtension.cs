using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public struct LazyVisualData {
    public AsyncOperationHandle<GameObject> loadHandle;
    public GameObject visualGameObject;
}
public class LazyVisualExtension : IExtension<LazyVisualData>
{
    public void Init(Unit unit, ExtensionInitContext extensionInitContext, ref LazyVisualData extensionData) {}

    public void LoadVisual(Unit unit, AssetReferenceGameObject visual) {
        ref var data = ref ExtensionHandler<LazyVisualExtension, LazyVisualData>.GetData(unit);
        data.loadHandle = Addressables.LoadAssetAsync<GameObject>(visual);
        data.loadHandle.Completed += (handle) => {
            ref var data = ref ExtensionHandler<LazyVisualExtension, LazyVisualData>.GetData(unit);
            var prefab = handle.Result;

            GameObject go = Object.Instantiate(prefab);
            data.visualGameObject = go;
            go.transform.SetParent(unit.gameObject.transform, false);
        };
    }

    public void Update(ref LazyVisualData extensionData) {}

    public void Destroy(Unit unit)
    {
        ref var data = ref ExtensionHandler<LazyVisualExtension, LazyVisualData>.GetData(unit);
        if(data.loadHandle.IsValid()) {
            Addressables.Release(data.loadHandle);
        }

        if(data.visualGameObject) {
            Object.Destroy(data.visualGameObject);
        }
    }
}
