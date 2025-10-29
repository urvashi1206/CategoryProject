using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarItemUI : MonoBehaviour
{
    [Header("Refs")]
    public Image icon;
    public TMP_Text nameText;
    public GameObject lockGroup;
    public TMP_Text lockText;
    public Image selectionOverlay;  // you can ignore if using frame/glow
    public Button button;

    [Header("Selected Visuals")]
    public Image background;        // card bg image
    public Image selectedFrame;     // new
    public Image selectedGlow;      // new
    public Color bgNormal = new Color(0.11f, 0.14f, 0.18f, 0.95f);
    public Color bgSelected = new Color(0.16f, 0.20f, 0.26f, 0.98f);
    [Range(1f, 1.2f)] public float selectedScale = 1.06f;

    [HideInInspector] public int carId;
    [HideInInspector] public bool isUnlocked;

    // Call with bestScore (old call)
    public void Setup(Sprite s, string display, int id, bool unlocked, int unlockScore, int bestScore)
    {
        InternalApply(s, display, id, unlocked, unlockScore, bestScore, true);
    }
    // Call without bestScore (lite call)
    public void Apply(Sprite s, string display, int id, bool unlocked, int unlockScore)
    {
        InternalApply(s, display, id, unlocked, unlockScore, 0, false);
    }

    void InternalApply(Sprite s, string display, int id, bool unlocked, int unlockScore, int bestScore, bool showProgress)
    {
        carId = id;
        isUnlocked = unlocked;

        if (icon) icon.sprite = s;
        if (nameText) nameText.text = display;

        if (lockGroup) lockGroup.SetActive(!unlocked);
        if (lockText)
            lockText.text = unlocked ? "" :
                (showProgress ? $"Unlock at {unlockScore}\n(You: {bestScore})" : $"Unlock at {unlockScore}");

        if (!button) button = GetComponent<Button>();
        if (button) button.interactable = unlocked;

        SetSelected(false, immediate: true);
    }

    public void SetSelected(bool on) => SetSelected(on, false);

    void SetSelected(bool on, bool immediate)
    {
        if (background) background.color = on ? bgSelected : bgNormal;
        if (selectedFrame) selectedFrame.enabled = on;
        if (selectedGlow) selectedGlow.enabled = on;
        if (selectionOverlay) selectionOverlay.enabled = on; // optional

        var target = on ? selectedScale : 1f;
        if (immediate) transform.localScale = Vector3.one * target;
        else
        {
            // simple tween substitute:
            StopAllCoroutines();
            StartCoroutine(ScaleTo(target, 0.12f));
        }
    }

    System.Collections.IEnumerator ScaleTo(float target, float time)
    {
        float t = 0f;
        Vector3 start = transform.localScale;
        Vector3 end = Vector3.one * target;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            transform.localScale = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        transform.localScale = end;
    }
}
