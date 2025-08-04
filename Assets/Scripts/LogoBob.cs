using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoBob : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Moves logo +8 px on Y over 2 sec, then back, forever
        LeanTween.moveLocalY(gameObject, transform.localPosition.y + 6f, 0.2f)
                 .setEaseInOutSine()
                 .setLoopPingPong();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
