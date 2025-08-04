using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioStartAtTime : MonoBehaviour
{
    [Tooltip("Seconds from clip start to begin playback")]
    public float startTime = 7.25f;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource src = GetComponent<AudioSource>();

        // clamp to clip length
        startTime = Mathf.Clamp(startTime, 0f, src.clip.length);

        src.time = startTime;        // jump to desired point
        src.Play();                  // NOW start playback
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
