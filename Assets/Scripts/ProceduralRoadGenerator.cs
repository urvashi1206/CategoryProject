using System.Collections.Generic;
using UnityEngine;

public class ProceduralRoadGenerator : MonoBehaviour
{
    [Header("Prefabs & Target")]
    public GameObject roadSegmentPrefab;
    [Tooltip("Optional. If null, we'll look for an object tagged 'Player'.")]
    public Transform player;

    [Header("Generation Settings")]
    public int initialSegments = 5;
    public float segmentLength = 40f;          // world X size of a road piece
    public float spawnDistanceAhead = 120f;    // keep this much filled ahead of player (toward -X)
    public float destroyDistanceBehind = 80f;  // delete when this far behind player (toward +X)

    // runtime
    private float nextSpawnX; // where the next segment will go (we move this negative)
    private readonly List<GameObject> spawnedSegments = new List<GameObject>(); // C# 8-safe

    void Awake()
    {
        if (!roadSegmentPrefab)
        {
            Debug.LogError("[ProceduralRoadGenerator] roadSegmentPrefab is not assigned.");
            enabled = false;
            return;
        }

        // Pre-warm a strip starting at x=0 and extending to the LEFT (−X),
        // since the car moves left. (So not actually 'centered'—adjust if you want.)
        nextSpawnX = 0f;
        for (int i = 0; i < initialSegments; i++)
            SpawnNextSegment();
    }

    void Update()
    {
        // If no player assigned yet, try to find one by tag
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) SetPlayer(p.transform);
            else return; // no player yet -> do nothing this frame
        }

        // Fill space AHEAD of player (car moves toward negative X).
        // We want the next spawn point (more negative) to be at least spawnDistanceAhead ahead of the player.
        // i.e., (playerX - nextSpawnX) < spawnDistanceAhead ⇒ not enough ahead ⇒ spawn more.
        while (player.position.x - nextSpawnX < spawnDistanceAhead)
            SpawnNextSegment();

        // Despawn segments that are far BEHIND the player (to the RIGHT, i.e., higher X).
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            var seg = spawnedSegments[i];
            if (!seg) { spawnedSegments.RemoveAt(i); continue; }

            float segX = seg.transform.position.x;

            // BEHIND if segX > playerX; remove when (segX - playerX) exceeds threshold.
            if (segX - player.position.x > destroyDistanceBehind)
            {
                Destroy(seg);
                spawnedSegments.RemoveAt(i);
            }
        }
    }

    /// <summary>Call this after you spawn/select the car so generation can key off it.</summary>
    public void SetPlayer(Transform t)
    {
        player = t;
        if (!player) return;

        // Move the spawn cursor just ahead (to the left) of the player so the while-loop fills immediately.
        // If nextSpawnX is to the right of the player (less ahead), push it to one segment left of the player.
        if (player.position.x - nextSpawnX > 0f)
            nextSpawnX = player.position.x - segmentLength;
    }

    void SpawnNextSegment()
    {
        // Safety: don’t try to instantiate if prefab is missing (already guarded in Awake, but cheap check here).
        if (!roadSegmentPrefab) return;

        var spawnPos = new Vector3(nextSpawnX, 0f, 0f);
        var seg = Instantiate(roadSegmentPrefab, spawnPos, Quaternion.identity, transform);
        spawnedSegments.Add(seg);

        // We lay pieces to the LEFT (negative X) because the car goes left.
        nextSpawnX -= segmentLength;
    }
}