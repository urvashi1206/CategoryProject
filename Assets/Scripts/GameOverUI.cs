using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;           // CanvasGroup on GameOverUI root
    public RectTransform card;          // Centered panel
    public TMP_Text finalScoreText;     // "Final Score: X"
    public TMP_Text bestText;           // "Best: Y"
    public Button restartButton;
    public Button quitButton;
    [Tooltip("Optional Main Menu button; leave null if you don't have one.")]
    public Button mainMenuButton;

    [Header("Style")]
    public float fadeTime = 0.25f;
    public float popTime = 0.28f;
    public float cardPopFrom = 0.85f;

    [Header("Scenes")]
    [Tooltip("Scene name for your start/menu screen.")]
    public string mainMenuSceneName = "Start";

    [Header("Tweening")]
    [Tooltip("If true, tweens will ignore Time.timeScale (so they animate even if you paused time).")]
    public bool ignoreTimeScale = false;

    const string BestKey = "BestScore";

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
    }

    void Awake
    ()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        HideImmediate();

        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (quitButton) quitButton.onClick.AddListener(Quit);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(GoToMenu);
    }

    /// <summary>
    /// Call this once when the round ends with the *final* score.
    /// </summary>
    public void Show(int finalScore)
    {
        // Read current best, update ONLY if the *final* score beats it.
        int best = PlayerPrefs.GetInt(BestKey, 0);
        if (finalScore > best)
        {
            best = finalScore;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save(); // force write so it persists immediately
        }

        if (finalScoreText) finalScoreText.text = $"Final Score: {finalScore}";
        if (bestText) bestText.text = $"Best: {best}";

        // Enable interaction & show
        gameObject.SetActive(true);
        group.blocksRaycasts = true;
        group.interactable = true;
        group.alpha = 0f;

        var fade = LeanTween.value(gameObject, 0f, 1f, fadeTime)
                             .setOnUpdate((float a) => group.alpha = a);
        if (ignoreTimeScale) fade.setIgnoreTimeScale(true);

        if (card)
        {
            card.localScale = Vector3.one * cardPopFrom;
            var pop = LeanTween.scale(card, Vector3.one, popTime).setEaseOutBack();
            if (ignoreTimeScale) pop.setIgnoreTimeScale(true);
        }
    }

    public void Hide()
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        var fade = LeanTween.value(gameObject, group.alpha, 0f, fadeTime)
                             .setOnUpdate((float a) => group.alpha = a)
                             .setOnComplete(() => gameObject.SetActive(false));
        if (ignoreTimeScale) fade.setIgnoreTimeScale(true);
    }

    void HideImmediate()
    {
        // Keep object active so references stay valid; just make it invisible/non-interactive
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        gameObject.SetActive(true);
    }

    // --- Buttons ---
    public void Restart()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
