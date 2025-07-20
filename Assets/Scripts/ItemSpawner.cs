using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    private float currentX = -20f;
    public float xOffset = 10f;
    public float spawnInterval = 2f;

    CategorySO currentCategory;
    readonly List<ItemSO> wrongItems = new();
    float timer;

    float nextX;

    public void SetCategory(CategorySO cat)
    {
        currentCategory = cat;
        wrongItems.Clear();
        foreach (ItemSO itm in Resources.LoadAll<ItemSO>("Data/Items"))
        {
            if (!currentCategory.correctItems.Contains(itm))
                wrongItems.Add(itm);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        nextX = currentX;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentCategory == null) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        // 50 % chance correct vs filler
        bool correct = Random.value < 0.5f;

        // Before calling Random.Range, be sure the list isn't empty
        if (correct && currentCategory.correctItems.Count == 0) return;
        if (!correct && wrongItems.Count == 0) return;


        ItemSO pick = correct
            ? currentCategory.correctItems[Random.Range(0, currentCategory.correctItems.Count)]
            : wrongItems[Random.Range(0, wrongItems.Count)];

        Vector3 spawnPos = new Vector3(
                            nextX,
                            pick.prefab.transform.position.y,
                            pick.prefab.transform.position.z
                            );

        Instantiate(pick.prefab, spawnPos, pick.prefab.transform.rotation, transform);
        nextX -= xOffset;
    }
}
