using System.Collections.Generic;
using UnityEngine;

public class TruckStreamManager : MonoBehaviour
{
    [Header("Prefabs & Player")]
    public GameObject[] truckPrefabs;            // your 3 trucks
    [Tooltip("Optional. If null, auto-find object tagged 'Player'.")]
    public Transform player;

    [Header("Spawn/Despawn (player-relative, like your road)")]
    public float spawnDistanceAhead = 120f;      // keep this much filled AHEAD of player (towards -X)
    public float destroyDistanceBehind = 80f;    // destroy when this far BEHIND player (+X side)

    [Header("X spacing between trucks")]
    public float minSpacingX = 18f;              // don't spawn another truck too close on X
    public float truckSpeed = 8f;                // -X speed for all trucks

    [Header("Z band (your choice)")]
    public float zCenter = 0f;                   // lane center in world Z
    public float zHalfWidth = 6f;                // usable band = [center-half .. center+half]
    public float edgePadding = 0.5f;             // keep a bit inside edges
    public float zJitter = 0.35f;                // little wobble so it’s not identical

    [Header("Y & Rotation")]
    public float groundY = 0f;                   // force Y = 0
    public bool keepPrefabRotation = true;       // use prefab rotation

    // runtime cursor like your road generator
    private float nextSpawnX;                    // where next truck row/instance goes (we move this negative)
    private readonly List<GameObject> live = new();

    void Awake()
    {
        // pre-warm a tiny stretch so you see something instantly
        nextSpawnX = 0f;
    }

    void Update()
    {
        // lazy bind player (same behavior as your road script)
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) SetPlayer(p.transform);
            else return; // no player yet => do nothing
        }

        // 1) move trucks along -X
        float dx = -truckSpeed * Time.deltaTime;
        for (int i = live.Count - 1; i >= 0; i--)
        {
            var go = live[i];
            if (!go) { live.RemoveAt(i); continue; }
            var t = go.transform;
            t.position = new Vector3(t.position.x + dx, t.position.y, t.position.z);

            // 2) destroy when far BEHIND player
            if (player.position.x - t.position.x > destroyDistanceBehind)
            {
                Destroy(go);
                live.RemoveAt(i);
            }
        }

        // 3) fill AHEAD of player (towards -X) similar to road
        //     Keep laying trucks while the gap ahead is smaller than target distance
        while (player.position.x - nextSpawnX < spawnDistanceAhead)
            SpawnNextTruck();
    }

    /// <summary>Call this when you spawn the car at runtime.</summary>
    public void SetPlayer(Transform t)
    {
        player = t;
        if (!player) return;

        // place spawn cursor just ahead so while-loop fills immediately
        if (player.position.x - nextSpawnX > 0f)
            nextSpawnX = player.position.x - minSpacingX;
    }

    void SpawnNextTruck()
    {
        if (truckPrefabs == null || truckPrefabs.Length == 0) return;

        // X: player se aage = smaller X (because forward is -X)
        float spawnX = nextSpawnX;

        // Z: your chosen band + slight jitter
        float zMin = zCenter - zHalfWidth + Mathf.Max(0f, edgePadding);
        float zMax = zCenter + zHalfWidth - Mathf.Max(0f, edgePadding);
        if (zMax < zMin) { var tmp = zMin; zMin = zMax; zMax = tmp; } // safety
        float z = Random.Range(zMin, zMax);
        if (zJitter > 0f) z = Mathf.Clamp(z + Random.Range(-zJitter, zJitter), zMin, zMax);

        // pick prefab and instantiate
        var prefab = truckPrefabs[Random.Range(0, truckPrefabs.Length)];
        var rot = keepPrefabRotation ? prefab.transform.rotation : Quaternion.identity;
        var go = Instantiate(prefab, new Vector3(spawnX, groundY, z), rot, transform);
        live.Add(go);

        // advance cursor further into negative X (like your segmentLength usage)
        nextSpawnX -= Mathf.Abs(minSpacingX);
    }
}
