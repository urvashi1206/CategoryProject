using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralRoadGenerator : MonoBehaviour
{
    public GameObject roadSegmentPrefab;
    public Transform player;

    public int initialSegments = 5;
    public float segmentLength = 40f;
    public float spawnDistanceAhead = 120f;
    public float destroyDistanceBehind = 0f;

    private float lastSpawnX;
    private List<GameObject> spawnedSegments = new();

    // Start is called before the first frame update
    void Start()
    {
        lastSpawnX = 0;

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnNextSegment();
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Keep spawning ahead
        while (player.position.x - lastSpawnX < spawnDistanceAhead)
        {
            SpawnNextSegment();
        }

        // Optionally destroy far-behind segments
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            GameObject seg = spawnedSegments[i];
            float segmentX = seg.transform.position.x;

            if (segmentX - player.position.x > destroyDistanceBehind)
            {
                Destroy(seg);
                spawnedSegments.RemoveAt(i);
            }
        }
    }

    void SpawnNextSegment()
    {
        Vector3 spawnPos = new Vector3(lastSpawnX, 0, 0);
        GameObject newSeg = Instantiate(roadSegmentPrefab, spawnPos, Quaternion.identity);
        spawnedSegments.Add(newSeg);
        lastSpawnX -= segmentLength;
    }
}
