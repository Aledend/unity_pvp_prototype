using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnTemplates", menuName = "UES/SpawnTemplates")]
public class SpawnTemplates : ScriptableObject
{
    [Serializable]
    public class SpawnTemplateDefinition {
        public ExtensionGroup[] extensionGroups;
    }

    public SpawnTemplateDefinition character;
}
