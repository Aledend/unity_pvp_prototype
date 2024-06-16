using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public struct VisualData {
    public GameObject visualGameObject;
    public AsyncOperationHandle<GameObject> loadHandle;
    public Task<GameObject> loadTask;
}

public class VisualExtension : Extension<VisualData>
{

    public override void Destroy(Unit unit)
    {

    }

    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref VisualData extensionData)
    {
        
    }

    public void LoadVisual(Unit unit, GameObject visualPrefab) {
        ref VisualData data = ref GetData<VisualExtension>(unit);
        data.visualGameObject = Object.Instantiate(visualPrefab);
    }

    public override void Update(ref VisualData extensionData)
    {
        throw new System.NotImplementedException();
    }
}

public struct LazyVisualData {
    public AsyncOperationHandle<GameObject> loadHandle;
    public Task<GameObject> loadTask;
}

public class LazyVisualExtension : VisualExtension
{
    public override void Init(Unit unit, ExtensionInitContext extensionInitContext, ref VisualData extensionData)
    {
        base.Init(unit, extensionInitContext, ref extensionData);
        RentExtraData<LazyVisualData>(unit);
    }

    public void LoadVisual(Unit unit, AssetReferenceGameObject visual) {
        ref LazyVisualData lazyData = ref GetExtraData<LazyVisualData>(unit);

        lazyData.loadHandle = Addressables.LoadAssetAsync<GameObject>(visual);
        lazyData.loadTask = lazyData.loadHandle.Task;
        lazyData.loadHandle.Completed += (handle) => {
            ref VisualData data = ref GetData<LazyVisualExtension>(unit);
            var prefab = handle.Result;

            GameObject go = Object.Instantiate(prefab);
            data.visualGameObject = go;

            unit.SetVisual(go);
        };
    }

    public Task<GameObject> Visual(Unit unit) {
        ref var data = ref ExtensionHandler<LazyVisualExtension, VisualData>.GetData(unit);
        return data.loadTask;
    }

    public override void Destroy(Unit unit)
    {
        base.Destroy(unit);

        ReturnExtraData<LazyVisualData>(unit);

        ref var data = ref ExtensionHandler<VisualExtension, VisualData>.GetData(unit);
        if(data.loadHandle.IsValid()) {
            Addressables.Release(data.loadHandle);
        }

        if(data.visualGameObject) {
            Object.Destroy(data.visualGameObject);
        }
    }
}
