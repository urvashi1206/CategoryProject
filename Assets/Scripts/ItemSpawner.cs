using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Timing / Rows")]
    public float spawnInterval = 0.6f;   // how often to spawn a row
    public float rowSpacing = 8f;        // how far to move forward (X) for the next row
    public int minPerRow = 1;            // items per row
    public int maxPerRow = 3;

    [Header("Lane (world Z)")]
    public float laneCenterZ = 0f;
    public float laneHalfWidth = 6f;     // usable Z is [center - half .. center + half]
    public float edgePadding = 0.5f;     // keep a bit away from the edges
    public float zJitter = 0.25f;        // small wobble so rows aren’t too perfect

    [Header("Height")]
    public float groundY = 0f;           // set this to your road’s Y

    [Header("Start (world X forward is negative)")]
    public float startX = -40f;          // first row X
    private float nextX;
    private float timer;

    [SerializeField] public ItemDatabaseSO itemDatabase;
    private List<ItemSO> allItems;

    void Start()
    {
        nextX = startX;

        allItems = new List<ItemSO>(itemDatabase.allItems ?? new List<ItemSO>());
        allItems.RemoveAll(i => !i || !i.prefab);

        minPerRow = Mathf.Clamp(minPerRow, 1, 8);
        maxPerRow = Mathf.Max(maxPerRow, minPerRow);
        edgePadding = Mathf.Clamp(edgePadding, 0f, Mathf.Max(0f, laneHalfWidth));
        if (rowSpacing <= 0f) rowSpacing = 6f;
    }

    void Update()
    {
        if (allItems == null || allItems.Count == 0) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        SpawnRow(nextX);
        nextX -= Mathf.Abs(rowSpacing); // move further into negative X for next row
    }

    void SpawnRow(float x)
    {
        int count = Random.Range(minPerRow, maxPerRow + 1);

        float zMin = laneCenterZ - laneHalfWidth + edgePadding;
        float zMax = laneCenterZ + laneHalfWidth - edgePadding;

        for (int i = 0; i < count; i++)
        {
            // even slots across lane; for 1 item use center
            float t = (count == 1) ? 0.5f : i / (float)(count - 1);
            float z = Mathf.Lerp(zMin, zMax, t);
            if (zJitter > 0f) z = Mathf.Clamp(z + Random.Range(-zJitter, zJitter), zMin, zMax);

            ItemSO pick = allItems[Random.Range(0, allItems.Count)];
            Vector3 pos = new Vector3(x, groundY, z);

            var go = Instantiate(pick.prefab, pos, pick.prefab.transform.rotation, transform);

            // Snap the object's bottom to groundY
            SnapBottomToY(go, groundY);
        }

        void SnapBottomToY(GameObject go, float targetY)
        {
            // Prefer colliders (gameplay touch), else renderers (visual bottom)
            var colliders = go.GetComponentsInChildren<Collider>(includeInactive: true);
            if (colliders != null && colliders.Length > 0)
            {
                float minY = float.PositiveInfinity;
                foreach (var c in colliders)
                {
                    // bounds are world-space
                    if (c.enabled) minY = Mathf.Min(minY, c.bounds.min.y);
                }
                if (!float.IsPositiveInfinity(minY))
                {
                    Vector3 p = go.transform.position;
                    p.y += (targetY - minY);
                    go.transform.position = p;
                    return;
                }
            }

            var renderers = go.GetComponentsInChildren<Renderer>(includeInactive: true);
            if (renderers != null && renderers.Length > 0)
            {
                float minY = float.PositiveInfinity;
                foreach (var r in renderers)
                {
                    if (r.enabled && r.bounds.size != Vector3.zero)
                        minY = Mathf.Min(minY, r.bounds.min.y);
                }
                if (!float.IsPositiveInfinity(minY))
                {
                    Vector3 p = go.transform.position;
                    p.y += (targetY - minY);
                    go.transform.position = p;
                }
            }
        }
    }
}