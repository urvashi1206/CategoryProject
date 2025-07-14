using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManagerMeasurement : MonoBehaviour
{
    public CategorySO measurementCategory;
    public TMP_Text scoreText;

    int score;
    ItemSpawner spawner;

    void Awake()
    {
        spawner = GetComponent<ItemSpawner>();
        spawner.SetCategory(measurementCategory);
        GameEvents.OnCollect += OnCollect;
        scoreText.text = "Score: 0";
    }

    void OnCollect(ItemSO itm)
    {
        score += itm ? 10 : -5;
        scoreText.text = "Score: " + score.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
