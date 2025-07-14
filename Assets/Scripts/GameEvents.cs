using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{
    public static Action<ItemSO> OnCollect;
    public static void CollectItem(ItemSO itm) => OnCollect?.Invoke(itm);
}