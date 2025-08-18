using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningBannerUI : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;       // assign the CanvasGroup on WarningBanner
    public RectTransform root;      // assign the WarningBanner RectTransform
    public TMP_Text warningText;    // assign WarningText
    public Image background;        // assign the Image on WarningBanner

    [Header("Style")]
    public Color bannerColor = new Color(0.96f, 0.62f, 0.04f, 0.85f); // #F59E0B
    public float targetY = -94f;
    public float showSlide = 30f;   // slide down distance on show
    public float pulseScale = 1.03f;
    public float pulseTime = 0.6f;

    int lastSeconds = -999;
    LTDescr pulseTween;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        if (!root) root = GetComponent<RectTransform>();
        if (!background) background = GetComponent<Image>();
        HideImmediate();
    }

    public void Show(float secondsLeft)
    {
        gameObject.SetActive(true);
        background.color = bannerColor;
        warningText.text = BuildText(Mathf.CeilToInt(secondsLeft));

        // reset pos & alpha
        group.alpha = 0f;
        root.anchoredPosition = new Vector2(0, targetY - showSlide);

        // fade + slide in
        LeanTween.value(gameObject, 0f, 1f, 0.25f).setOnUpdate(v => group.alpha = v);
        LeanTween.moveY(root, targetY, 0.25f).setEaseOutQuad();

        // start pulse
        StartPulse();
        lastSeconds = Mathf.CeilToInt(secondsLeft);
    }

    public void UpdateCountdown(float secondsLeft)
    {
        int s = Mathf.CeilToInt(secondsLeft);
        if (s != lastSeconds)
        {
            warningText.text = BuildText(s);
            lastSeconds = s;
        }
    }

    public void Hide()
    {
        StopPulse();
        // fade + slide up a touch
        LeanTween.value(gameObject, group.alpha, 0f, 0.22f).setOnUpdate(v => group.alpha = v)
            .setOnComplete(() => gameObject.SetActive(false));
        LeanTween.moveY(root, targetY - showSlide, 0.22f).setEaseInQuad();
    }

    void HideImmediate()
    {
        group.alpha = 0f;
        root.anchoredPosition = new Vector2(0, targetY - showSlide);
        gameObject.SetActive(false);
    }

    void StartPulse()
    {
        StopPulse();
        pulseTween = LeanTween.scale(root, Vector3.one * pulseScale, pulseTime)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    void StopPulse()
    {
        if (pulseTween != null) LeanTween.cancel(pulseTween.uniqueId);
        root.localScale = Vector3.one;
        pulseTween = null;
    }

    string BuildText(int s) => $"Heads up! New category in {Mathf.Max(0, s)}…";
}
