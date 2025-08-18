using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float xOffset = 0f;
    private float currentX = -120f;
    private float nextX;
    private float timer;

    [Header("Placement")]
    [Tooltip("Center of the drivable lane on Z (world space).")]
    public float laneCenterZ = 0f;
    [Tooltip("Half the usable lane width. If you can drive from Z=-6 to Z=+6, set 6.")]
    public float laneHalfWidth = 6f;
    [Tooltip("Keeps spawns away from the edges a bit.")]
    public float edgePadding = 0.5f;
    [Tooltip("Small randomization on X so items don’t form a perfect line.")]
    public float xJitter = 3f;

    private List<ItemSO> allItems;

    [Header("Category Source")]
    [SerializeField] public ItemDatabaseSO itemDatabase;



    // Start is called before the first frame update
    void Start()
    {
        nextX = currentX;
        allItems = new List<ItemSO>(itemDatabase.allItems);
        timer = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (allItems == null || allItems.Count == 0) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        timer = 0f;
        ItemSO pick = allItems[Random.Range(0, allItems.Count)];

        float zMin = laneCenterZ - laneHalfWidth + edgePadding;
        float zMax = laneCenterZ + laneHalfWidth - edgePadding;
        float z = Random.Range(zMin, zMax);

        float x = nextX + Random.Range(-xJitter, xJitter);

        Vector3 spawnPos = new Vector3(
            x,
            pick.prefab.transform.position.y,
            z
        );

        Instantiate(pick.prefab, spawnPos, pick.prefab.transform.rotation, transform);
        nextX -= xOffset;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // visualize the lane band
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        float z0 = laneCenterZ - laneHalfWidth;
        float z1 = laneCenterZ + laneHalfWidth;
        Vector3 a = new Vector3(currentX, 0, z0);
        Vector3 b = new Vector3(currentX - 100f, 0, z1); // 100 units preview
        Vector3 center = (a + b) * 0.5f;
        Vector3 size = new Vector3(Mathf.Abs(b.x - a.x), 0.05f, Mathf.Abs(z1 - z0));
        Gizmos.DrawCube(center, size);
    }
#endif
}
