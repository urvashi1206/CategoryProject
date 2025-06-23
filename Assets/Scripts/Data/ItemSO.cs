using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrashCourse/Item")]
public class ItemSO : ScriptableObject
{
    public string displayName;
    public GameObject prefab;
    public bool isCommon;
}
