using UnityEngine;

public static class ExtensionMethods
{
    public static bool Contains(this LayerMask layerMask, int layer)
    {
        return layerMask.value == (layerMask.value | (1 << layer));
    }
}