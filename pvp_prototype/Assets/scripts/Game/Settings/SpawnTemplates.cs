using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "SpawnTemplates", menuName = "UES/SpawnTemplates")]
public class SpawnTemplates : ScriptableObject
{
    [Serializable]
    public class SpawnTemplateDefinition {
        public ExtensionGroup[] extensionGroups;
        public AssetReferenceGameObject visual;
    }

    public SpawnTemplateDefinition playerCharacter;
}
