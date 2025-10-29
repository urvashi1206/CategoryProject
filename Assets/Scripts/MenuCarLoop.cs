//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MenuCarLoop : MonoBehaviour
//{
//    [Header("Bounds")]
//    public float leftX = -15f;   // far-left edge of the visible road
//    public float rightX = 15f;   // far-right edge

//    [Header("Speeds")]
//    public float cruiseSpeed = 15f;  // speed while moving right
//    public float returnSpeed = 15f;  // speed while sliding back left

//    float direction = 1f;              // 1 = right, -1 = left (return)

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // choose velocity based on direction
//        float speed = (direction > 0f) ? cruiseSpeed : returnSpeed;
//        Vector3 step = Vector3.right * direction * speed * Time.deltaTime;
//        transform.Translate(step, Space.World);

//        // reached right edge → start swift return
//        if (direction > 0f && transform.position.x >= rightX)
//        {
//            direction = -1f;                      // flip direction
//        }
//        // reached left edge → resume normal cruise to the right
//        else if (direction < 0f && transform.position.x <= leftX)
//        {
//            direction = 1f;                       // flip again
//        }
//    }
//}


using UnityEngine;

public class MenuCarLoop : MonoBehaviour
{
    [Header("Sway (left-right)")]
    public float centerX = 0f;          // road/lane center X in WORLD space
    public float swayAmplitude = 10f;    // how far left/right
    public float swayPeriod = 2.0f;     // seconds per full sway cycle
    public bool useRealtime = true;

    // anchors
    private Vector3 startPos;         // initial Y/Z preserved
    private float startRealTime;
    private float startGameTime;
    private float phaseOffset;

    void Awake()
    {
        startPos = transform.position;
        startRealTime = Time.realtimeSinceStartup;
        startGameTime = Time.time;
        phaseOffset = Random.Range(0f, Mathf.PI * 2f); // helps when multiple cars exist
    }

    void Update()
    {
        // recorder-proof clock if desired
        float t = useRealtime ? (Time.realtimeSinceStartup - startRealTime)
                              : (Time.time - startGameTime);

        float omega = (Mathf.PI * 2f) / Mathf.Max(0.0001f, swayPeriod);
        float x = centerX + Mathf.Sin(t * omega + phaseOffset) * swayAmplitude;

        // keep original Y/Z, only move X
        transform.position = new Vector3(x, startPos.y, startPos.z);
    }
}