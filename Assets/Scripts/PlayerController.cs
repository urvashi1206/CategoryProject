//using UnityEngine;

//[RequireComponent(typeof(Rigidbody))]
//public class PlayerController : MonoBehaviour
//{
//    [Header("Drive")]
//    public bool autoDrive = true;
//    public float targetSpeed = 40f;
//    public float maxSpeed = 40f;
//    public float acceleration = 80f;
//    public float brake = 90f;

//    [Header("Steering")]
//    public float maxSteerAngle = 25f;     // degrees
//    public float steerEase = 12f;         // input smoothing
//    public float steerReturn = 180f;      // deg/sec toward target
//    public float turnScaleAtLowSpeed = 0.4f;

//    [Header("Wheels (visual only)")]
//    public Transform[] wheelMeshes;
//    public float wheelSpinScale = 60f;

//    [Header("Axes")]
//    [Tooltip("LOCAL axis that points forward along the lane for THIS model.\n" +
//             "Most top-down kits that move toward -world X use LOCAL LEFT here.")]
//    public Vector3 localForwardAxis = Vector3.left; // = -transform.right by default

//    // runtime
//    Rigidbody rb;

//    // input
//    float steerInput;      // -1..1
//    float throttleInput;   // -1..1

//    // smoothed / state
//    float steerSmoothed;
//    float currentSteerAngle;   // deg
//    float lastSteerAngle;      // deg
//    float forwardSpeed;        // signed along localForwardAxis in world space

//    bool running = true;

//    void Awake()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.interpolation = RigidbodyInterpolation.Interpolate;
//        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
//        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

//        if (localForwardAxis.sqrMagnitude < 0.5f) localForwardAxis = Vector3.left;
//    }

//    void Update()
//    {
//        if (!running)
//        {
//            // wheel spin ease-out (visual)
//            if (wheelMeshes != null && wheelMeshes.Length > 0)
//            {
//                float spin = Mathf.Max(0f, Mathf.Abs(forwardSpeed) - 5f) * 0.25f * wheelSpinScale * Time.deltaTime;
//                foreach (var w in wheelMeshes) if (w) w.Rotate(Vector3.right, spin, Space.Self);
//            }
//            return;
//        }

//        steerInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
//        throttleInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f);

//        // Smooth steering input
//        float k = 1f - Mathf.Exp(-steerEase * Time.deltaTime);
//        steerSmoothed = Mathf.Lerp(steerSmoothed, steerInput, k);

//        // Visual wheel spin
//        if (wheelMeshes != null && wheelMeshes.Length > 0)
//        {
//            float spin = Mathf.Abs(forwardSpeed) * wheelSpinScale * Time.deltaTime;
//            foreach (var w in wheelMeshes) if (w) w.Rotate(Vector3.right, spin, Space.Self);
//        }
//    }

//    void FixedUpdate()
//    {
//        // WORLD forward along the lane = this transform’s LOCAL forward axis in world space
//        Vector3 laneForward = transform.TransformDirection(localForwardAxis).normalized;

//        if (!running)
//        {
//            rb.angularVelocity = Vector3.zero;
//            rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, (brake + acceleration) * Time.fixedDeltaTime);
//            forwardSpeed = Vector3.Dot(rb.velocity, laneForward);
//            currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, 0f, steerReturn * Time.fixedDeltaTime);
//            lastSteerAngle = currentSteerAngle;
//            return;
//        }

//        // -------- SPEED (along current heading) --------
//        float currForward = Vector3.Dot(rb.velocity, laneForward);
//        float target = autoDrive
//            ? targetSpeed
//            : (throttleInput > 0.01f ? maxSpeed * throttleInput : 0f);
//        float accel = (autoDrive || throttleInput > 0.01f) ? acceleration : brake;
//        float nextForward = Mathf.MoveTowards(currForward, target, accel * Time.fixedDeltaTime);

//        // Compose velocity: keep vertical (gravity), zero lateral
//        float vertical = rb.velocity.y;
//        rb.velocity = laneForward * nextForward + Vector3.up * vertical;
//        forwardSpeed = nextForward;

//        // -------- STEERING (apply yaw; velocity follows because laneForward uses transform) --------
//        float speedNorm = Mathf.InverseLerp(0f, Mathf.Max(1f, targetSpeed), Mathf.Abs(nextForward));
//        float lowScale = Mathf.Lerp(turnScaleAtLowSpeed, 1f, speedNorm);
//        float targetSteerAngle = Mathf.Clamp(steerSmoothed * maxSteerAngle * lowScale, -maxSteerAngle, maxSteerAngle);

//        float maxStep = steerReturn * Time.fixedDeltaTime;
//        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, maxStep);

//        float delta = currentSteerAngle - lastSteerAngle;
//        if (Mathf.Abs(delta) > 0.0001f)
//        {
//            // prevent physics yaw from fighting our steering
//            rb.angularVelocity = Vector3.zero;
//            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, delta, 0f));
//        }
//        lastSteerAngle = currentSteerAngle;
//    }

//    // ---------- Public controls for GameManager ----------

//    public void StopImmediately()
//    {
//        running = false;
//        steerInput = throttleInput = 0f;
//        steerSmoothed = 0f;
//        currentSteerAngle = lastSteerAngle = 0f;
//        rb.angularVelocity = Vector3.zero;
//        rb.velocity = Vector3.zero;
//    }

//    public void StopSoft()
//    {
//        running = false;
//        steerInput = throttleInput = 0f;
//        steerSmoothed = 0f;
//        // will coast/brake in FixedUpdate
//    }

//    public void StartControl()
//    {
//        running = true;
//        steerInput = throttleInput = 0f;
//        steerSmoothed = 0f;
//        currentSteerAngle = lastSteerAngle = 0f;
//    }
//}

using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Drive")]
    public bool autoDrive = true;
    public float targetSpeed = 40f;
    public float maxSpeed = 40f;
    public float acceleration = 80f;
    public float brake = 90f;

    [Header("Steering")]
    public float maxSteerAngle = 25f;     // degrees
    public float steerEase = 12f;         // input smoothing
    public float steerReturn = 180f;      // deg/sec toward target
    public float turnScaleAtLowSpeed = 0.4f;

    [Header("Wheels (visual only)")]
    public Transform[] wheelMeshes;
    public float wheelSpinScale = 60f;

    [Header("Axes")]
    [Tooltip("LOCAL axis that points forward along the lane for THIS model.\n" +
             "Most top-down kits that move toward -world X use LOCAL LEFT here.")]
    public Vector3 localForwardAxis = Vector3.left; // = -transform.right by default

    // runtime
    Rigidbody rb;

    // input
    float steerInput;      // -1..1
    float throttleInput;   // -1..1

    // smoothed / state
    float steerSmoothed;
    float currentSteerAngle;   // deg
    float lastSteerAngle;      // deg
    float forwardSpeed;        // signed along lane

    bool running = true;

    // Stable lane direction (world space, flattened)
    Vector3 baseLaneForwardWS;

    // Preserve the model's initial yaw relative to lane
    Quaternion initialYawOffset; // yaw that maps lane-forward -> your model's current forward

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (localForwardAxis.sqrMagnitude < 0.5f) localForwardAxis = Vector3.left;

        // Stable lane direction from your configured local axis
        baseLaneForwardWS = transform.TransformDirection(localForwardAxis).normalized;
        baseLaneForwardWS.y = 0f;
        baseLaneForwardWS = baseLaneForwardWS.sqrMagnitude > 1e-4f ? baseLaneForwardWS.normalized : Vector3.left;

        // Compute the model's current forward (flattened)
        Vector3 modelFwdWS = transform.forward;
        modelFwdWS.y = 0f;
        modelFwdWS = modelFwdWS.sqrMagnitude > 1e-4f ? modelFwdWS.normalized : baseLaneForwardWS;

        // This yaw offset preserves your car's current look direction relative to the lane
        initialYawOffset = Quaternion.FromToRotation(baseLaneForwardWS, modelFwdWS);
    }

    void Update()
    {
        if (!running)
        {
            // wheel spin ease-out (visual)
            if (wheelMeshes != null && wheelMeshes.Length > 0)
            {
                float spin = Mathf.Max(0f, Mathf.Abs(forwardSpeed) - 5f) * 0.25f * wheelSpinScale * Time.deltaTime;
                foreach (var w in wheelMeshes) if (w) w.Rotate(Vector3.right, spin, Space.Self);
            }
            return;
        }

        steerInput = Mathf.Clamp(Input.GetAxisRaw("Horizontal"), -1f, 1f);
        throttleInput = Mathf.Clamp(Input.GetAxisRaw("Vertical"), -1f, 1f);

        // Smooth steering input
        float k = 1f - Mathf.Exp(-steerEase * Time.deltaTime);
        steerSmoothed = Mathf.Lerp(steerSmoothed, steerInput, k);

        // Visual wheel spin
        if (wheelMeshes != null && wheelMeshes.Length > 0)
        {
            float spin = Mathf.Abs(forwardSpeed) * wheelSpinScale * Time.deltaTime;
            foreach (var w in wheelMeshes) if (w) w.Rotate(Vector3.right, spin, Space.Self);
        }
    }

    void FixedUpdate()
    {
        // Compute steering target (use last forwardSpeed to scale low-speed turning)
        float speedNorm = Mathf.InverseLerp(0f, Mathf.Max(1f, targetSpeed), Mathf.Abs(forwardSpeed));
        float lowScale = Mathf.Lerp(turnScaleAtLowSpeed, 1f, speedNorm);
        float targetSteerAngle = Mathf.Clamp(steerSmoothed * maxSteerAngle * lowScale, -maxSteerAngle, maxSteerAngle);

        float maxStep = steerReturn * Time.fixedDeltaTime;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, maxStep);

        // Desired lane heading from cached lane + steering angle
        Vector3 desiredLaneForward = Quaternion.Euler(0f, currentSteerAngle, 0f) * baseLaneForwardWS;
        desiredLaneForward.y = 0f;
        desiredLaneForward = desiredLaneForward.sqrMagnitude > 1e-4f ? desiredLaneForward.normalized : baseLaneForwardWS;

        if (!running)
        {
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.MoveTowards(rb.velocity, Vector3.zero, (brake + acceleration) * Time.fixedDeltaTime);
            forwardSpeed = Vector3.Dot(rb.velocity, desiredLaneForward);

            // Keep facing with preserved visual yaw
            Quaternion stopRot = Quaternion.LookRotation(desiredLaneForward, Vector3.up) * initialYawOffset;
            rb.MoveRotation(stopRot);

            lastSteerAngle = currentSteerAngle;
            return;
        }

        // -------- SPEED strictly along desired heading --------
        float currForward = Vector3.Dot(rb.velocity, desiredLaneForward);
        float target = autoDrive
            ? targetSpeed
            : (throttleInput > 0.01f ? maxSpeed * throttleInput : 0f);
        float accel = (autoDrive || throttleInput > 0.01f) ? acceleration : brake;
        float nextForward = Mathf.MoveTowards(currForward, target, accel * Time.fixedDeltaTime);

        float vertical = rb.velocity.y; // preserve gravity
        rb.velocity = desiredLaneForward * nextForward + Vector3.up * vertical;
        forwardSpeed = nextForward;

        // -------- ORIENTATION (override physics yaw) --------
        rb.angularVelocity = Vector3.zero; // cancel collision-induced yaw
        Quaternion desiredRot = Quaternion.LookRotation(desiredLaneForward, Vector3.up) * initialYawOffset;
        rb.MoveRotation(desiredRot);

        lastSteerAngle = currentSteerAngle;
    }

    // ---------- Public controls for GameManager ----------

    public void StopImmediately()
    {
        running = false;
        steerInput = throttleInput = 0f;
        steerSmoothed = 0f;
        currentSteerAngle = lastSteerAngle = 0f;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;

        // face lane with preserved visual yaw
        Quaternion rot = Quaternion.LookRotation(baseLaneForwardWS, Vector3.up) * initialYawOffset;
        rb.MoveRotation(rot);
    }

    public void StopSoft()
    {
        running = false;
        steerInput = throttleInput = 0f;
        steerSmoothed = 0f;
        // will coast/brake in FixedUpdate
    }

    public void StartControl()
    {
        running = true;
        steerInput = throttleInput = 0f;
        steerSmoothed = 0f;
        currentSteerAngle = lastSteerAngle = 0f;

        // ensure facing matches lane with your original visual yaw
        Quaternion rot = Quaternion.LookRotation(baseLaneForwardWS, Vector3.up) * initialYawOffset;
        rb.MoveRotation(rot);
    }
}