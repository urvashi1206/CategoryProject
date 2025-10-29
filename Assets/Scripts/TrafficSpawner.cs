using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns traffic ahead of the player with solid anti-overlap:
/// - Capsule overlap using prefab length/width (not just a point).
/// - Any collider (non-trigger) blocks, not only rigidbodies.
/// - Same-lane spacing accounts for per-prefab length.
/// - Preserves prefab rotation; movement is driven by ObstacleCarController.desiredWorldForward.
/// </summary>
public class TrafficSpawner : MonoBehaviour
{
    [Header("Auto Player Lookup")]
    public string playerTag = "Player";
    Transform player;
    PlayerController playerController;

    [Header("Prefabs")]
    public GameObject[] carPrefabs;

    [Header("Spawn Tuning")]
    [Tooltip("Nominal forward distance ahead of the player to spawn.")]
    public float spawnDistanceAhead = 60f;
    [Tooltip("Random delay between spawn attempts.")]
    public Vector2 spawnIntervalRange = new Vector2(1.3f, 2.2f);
    [Tooltip("Jitter (±) along forward so spawns aren’t all at the same distance.")]
    public float forwardJitterRange = 6f;
    [Tooltip("Traffic speed as a fraction of the player's target speed.")]
    public Vector2 speedAsPlayerMultiplier = new Vector2(0.8f, 1.08f);
    [Tooltip("Hard cap on simultaneously active traffic.")]
    public int maxActiveCars = 16;

    [Header("Road / Lanes")]
    [Tooltip("Half of drivable width. Lane offsets are clamped to [-roadHalfWidth, +roadHalfWidth].")]
    public float roadHalfWidth = 10f;
    [Tooltip("Side offsets from lane center (meters). Example: -3.5, 0, +3.5")]
    public float[] laneSideOffsets = new float[] { -3.5f, 0f, 3.5f };
    [Tooltip("Vertical offset after road snap.")]
    public float yOffset = 0.0f;

    [Header("Lane Direction")]
    [Tooltip("Use a fixed world forward (straight roads) instead of deriving from the player.")]
    public bool forceWorldLaneForward = true;
    public Vector3 worldLaneForward = Vector3.left;

    [Header("Ground / Road Snap")]
    [Tooltip("DRIVABLE ROAD layers only (NOT grass).")]
    public LayerMask roadMask;
    public float snapRayHeight = 10f;
    public float snapRayDepth = 100f;

    [Header("Spacing / Safety")]
    [Tooltip("Extra desired gap (meters) added beyond car lengths for same-lane spacing.")]
    public float extraSameLaneGap = 6f;
    [Tooltip("Global 3D proximity check to avoid diagonal crowding.")]
    public float minAnyLaneSpacing = 10f;
    [Tooltip("Don’t spawn if any vehicle is within this of the player’s probe position.")]
    public float minPlayerSeparation = 14f;
    [Tooltip("Cooldown per lane (seconds) to avoid hammering the same lane).")]
    public float laneCooldownSeconds = 0.6f;
    [Tooltip("Max lane attempts per spawn tick.")]
    public int maxSpawnAttempts = 8;

    [Header("Overlap / Layers")]
    [Tooltip("LayerMask for vehicle colliders (player + all traffic).")]
    public LayerMask vehicleMask;

    [Header("Lifetime Despawn")]
    public Vector2 lifetimeRange = new Vector2(9f, 16f);

    [Header("Physics Guard (for spawned cars)")]
    [Tooltip("Freeze X/Z rotation on a spawned car’s Rigidbody to reduce tipping.")]
    public bool freezeSpawnedCarTilt = true;

    [Header("Debug")]
    [SerializeField] bool verboseLogs = false;

    // ===== Internals =====
    class CarEntry
    {
        public Transform t;
        public float despawnAt;
        public int laneIndex;
        public float halfLength; // cached footprint half-length for spacing
    }

    struct Footprint
    {
        public float halfLength; // along lane
        public float radius;     // half-width for capsule radius
    }

    readonly List<CarEntry> activeCars = new();
    float[] laneNextAllowedSpawn; // per-lane cooldown timestamps
    readonly Dictionary<GameObject, Footprint> footprintCache = new();

    void Start()
    {
        // Clamp offsets so misconfig can't send you into grass.
        for (int i = 0; i < laneSideOffsets.Length; i++)
            laneSideOffsets[i] = Mathf.Clamp(laneSideOffsets[i], -roadHalfWidth, roadHalfWidth);

        laneNextAllowedSpawn = new float[Mathf.Max(1, laneSideOffsets.Length)];

        // Pre-cache footprints so first spawns are accurate.
        foreach (var p in carPrefabs)
            if (p) GetFootprint(p);

        StartCoroutine(WaitForPlayerThenRun());
    }

    IEnumerator WaitForPlayerThenRun()
    {
        while (!TryResolvePlayer())
            yield return new WaitForSeconds(0.2f);
        StartCoroutine(SpawnLoop());
    }

    bool TryResolvePlayer()
    {
        if (!player)
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (!go) return false;
            player = go.transform;
        }
        if (!playerController)
        {
            playerController = player.GetComponent<PlayerController>();
            if (!playerController) return false;
        }
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("[TrafficSpawner] No carPrefabs assigned.");
            enabled = false;
            return true;
        }
        return true;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            CullExpiredCars();
            if (activeCars.Count < maxActiveCars)
                TrySpawnOne();
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);
        }
    }

    Vector3 GetLaneForwardWS()
    {
        Vector3 fwd = (forceWorldLaneForward && worldLaneForward.sqrMagnitude > 0.25f)
            ? worldLaneForward
            : player.TransformDirection(playerController.localForwardAxis);
        fwd.y = 0f;
        return (fwd.sqrMagnitude > 0.0001f) ? fwd.normalized : Vector3.left;
    }

    void TrySpawnOne()
    {
        if (!player || !playerController) return;

        Vector3 laneForward = GetLaneForwardWS();
        Vector3 laneRight = Vector3.Cross(Vector3.up, laneForward).normalized;

        // Avoid popping into the player’s bubble
        Vector3 nominal = player.position + laneForward * spawnDistanceAhead;
        if (AnyVehicleWithin(nominal, minPlayerSeparation))
            return;

        GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
        if (!prefab) return;
        var fp = GetFootprint(prefab);

        int laneCount = laneSideOffsets.Length;
        if (laneCount == 0) return;

        int[] laneOrder = ShuffledLaneIndices(laneCount);

        int attempts = 0;
        for (int i = 0; i < laneCount && attempts < maxSpawnAttempts; i++)
        {
            int laneIndex = laneOrder[i];
            attempts++;

            // per-lane cooldown
            if (Time.time < laneNextAllowedSpawn[laneIndex]) continue;

            float side = laneSideOffsets[laneIndex];

            float forwardJitter = (forwardJitterRange > 0f)
                ? Random.Range(-forwardJitterRange, forwardJitterRange)
                : 0f;

            Vector3 basePos = player.position
                              + laneForward * (spawnDistanceAhead + forwardJitter)
                              + laneRight * side;

            // Snap to ROAD (not grass)
            if (!SnapToRoad(basePos, out Vector3 spawnPos))
                continue;

            // 1) Capsule overlap at spawn using prefab length/width
            if (CapsuleBlockedAt(spawnPos, laneForward, fp))
                continue;

            // 2) Same-lane spacing (length-aware)
            if (!PassesSameLaneSpacing(spawnPos, laneForward, laneIndex, fp.halfLength, extraSameLaneGap))
                continue;

            // 3) Any-lane proximity (diagonal)
            if (AnyVehicleWithin(spawnPos, minAnyLaneSpacing))
                continue;

            // Spawn
            SpawnCar(prefab, spawnPos, laneForward, laneRight, laneIndex, fp.halfLength);
            laneNextAllowedSpawn[laneIndex] = Time.time + laneCooldownSeconds;
            return;
        }

        // All attempts failed → intentionally skip this cycle to prevent stacking.
    }

    // --- Footprint & overlap helpers ---

    Footprint GetFootprint(GameObject prefab)
    {
        if (footprintCache.TryGetValue(prefab, out var fp)) return fp;

        // Try renderers for size; fall back to colliders if needed.
        Bounds b = default;
        var rends = prefab.GetComponentsInChildren<Renderer>(true);
        bool have = false;
        foreach (var r in rends)
        {
            if (!r) continue;
            if (!have) { b = r.bounds; have = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!have)
        {
            var cols = prefab.GetComponentsInChildren<Collider>(true);
            foreach (var c in cols)
            {
                if (!c) continue;
                if (!have) { b = c.bounds; have = true; }
                else b.Encapsulate(c.bounds);
            }
        }

        // Reasonable defaults if we can’t measure
        if (!have)
        {
            fp = new Footprint { halfLength = 2.0f, radius = 1.0f };
        }
        else
        {
            // Project bounds to XZ for width/length estimates
            float width = Mathf.Max(0.5f, Mathf.Max(b.size.x, b.size.z) * 0.5f);
            float length = Mathf.Max(1.5f, Mathf.Min(b.size.x, b.size.z)); // the longer planar axis is unclear pre-rotation; pick min as conservative length
            fp = new Footprint
            {
                halfLength = Mathf.Clamp(length * 0.5f, 1.0f, 8.0f),
                radius = Mathf.Clamp(width * 0.5f, 0.6f, 3.0f)
            };
        }
        footprintCache[prefab] = fp;
        return fp;
    }

    bool CapsuleBlockedAt(Vector3 center, Vector3 laneForward, Footprint fp)
    {
        // Build a horizontal capsule aligned to laneForward covering the car's length.
        Vector3 half = laneForward * fp.halfLength;
        Vector3 a = center - half + Vector3.up * 0.5f;
        Vector3 b = center + half + Vector3.up * 0.5f;
        // Check any collider on vehicleMask (non-trigger) blocks
        var hits = Physics.OverlapCapsule(a, b, fp.radius, vehicleMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (!h || h.isTrigger) continue;
            // Don’t count completely disabled parents (rare)
            if (!h.gameObject.activeInHierarchy) continue;
            return true;
        }
        return false;
    }

    bool PassesSameLaneSpacing(Vector3 candidatePos, Vector3 laneForward, int laneIndex, float candidateHalfLen, float extraGap)
    {
        float sCandidate = Vector3.Dot(candidatePos, laneForward);
        for (int i = 0; i < activeCars.Count; i++)
        {
            var e = activeCars[i];
            if (!e.t || e.laneIndex != laneIndex) continue;

            float sOther = Vector3.Dot(e.t.position, laneForward);
            float neededGap = (candidateHalfLen + e.halfLength) + extraGap;
            if (Mathf.Abs(sOther - sCandidate) < neededGap)
                return false;
        }
        return true;
    }

    bool AnyVehicleWithin(Vector3 position, float radius)
    {
        var hits = Physics.OverlapSphere(position, radius, vehicleMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (!h || h.isTrigger) continue;
            if (!h.gameObject.activeInHierarchy) continue;
            return true;
        }
        return false;
    }

    bool SnapToRoad(Vector3 approxPos, out Vector3 snappedPos)
    {
        Vector3 rayStart = approxPos + Vector3.up * snapRayHeight;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, snapRayDepth, roadMask, QueryTriggerInteraction.Ignore))
        {
            snappedPos = hit.point + Vector3.up * yOffset;
            return true;
        }
        snappedPos = approxPos;
        return false;
    }

    int[] ShuffledLaneIndices(int count)
    {
        int[] order = new int[count];
        for (int i = 0; i < count; i++) order[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }
        return order;
    }

    // --- Spawn / Despawn ---

    void SpawnCar(GameObject prefab, Vector3 pos, Vector3 laneForward, Vector3 laneRight, int laneIndex, float halfLen)
    {
        // Preserve prefab rotation exactly
        Quaternion rot = prefab.transform.rotation;
        GameObject car = Instantiate(prefab, pos, rot);

        if (freezeSpawnedCarTilt)
        {
            var rb = car.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        float baseTarget = Mathf.Max(1f, playerController.targetSpeed);
        float speed = Random.Range(speedAsPlayerMultiplier.x, speedAsPlayerMultiplier.y) * baseTarget;

        var oc = car.GetComponent<ObstacleCarController>();
        if (!oc) oc = car.AddComponent<ObstacleCarController>();
        oc.speed = speed;
        oc.desiredWorldForward = laneForward; // Make it move same direction as player

        // slight lateral jitter so lanes aren’t laser-straight
        car.transform.position += laneRight * Random.Range(-0.25f, 0.25f);

        float life = Random.Range(lifetimeRange.x, lifetimeRange.y);
        activeCars.Add(new CarEntry
        {
            t = car.transform,
            despawnAt = Time.time + life,
            laneIndex = laneIndex,
            halfLength = halfLen
        });
    }

    void CullExpiredCars()
    {
        float now = Time.time;
        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            var e = activeCars[i];
            if (!e.t || now >= e.despawnAt)
            {
                if (e.t) Destroy(e.t.gameObject);
                activeCars.RemoveAt(i);
            }
        }
    }
}