using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetManager {
    public AssetRegistry assetRegistry;
    private AsyncOperationHandle<AssetRegistry> loadHandle;

    public async Task Init() {
        loadHandle = Addressables.LoadAssetAsync<AssetRegistry>("Assets/Assets/AssetRegistry.asset");
        await loadHandle.Task;
        if(loadHandle.IsValid()) {
            assetRegistry = loadHandle.Result;
        }
    }

    public void Destroy() {
        assetRegistry = null;
        Addressables.Release(loadHandle);
    }
}