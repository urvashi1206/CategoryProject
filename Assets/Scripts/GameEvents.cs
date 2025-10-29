using System;
using UnityEngine;

public static class GameEvents
{
    public static Action<ItemSO> OnCollect;
    public static Action<ItemSO, Vector3> OnCollectWithPos;

    public static void CollectItem(ItemSO item) { OnCollect?.Invoke(item); }
    public static void CollectItemAt(ItemSO item, Vector3 pos)
    {
        OnCollect?.Invoke(item);
        OnCollectWithPos?.Invoke(item, pos);
    }
}