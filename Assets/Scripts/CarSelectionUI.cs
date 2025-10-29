using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectionUI : MonoBehaviour
{
    [Header("Data")]
    public CarCatalogSO catalog;

    [Header("UI")]
    public Transform gridParent;      // CarScroll/Viewport/Content
    public CarItemUI itemPrefab;      // CarItem prefab
    public Button playButton;
    public TMP_Text titleText;        // optional: set to "Choose Your Car"
    public TMP_Text bestText;         // optional: show Best: 0

    [Header("Scene")]
    public string gameplaySceneName = "Game";

    List<CarItemUI> items = new();
    int selectedId = -1;

    void Start()
    {
        CarUnlocks.InitDefaults(catalog); // ensures defaults unlocked

        int best = PlayerPrefs.GetInt("BestScore", 0);
        if (bestText) bestText.text = $"Best: {best}";

        BuildGrid(best);

        // auto-select saved or first unlocked
        int saved = CarUnlocks.GetSelectedCarId();
        if (!CarUnlocks.IsUnlocked(saved))
            saved = CarUnlocks.GetFirstUnlockedId(catalog);

        Select(saved);

        if (playButton) playButton.onClick.AddListener(Play);
    }

    void BuildGrid(int bestScore)
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
        items.Clear();

        foreach (var skin in catalog.cars)
        {
            if (!skin) continue;

            var ui = Instantiate(itemPrefab, gridParent);
            bool unlocked = CarUnlocks.IsUnlocked(skin.id) || skin.defaultUnlocked;

            ui.Setup(
                skin.icon,
                skin.displayName,
                skin.id,
                unlocked,
                skin.unlockScore,
                bestScore
            );

            var btn = ui.button ? ui.button : ui.GetComponent<Button>();
            if (btn)
            {
                int idCopy = skin.id;
                btn.onClick.AddListener(() => OnClickItem(idCopy));
            }

            items.Add(ui);
        }
    }

    void OnClickItem(int id)
    {
        if (!CarUnlocks.IsUnlocked(id)) return;
        Select(id);
    }

    void Select(int id)
    {
        selectedId = id;
        CarUnlocks.SetSelectedCarId(id);
        foreach (var it in items) it.SetSelected(it.carId == id);

        if (playButton) playButton.interactable = (selectedId >= 0);
    }

    void Play()
    {
        if (selectedId < 0)
            selectedId = CarUnlocks.GetFirstUnlockedId(catalog);

        SceneManager.LoadScene(gameplaySceneName);
    }
}
