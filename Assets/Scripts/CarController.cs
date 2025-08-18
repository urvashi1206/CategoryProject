using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Drive")]
    public bool autoDrive = true;       // constant forward motion
    public float targetSpeed = 40f;     // m/s when autoDrive (?144 km/h / 89 mph)
    public float maxSpeed = 40f;      // used if autoDrive = false
    public float acceleration = 80f;    // m/s^2 speed up
    public float brake = 90f;    // m/s^2 slow down toward 0

    [Header("Steering")]
    public float maxSteerAngle = 25f;   // degrees, absolute cap
    public float steerEase = 12f;   // bigger = smoother response
    public float steerReturn = 180f;  // deg/sec how fast to recenter
    public float turnScaleAtLowSpeed = 0.4f; // steering reduced when slow

    [Header("Wheels (visual only)")]
    public Transform[] wheelMeshes;
    public float wheelSpinScale = 60f;

    Rigidbody rb;

    // input
    float steerInput;    // raw -1..1
    float throttleInput; // raw -1..1

    // smoothed / state
    float steerSmoothed;     // smoothed input
    float currentSteerAngle; // what the car is actually steered to (deg)
    float lastSteerAngle;    // for delta rotation step
    float forwardSpeed;      // signed along -transform.right


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        // Read inputs
        steerInput = Input.GetAxis("Horizontal");
        throttleInput = Input.GetAxis("Vertical");

        // Smooth steering input (critically damped-ish)
        float k = 1f - Mathf.Exp(-steerEase * Time.deltaTime);
        steerSmoothed = Mathf.Lerp(steerSmoothed, steerInput, k);

        // Spin wheel meshes visually
        if (wheelMeshes != null && wheelMeshes.Length > 0)
        {
            float spin = Mathf.Abs(forwardSpeed) * wheelSpinScale * Time.deltaTime;
            foreach (var w in wheelMeshes) if (w) w.Rotate(Vector3.right, spin, Space.Self);
        }
    }


    void FixedUpdate()
    {
        // Lane forward in your game is -X (car's local right negative)
        Vector3 laneForward = -transform.right;

        // --- SPEED ---
        // read current signed speed on the lane axis
        float curr = Vector3.Dot(rb.velocity, laneForward);

        // choose target speed
        float tgt;
        if (autoDrive)
            tgt = targetSpeed;
        else
            tgt = (throttleInput > 0.01f) ? maxSpeed * throttleInput : 0f;

        // pick accel toward target (use stronger brake when not pressing forward)
        float accel = (autoDrive || throttleInput > 0.01f) ? acceleration : brake;
        float next = Mathf.MoveTowards(curr, tgt, accel * Time.fixedDeltaTime);

        // compose final velocity (preserve vertical component for gravity)
        rb.velocity = laneForward * next + Vector3.up * rb.velocity.y;
        forwardSpeed = next;

        // --- STEERING (capped, smooth, auto-recenter) ---
        // reduce steering at low speeds so it doesn't whip in place
        float speedNorm = Mathf.InverseLerp(0f, Mathf.Max(1f, targetSpeed), Mathf.Abs(next));
        float lowSpeedScale = Mathf.Lerp(turnScaleAtLowSpeed, 1f, speedNorm);

        float targetSteerAngle = steerSmoothed * maxSteerAngle * lowSpeedScale;

        // move current steer toward target, with a max rate (steerReturn)
        float maxStep = steerReturn * Time.fixedDeltaTime;
        currentSteerAngle = Mathf.MoveTowards(currentSteerAngle, targetSteerAngle, maxStep);

        // apply only the delta this frame to avoid cumulative spin
        float delta = currentSteerAngle - lastSteerAngle;
        if (Mathf.Abs(delta) > 0.0001f)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, delta, 0f));
        }
        lastSteerAngle = currentSteerAngle;
    }
}
