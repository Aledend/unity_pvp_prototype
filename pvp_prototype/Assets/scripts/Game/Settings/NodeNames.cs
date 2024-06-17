using System;
using System.Collections.Generic;

public enum NodeName
{
    ItemHeldCloseRight,
    ItemHeldClose,
}

public static class NodeNameString {
    public static string GetString(NodeName name) => namesAsString[(int)name];
    private static readonly string[] namesAsString = Enum.GetNames(typeof(NodeName));
    
}

public static class FallbackNodeNames
{
    public static bool TryGet(NodeName name, out NodeName fallbackName) => names.TryGetValue(name, out fallbackName);
    private static readonly Dictionary<NodeName, NodeName> names = new() {
        {NodeName.ItemHeldCloseRight, NodeName.ItemHeldClose},
    };
}