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
    public WarningBannerUI bannerUI;
    public TMP_Text categoryLabel;

    [Header("Timer Settings")]
    public float roundDuration = 20f;
    public float warningDuration = 3f;
    private float roundTimer;
    private bool warningShown;

    [Header("SFX")]
    public AudioSource sfxCorrect;
    public AudioSource sfxWrong;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;
    [Range(0f, 1f)] public float correctVolume = 0.9f;
    [Range(0f, 2f)] public float wrongVolume = 0.9f;
    [Range(0.5f, 2f)] public float commonPitch = 1.15f;

    [Header("Scoring")]
    public int pointsPerCorrect = 10;
    public int wrongPenalty = -5;       // keep negative for clarity
    public int commonMultiplier = 2;    // x2 for common items

    public float wrongStartSec = 0.70f;

    private int score;

    void Awake()
    {
        GameEvents.OnCollect += OnCollect;
        if (correctSfx) correctSfx.LoadAudioData();
        if (wrongSfx) wrongSfx.LoadAudioData();
    }
    // Start is called before the first frame update
    void Start()
    {
        NextCategory();
        score = 0;
        scoreText.text = "Score: 0";
        if (bannerUI) bannerUI.gameObject.SetActive(false);
        roundTimer = roundDuration;
    }

    // Update is called once per frame
    void Update()
    {
        roundTimer -= Time.deltaTime;

        if (!warningShown && roundTimer <= warningDuration)
        {
            ShowWarningUI();
            warningShown = true;
        }

        if (warningShown && bannerUI)
            bannerUI.UpdateCountdown(roundTimer);

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

        if (isCorrect)
        {
            // if common item, award double
            bool isCommon = itm != null && itm.isCommon;
            int multiplier = isCommon ? commonMultiplier : 1;
            score += pointsPerCorrect * multiplier;

            if (sfxCorrect && correctSfx)
            {
                sfxCorrect.pitch = isCommon ? commonPitch : 1f;
                sfxCorrect.PlayOneShot(correctSfx, correctVolume);
                sfxCorrect.pitch = 1f;
            }
        }

        else
        {
            score += wrongPenalty;

            if (sfxWrong && wrongSfx)
            {
                sfxWrong.clip = wrongSfx;
                sfxWrong.volume = wrongVolume;

                int offsetSamples = Mathf.Clamp(
                    Mathf.FloorToInt(wrongSfx.frequency * wrongStartSec),
                    0, wrongSfx.samples - 1
                );
                sfxWrong.timeSamples = offsetSamples;
                sfxWrong.Play();
            }
        }

        scoreText.text = "Score: " + score.ToString();
    }

    void NextCategory()
    {
        currentCategoryIndex = (currentCategoryIndex + 1) % allCategories.Count;
        CurrentCategory = allCategories[currentCategoryIndex];
        categoryLabel.text = "Category: " + CurrentCategory.displayName;
        categoryLabel.color = CurrentCategory.uiColor;
    }

    void ShowWarningUI()
    {
        if (bannerUI) bannerUI.Show(roundTimer);
    }

    void HideWarningUI()
    {
        if (bannerUI) bannerUI.Hide();
    }

    public int GetScore()
    {
        return score;
    }
}
