using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class GameOverUI : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;
    public RectTransform card;
    public TMP_Text finalScoreText;
    public TMP_Text bestText;
    public Button restartButton;
    public Button quitButton;
    [Tooltip("Optional Car Select / Main Menu button; leave null if you don't have one.")]
    public Button skinsButton;

    [Header("Style")]
    public float fadeTime = 0.25f;
    public float popTime = 0.28f;
    public float cardPopFrom = 0.85f;

    [Header("Scenes")]
    [Tooltip("Exact scene name (as in Build Settings) for your CarSelect/menu screen.")]
    public string skinSceneName = "CarSelect";

    [Header("Tweening")]
    [Tooltip("If true, tweens will ignore Time.timeScale (so they animate even if you paused time).")]
    public bool ignoreTimeScale = false;

    const string BestKey = "BestScore";

    // LeanTween ids so we can cancel if Show/Hide called repeatedly
    int _fadeId = -1;
    int _popId = -1;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        HideImmediate();
    }

    void OnEnable()
    {
        if (restartButton) restartButton.onClick.AddListener(Restart);
        if (quitButton) quitButton.onClick.AddListener(Quit);
        if (skinsButton) skinsButton.onClick.AddListener(GoToSkins);
    }

    void OnDisable()
    {
        if (restartButton) restartButton.onClick.RemoveListener(Restart);
        if (quitButton) quitButton.onClick.RemoveListener(Quit);
        if (skinsButton) skinsButton.onClick.RemoveListener(GoToSkins);
        CancelTweens();
    }

    void CancelTweens()
    {
        if (_fadeId != -1) { LeanTween.cancel(_fadeId); _fadeId = -1; }
        if (_popId != -1) { LeanTween.cancel(_popId); _popId = -1; }
    }

    /// <summary>
    /// Call this once when the round ends with the *final* score.
    /// </summary>
    public void Show(int finalScore)
    {
        if (!group) group = GetComponent<CanvasGroup>();
        CancelTweens();

        // Best score update once at game over
        int best = PlayerPrefs.GetInt(BestKey, 0);
        if (finalScore > best)
        {
            best = finalScore;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();
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
        _fadeId = fade.id;

        if (card)
        {
            card.localScale = Vector3.one * cardPopFrom;
            var pop = LeanTween.scale(card, Vector3.one, popTime).setEaseOutBack();
            if (ignoreTimeScale) pop.setIgnoreTimeScale(true);
            _popId = pop.id;
        }
    }

    public void Hide()
    {
        if (!group) return;
        group.interactable = false;
        group.blocksRaycasts = false;

        CancelTweens();

        var fade = LeanTween.value(gameObject, group.alpha, 0f, fadeTime)
                             .setOnUpdate((float a) => group.alpha = a)
                             .setOnComplete(() => gameObject.SetActive(false));
        if (ignoreTimeScale) fade.setIgnoreTimeScale(true);
        _fadeId = fade.id;
    }

    void HideImmediate()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        gameObject.SetActive(true); // keep active so references stay valid
    }

    // --- Buttons ---
    public void Restart()
    {
        Time.timeScale = 1f;
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void GoToSkins()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(skinSceneName))
        {
            Debug.LogWarning("[GameOverUI] skinSceneName is empty. Set it to your CarSelect/menu scene name.");
            return;
        }

        // This check helps diagnose why the scene might not be loading
        if (!Application.CanStreamedLevelBeLoaded(skinSceneName))
        {
            Debug.LogWarning($"[GameOverUI] Scene '{skinSceneName}' can’t be loaded. " +
                             "Make sure it’s added to Build Settings (File → Build Settings → Scenes In Build) " +
                             "and the name matches exactly.");
            return;
        }

        SceneManager.LoadSceneAsync(skinSceneName);
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