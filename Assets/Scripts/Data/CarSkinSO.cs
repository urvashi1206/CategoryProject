using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "CrashCourse/Cars/Car Skin")]
public class CarSkinSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique 0..31. Use list index for simplicity.")]
    public int id;

    public string displayName;

    [Header("Assets")]
    public GameObject prefab;     // the car to spawn in gameplay
    public Sprite icon;           // small icon for selection grid

    [Header("Unlocks")]
    [Tooltip("Unlocked at start without scoring.")]
    public bool defaultUnlocked;

    [Tooltip("Required final/best score to unlock (ignored if defaultUnlocked = true).")]
    public int unlockScore = 0;
}
