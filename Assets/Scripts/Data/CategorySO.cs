using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrashCourse/Category")]
public class CategorySO : ScriptableObject
{
    public string displayName;
    public Color uiColor;
    public List<ItemSO> correctItems;
}
