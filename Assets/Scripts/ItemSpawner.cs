using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float xOffset = 0f;
    private float currentX = -120f;
    private float nextX;
    private float timer;

    private List<ItemSO> allItems;

    [SerializeField] public ItemDatabaseSO itemDatabase;

    // Start is called before the first frame update
    void Start()
    {
        nextX = currentX;
        allItems = new List<ItemSO>(itemDatabase.allItems);
    }

    // Update is called once per frame
    void Update()
    {
        if (allItems == null || allItems.Count == 0) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        timer = 0f;
        ItemSO pick = allItems[Random.Range(0, allItems.Count)];

        Vector3 spawnPos = new Vector3(
            nextX,
            pick.prefab.transform.position.y,
            pick.prefab.transform.position.z
        );

        Instantiate(pick.prefab, spawnPos, pick.prefab.transform.rotation, transform);
        nextX -= xOffset;
    }
}
