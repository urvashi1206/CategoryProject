using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Categories")]
    public List<CategorySO> allCategories;
    private int currentCategoryIndex = -1;
    public CategorySO CurrentCategory { get; private set; }

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text warningLabel;
    public TMP_Text categoryLabel;

    [Header("Timer Settings")]
    public float roundDuration = 60f;
    public float warningDuration = 5f;
    private float roundTimer;
    private bool warningShown;

    private int score;

    void Awake()
    {
        GameEvents.OnCollect += OnCollect;
    }
    // Start is called before the first frame update
    void Start()
    {
        NextCategory();
        score = 0;
        scoreText.text = "Score: 0";
        warningLabel.gameObject.SetActive(false);
        roundTimer = roundDuration;
    }

    // Update is called once per frame
    void Update()
    {
        roundTimer -= Time.deltaTime;

        if (!warningShown && roundTimer <= warningDuration)
        {
            ShowWarningUI("Category changing soon!");
            warningShown = true;
        }

        if (roundTimer <= 0)
        {
            NextCategory();
            roundTimer = roundDuration;
            warningShown = false;
            HideWarningUI();
        }
    }

    void OnCollect(ItemSO itm)
    {
        bool isCorrect = CurrentCategory.correctItems.Contains(itm);
        score += isCorrect ? 10 : -5;
        scoreText.text = "Score: " + score.ToString();
    }

    void NextCategory()
    {
        currentCategoryIndex = (currentCategoryIndex + 1) % allCategories.Count;
        CurrentCategory = allCategories[currentCategoryIndex];
        categoryLabel.text = "Category: " + CurrentCategory.displayName;
        categoryLabel.color = CurrentCategory.uiColor;
    }

    void ShowWarningUI(string msg)
    {
        warningLabel.text = msg;
        warningLabel.gameObject.SetActive(true);
        warningLabel.color = new Color(1f, 0.7f, 0f);
    }

    void HideWarningUI()
    {
        warningLabel.gameObject.SetActive(false);
    }

    public int GetScore()
    {
        return score;
    }
}
