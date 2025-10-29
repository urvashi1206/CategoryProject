using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Your truck prefabs (3, or more if you like)")]
    public GameObject[] truckPrefabs;

    [Header("Timing")]
    public float spawnInterval = 1.0f;   // how often to try spawn
    public int maxAlive = 3;

    [Header("Ahead spawn (−X is forward for road)")]
    public float aheadX = 40f;           // spawn at player.x - aheadX
    public float killAtX = -200f;        // world kill line (NOT player-relative)

    [Header("Z Band (choose how wide you allow)")]
    public float zCenter = 0f;
    public float zHalfWidth = 6f;        // usable band = [center - half .. center + half]
    public float edgePadding = 0.5f;     // keep a bit inside
    public float zJitter = 0.35f;        // a little wobble so it's not grid-perfect

    [Header("Optional Z de-dup at spawn line")]
    public bool enforceMinZGap = true;
    public float minZGap = 2.0f;         // don't stack on top at spawn
    public float nearSpawnXWindow = 8f;  // only compare trucks whose X is near spawnX

    [Header("Movement")]
    public float truckSpeed = 8f;

    // runtime player
    Transform _player;

    // internals
    float timer;
    readonly List<Transform> live = new();

    // Call this when player is created / available at runtime
    public void SetPlayer(Transform player) => _player = player;

    void Update()
    {
        // clean nulls
        for (int i = live.Count - 1; i >= 0; i--)
            if (live[i] == null) live.RemoveAt(i);

        // kill by world X (no follow)
        for (int i = live.Count - 1; i >= 0; i--)
        {
            var tr = live[i];
            if (!tr) continue;
            if (tr.position.x <= killAtX)
            {
                Destroy(tr.gameObject);
                live.RemoveAt(i);
            }
        }

        // must have player to know "ahead of car"
        if (!_player || truckPrefabs == null || truckPrefabs.Length == 0) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        if (live.Count >= maxAlive) return;

        timer = 0f;
        SpawnOne();
    }

    void SpawnOne()
    {
        // X: player ke aage (tumhara forward -X hai, so smaller X means aage)
        float spawnX = _player.position.x - Mathf.Abs(aheadX);

        // Z range user-controlled
        float zMin = zCenter - zHalfWidth + Mathf.Max(0f, edgePadding);
        float zMax = zCenter + zHalfWidth - Mathf.Max(0f, edgePadding);
        if (zMax < zMin) { var t = zMin; zMin = zMax; zMax = t; } // safety

        float z = Random.Range(zMin, zMax);
        if (zJitter > 0f) z = Mathf.Clamp(z + Random.Range(-zJitter, zJitter), zMin, zMax);

        if (enforceMinZGap && !IsZFreeNearSpawn(z, spawnX))
        {
            // try a few times for a free slot
            const int attempts = 8;
            for (int a = 0; a < attempts; a++)
            {
                float cand = Random.Range(zMin, zMax);
                if (zJitter > 0f) cand = Mathf.Clamp(cand + Random.Range(-zJitter, zJitter), zMin, zMax);
                if (IsZFreeNearSpawn(cand, spawnX)) { z = cand; break; }
            }
        }

        var prefab = truckPrefabs[Random.Range(0, truckPrefabs.Length)];
        Vector3 pos = new Vector3(spawnX, 0f /* force Y=0 */, z);

        // rotation: keep prefab's
        var go = Instantiate(prefab, pos, prefab.transform.rotation);

        var mover = go.GetComponent<VehicleMover>() ?? go.AddComponent<VehicleMover>();
        mover.speed = truckSpeed;
        mover.killAtX = killAtX;

        live.Add(go.transform);
    }

    bool IsZFreeNearSpawn(float candidateZ, float spawnX)
    {
        foreach (var tr in live)
        {
            if (!tr) continue;
            if (Mathf.Abs(tr.position.x - spawnX) <= nearSpawnXWindow)
            {
                if (Mathf.Abs(tr.position.z - candidateZ) < minZGap)
                    return false;
            }
        }
        return true;
    }
}
