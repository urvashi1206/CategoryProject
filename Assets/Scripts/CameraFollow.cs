using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // can be left empty
    public string targetTag = "Player";      // auto-find when target is null

    [Header("Follow")]
    public Vector3 offset = new Vector3(0f, 25f, 0f);  // top-down; tweak for your setup
    public float smoothTime = 0.2f;                   // lower = snappier

    [Header("Axes")]
    public bool followX = true;
    public bool followY = true;
    public bool followZ = true;

    Vector3 _velocity;        // for SmoothDamp
    float _recheckTimer = 0f; // throttle auto-find

    /// <summary>Call this right after you spawn the car.</summary>
    public void SetTarget(Transform t) => target = t;

    void LateUpdate()
    {
        // If no target yet, try to find a GameObject tagged "Player" every 0.25s
        if (!target)
        {
            _recheckTimer -= Time.deltaTime;
            if (_recheckTimer <= 0f)
            {
                _recheckTimer = 0.25f;
                var go = GameObject.FindGameObjectWithTag(targetTag);
                if (go) target = go.transform;
            }
            return;
        }

        // Desired position with offset
        var desired = target.position + offset;
        var current = transform.position;

        // Optionally lock axes
        if (!followX) desired.x = current.x;
        if (!followY) desired.y = current.y;
        if (!followZ) desired.z = current.z;

        // Smooth follow
        transform.position = Vector3.SmoothDamp(current, desired, ref _velocity, smoothTime);
    }
}
