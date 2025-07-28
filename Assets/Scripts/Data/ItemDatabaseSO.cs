using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrashCourse/ItemDatabase")]
public class ItemDatabaseSO : ScriptableObject
{
    public List<ItemSO> allItems;
}
