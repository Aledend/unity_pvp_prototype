using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetManager {
    public AssetRegistry assetRegistry;
    private AsyncOperationHandle<AssetRegistry> assetRegistryHandle;
    public SpawnTemplates spawnTemplates;
    private AsyncOperationHandle<SpawnTemplates> spawnTemplatesHandle;
    public ItemDatabase itemDatabase;
    private AsyncOperationHandle<ItemDatabase> itemDatabaseHandle;

    public async Task Init() {
        assetRegistryHandle = Addressables.LoadAssetAsync<AssetRegistry>("Assets/Assets/AssetRegistry.asset");
        spawnTemplatesHandle = Addressables.LoadAssetAsync<SpawnTemplates>("Assets/Assets/SpawnTemplates.asset");
        itemDatabaseHandle = Addressables.LoadAssetAsync<ItemDatabase>("Assets/Assets/ItemDatabase.asset");

        await assetRegistryHandle.Task;
        if(assetRegistryHandle.IsValid())
            assetRegistry = assetRegistryHandle.Result;

        await spawnTemplatesHandle.Task;
        if(spawnTemplatesHandle.IsValid())
            spawnTemplates = spawnTemplatesHandle.Result;

        await itemDatabaseHandle.Task;
        if(itemDatabaseHandle.IsValid())
            itemDatabase = itemDatabaseHandle.Result;
    }

    public void Destroy() {
        Addressables.Release(assetRegistryHandle);
        Addressables.Release(spawnTemplatesHandle);
        Addressables.Release(itemDatabaseHandle);
    }
}