using UnityEngine;

public class VehicleMover : MonoBehaviour
{
    [Tooltip("Units/sec along -X")]
    public float speed = 8f;

    [Tooltip("Destroy when X <= this (world-space). Set from spawner or inspector).")]
    public float killAtX = -100f;

    void Update()
    {
        var p = transform.position;
        p.x += -speed * Time.deltaTime;
        transform.position = p;

        if (p.x <= killAtX) Destroy(gameObject);
    }
}
