using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class ItemDefinition {
    public SpawnTemplates.SpawnTemplateDefinition itemSpawnTemplate;
    public ActionTemplate actionTemplate;
    public NodeName node;
    public bool supportsMultiWield;
}

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public ItemDefinition sword = new();
}
