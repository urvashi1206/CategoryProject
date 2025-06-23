using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour
{
    [Header("Stats")]
    public float acceleration = 40f;
    public float maxSpeed = 25f;
    public float brakeForce = 60f;
    public float turnSmooth = 5f;

    Rigidbody rb;
    float horizontal;
    float v;

    public Transform[] wheelMeshes;
    public float wheelSpinSpeed = 720f;
    // Start is called before the first frame update

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        // Spin the wheels
        foreach (var wheel in wheelMeshes)
        {
            wheel.Rotate(Vector3.right, wheelSpinSpeed * Time.deltaTime, Space.Self);
        }
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (v > 0 && rb.velocity.magnitude < maxSpeed) 
        {
            rb.AddForce(-transform.right * v * acceleration, ForceMode.Acceleration);
        }
        else if (v < 0)
        {
            rb.AddForce(transform.right * Mathf.Abs(v) * brakeForce, ForceMode.Acceleration);
        }

        if (Mathf.Abs(horizontal) > 0.05f)
        {
            var targetRot = Quaternion.Euler(0, horizontal * 15f, 0f);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.fixedDeltaTime * turnSmooth
            );
        }
    }
}
