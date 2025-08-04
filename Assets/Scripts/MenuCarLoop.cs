using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCarLoop : MonoBehaviour
{
    [Header("Bounds")]
    public float leftX = -15f;   // far-left edge of the visible road
    public float rightX = 15f;   // far-right edge

    [Header("Speeds")]
    public float cruiseSpeed = 15f;  // speed while moving right
    public float returnSpeed = 15f;  // speed while sliding back left

    float direction = 1f;              // 1 = right, -1 = left (return)

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // choose velocity based on direction
        float speed = (direction > 0f) ? cruiseSpeed : returnSpeed;
        Vector3 step = Vector3.right * direction * speed * Time.deltaTime;
        transform.Translate(step, Space.World);

        // reached right edge → start swift return
        if (direction > 0f && transform.position.x >= rightX)
        {
            direction = -1f;                      // flip direction
        }
        // reached left edge → resume normal cruise to the right
        else if (direction < 0f && transform.position.x <= leftX)
        {
            direction = 1f;                       // flip again
        }
    }
}
