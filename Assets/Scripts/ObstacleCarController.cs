using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ObstacleCarController : MonoBehaviour
{
    [Header("Motion")]
    public float speed = 30f;

    [Tooltip("If true, auto-pick which LOCAL axis best matches desiredWorldForward for THIS prefab's rotation.")]
    public bool autoPickLocalAxis = true;

    [Tooltip("Target world direction of traffic flow. Set to your road direction (e.g., -X).")]
    public Vector3 desiredWorldForward = Vector3.left; // world -X

    [Tooltip("Used only when autoPickLocalAxis = false.")]
    public Vector3 localForwardAxis = Vector3.left;

    [Header("Physics")]
    public bool enableGravity = true;
    public bool freezeTilt = true;
    public float downforceAcceleration = 30f;
    public bool lockYToSpawnHeight = false; // use only if your road is perfectly flat

    Rigidbody rb;
    Vector3 chosenLocalAxis = Vector3.left;
    float lockedY;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = enableGravity;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        if (freezeTilt)
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (lockYToSpawnHeight)
        {
            lockedY = transform.position.y;
            rb.constraints |= RigidbodyConstraints.FreezePositionY;
        }

        var col = GetComponent<Collider>();
        if (col) col.isTrigger = false;

        if (autoPickLocalAxis)
            chosenLocalAxis = PickBestLocalAxis(desiredWorldForward.normalized);
        else
            chosenLocalAxis = localForwardAxis.normalized;
    }

    void FixedUpdate()
    {
        Vector3 laneForwardWS = transform.TransformDirection(chosenLocalAxis).normalized;

        // keep motion horizontal; prevents unintended climb if model's forward has Y
        laneForwardWS.y = 0f;
        if (laneForwardWS.sqrMagnitude < 1e-4f) laneForwardWS = Vector3.left;
        laneForwardWS.Normalize();

        float vertical = rb.velocity.y;
        rb.velocity = laneForwardWS * speed + Vector3.up * vertical;

        if (enableGravity && downforceAcceleration > 0f && !lockYToSpawnHeight)
            rb.AddForce(Vector3.down * downforceAcceleration, ForceMode.Acceleration);
    }

    Vector3 PickBestLocalAxis(Vector3 desiredWorldDir)
    {
        Vector3[] localCandidates = {
            Vector3.right, Vector3.left,
            Vector3.forward, Vector3.back,
            Vector3.up, Vector3.down
        };

        float bestDot = -2f;
        Vector3 best = Vector3.left;

        foreach (var localAxis in localCandidates)
        {
            Vector3 worldAxis = transform.TransformDirection(localAxis).normalized;
            worldAxis.y = 0f; worldAxis.Normalize();
            float d = Vector3.Dot(worldAxis, desiredWorldDir);
            if (d > bestDot) { bestDot = d; best = localAxis; }
        }
        return best;
    }
}
