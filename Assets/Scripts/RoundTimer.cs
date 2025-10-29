//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.InputSystem.XR;

//public class RoundTimer : MonoBehaviour
//{
//    [Header("Timer")]
//    public float roundDuration = 60f;
//    private float timeLeft;
//    private bool isRunning = true;

//    [Header("Refs")]
//    public TMP_Text timerText;          // "Time: 60"
//    public GameManager gameManager;     // to read score
//    public GameOverUI gameOverUI;

//    [Header("Stop on End (auto-bound)")]
//    [Tooltip("Script that drives the car (left empty; auto-bound at runtime).")]
//    public MonoBehaviour player;
//    [Tooltip("Car's Rigidbody (left empty; auto-bound at runtime).")]
//    public Rigidbody carRigidbody;

//    [Header("Auto-bind settings")]
//    [Tooltip("Tag on your car prefab root. Set your car prefab's tag to 'Player'.")]
//    public string playerTag = "Player";
//    [Tooltip("Name of your controller script class on the car (e.g., 'CarController').")]
//    public string playerControllerTypeName = "CarController";
//    [Tooltip("Try to bind on Start if fields are empty.")]
//    public bool autoBindOnStart = true;

//    [Header("Other gameplay refs")]
//    public GameObject spawner;

//    [Header("Unlocks")]
//    public CarCatalogSO catalog;
//    [Tooltip("If ON, unlocks are based on the best-ever FINAL score; if OFF, use this run's final score only.")]
//    public bool unlockByBestEver = true;


//    // Start is called before the first frame update
//    void Start()
//    {
//        timeLeft = roundDuration;
//        UpdateTimerUI();

//        if (autoBindOnStart)
//            TryBindPlayer();
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (!isRunning) return;

//        timeLeft -= Time.deltaTime;

//        if (timeLeft <= 0f)
//        {
//            timeLeft = 0f;
//            isRunning = false;
//            EndGame();
//        }

//        UpdateTimerUI();
//    }

//    void UpdateTimerUI()
//    {
//        if (timerText)
//            timerText.text = "Time: " + Mathf.CeilToInt(timeLeft).ToString();
//    }

//    public void RegisterPlayer(MonoBehaviour controller, Rigidbody rb)
//    {
//        player = controller;
//        carRigidbody = rb;
//    }

//    public void TryBindPlayer()
//    {
//        if (player && carRigidbody) return;

//        var go = GameObject.FindGameObjectWithTag(playerTag);
//        if (!go)
//        {
//            Debug.LogWarning($"RoundTimer: No GameObject found with tag '{playerTag}'. " +
//                             "Set your car prefab root tag to 'Player'.");
//            return;
//        }

//        // Bind Rigidbody
//        if (!carRigidbody)
//        {
//            carRigidbody = go.GetComponent<Rigidbody>();
//            if (!carRigidbody)
//                Debug.LogWarning("RoundTimer: Found Player but it has no Rigidbody.");
//        }

//        // Bind controller script by type name (avoids compile deps on a specific class)
//        if (!player && !string.IsNullOrEmpty(playerControllerTypeName))
//        {
//            var comp = go.GetComponent(playerControllerTypeName);
//            player = comp as MonoBehaviour;
//            if (!player)
//                Debug.LogWarning($"RoundTimer: Could not find component '{playerControllerTypeName}' on Player.");
//        }
//    }

//    void EndGame()
//    {
//        if (!player || !carRigidbody) TryBindPlayer();

//        if (player) player.enabled = false;

//        if (carRigidbody)
//        {
//            carRigidbody.velocity = Vector3.zero;
//            carRigidbody.angularVelocity = Vector3.zero;
//        }

//        if (spawner) spawner.SetActive(false);

//        // Compute FINAL score of this run
//        int finalScore = gameManager ? gameManager.GetScore() : 0;

//        // Decide the score used for unlock gating
//        int scoreForUnlocks = finalScore;
//        if (unlockByBestEver)
//        {
//            int bestEver = PlayerPrefs.GetInt("BestScore", 0);
//            scoreForUnlocks = Mathf.Max(finalScore, bestEver);
//        }

//        // Unlock eligible cars (once per game end)
//        if (catalog)
//        {
//            var newlyUnlocked = CarUnlocks.UnlockEligible(catalog, scoreForUnlocks);
//            if (newlyUnlocked.Count > 0)
//            {
//                Debug.Log("Unlocked: " + string.Join(", ", newlyUnlocked));
//            }
//        }
//        else
//        {
//            Debug.LogWarning("RoundTimer: 'catalog' not assigned; unlock check skipped.");
//        }

//        // Show animated Game Over with the FINAL score (not peak)
//        if (gameOverUI) gameOverUI.Show(finalScore);
//        else Debug.LogWarning("RoundTimer: GameOverUI reference not assigned.");
//    }
//}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundTimer : MonoBehaviour
{
    [Header("Timer")]
    public float roundDuration = 60f;
    private float timeLeft;
    private bool isRunning = true;

    [Header("Refs")]
    public TMP_Text timerText;          // "Time: 60"
    public GameManager gameManager;     // to read score
    public GameOverUI gameOverUI;

    [Header("Stop on End (auto-bound)")]
    [Tooltip("Script that drives the car (left empty; auto-bound at runtime).")]
    public MonoBehaviour player;
    [Tooltip("Car's Rigidbody (left empty; auto-bound at runtime).")]
    public Rigidbody carRigidbody;

    [Header("Auto-bind settings")]
    [Tooltip("Tag on your car prefab root. Set your car prefab's tag to 'Player'.")]
    public string playerTag = "Player";
    [Tooltip("Name of your controller script class on the car (e.g., 'PlayerController').")]
    public string playerControllerTypeName = "PlayerController";
    [Tooltip("Try to bind on Start if fields are empty.")]
    public bool autoBindOnStart = true;

    [Header("Other gameplay refs")]
    public GameObject spawner; // your TrafficSpawner GameObject

    [Header("Unlocks")]
    public CarCatalogSO catalog;
    [Tooltip("If ON, unlocks are based on the best-ever FINAL score; if OFF, use this run's final score only.")]
    public bool unlockByBestEver = true;

    // ===== NEW: Traffic freeze settings =====
    [Header("Traffic Freeze on End")]
    [Tooltip("If ON, freeze all existing traffic cars when the round ends.")]
    public bool stopTrafficOnEnd = true;

    [Tooltip("Try to stop cars by finding ObstacleCarController components first.")]
    public bool stopByControllerComponent = true;

    [Tooltip("Fallback: also stop any Rigidbodies on these layers (e.g., a 'Vehicle' or 'Traffic' layer).")]
    public LayerMask trafficLayers;

    [Tooltip("Fallback: also stop any Rigidbodies with this tag (leave empty to skip).")]
    public string trafficTag = "Traffic";

    [Tooltip("Make traffic rigidbodies kinematic after stopping to avoid post-stop jitter.")]
    public bool makeTrafficKinematic = true;

    // =======================================

    void Start()
    {
        timeLeft = roundDuration;
        UpdateTimerUI();

        if (autoBindOnStart)
            TryBindPlayer();
    }

    void Update()
    {
        if (!isRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            isRunning = false;
            EndGame();
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText)
            timerText.text = "Time: " + Mathf.CeilToInt(timeLeft).ToString();
    }

    public void RegisterPlayer(MonoBehaviour controller, Rigidbody rb)
    {
        player = controller;
        carRigidbody = rb;
    }

    public void TryBindPlayer()
    {
        if (player && carRigidbody) return;

        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (!go)
        {
            Debug.LogWarning($"RoundTimer: No GameObject found with tag '{playerTag}'. " +
                             "Set your car prefab root tag to 'Player'.");
            return;
        }

        if (!carRigidbody)
        {
            carRigidbody = go.GetComponent<Rigidbody>();
            if (!carRigidbody)
                Debug.LogWarning("RoundTimer: Found Player but it has no Rigidbody.");
        }

        if (!player && !string.IsNullOrEmpty(playerControllerTypeName))
        {
            var comp = go.GetComponent(playerControllerTypeName);
            player = comp as MonoBehaviour;
            if (!player)
                Debug.LogWarning($"RoundTimer: Could not find component '{playerControllerTypeName}' on Player.");
        }
    }

    void EndGame()
    {
        // Ensure we have player refs
        if (!player || !carRigidbody) TryBindPlayer();

        // 1) Stop the PLAYER
        if (player) player.enabled = false;
        if (carRigidbody)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        // 2) Stop SPAWNING new traffic
        if (spawner) spawner.SetActive(false);

        // 3) NEW: Freeze EXISTING traffic
        if (stopTrafficOnEnd) StopAllTraffic();

        // 4) Compute FINAL score of this run
        int finalScore = gameManager ? gameManager.GetScore() : 0;

        // 5) Unlocks based on final or best-ever
        int scoreForUnlocks = finalScore;
        if (unlockByBestEver)
        {
            int bestEver = PlayerPrefs.GetInt("BestScore", 0);
            scoreForUnlocks = Mathf.Max(finalScore, bestEver);
        }
        if (catalog)
        {
            var newlyUnlocked = CarUnlocks.UnlockEligible(catalog, scoreForUnlocks);
            if (newlyUnlocked.Count > 0)
                Debug.Log("Unlocked: " + string.Join(", ", newlyUnlocked));
        }
        else
        {
            Debug.LogWarning("RoundTimer: 'catalog' not assigned; unlock check skipped.");
        }

        // 6) Show Game Over UI with THE final score
        if (gameOverUI) gameOverUI.Show(finalScore);
        else Debug.LogWarning("RoundTimer: GameOverUI reference not assigned.");
    }

    // ===== Helpers to stop traffic =====

    void StopAllTraffic()
    {
        int stoppedCount = 0;

        // A) Primary: stop by controller type (ObstacleCarController)
        if (stopByControllerComponent)
        {
            var controllers = FindObjectsOfType<ObstacleCarController>(true);
            foreach (var oc in controllers)
            {
                if (!oc) continue;
                // Stop logic
                oc.speed = 0f;
                oc.enabled = false;

                if (oc.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    if (makeTrafficKinematic) rb.isKinematic = true;
                }
                stoppedCount++;
            }
        }

        // B) Fallback: catch any RB on traffic layers or with traffic tag
        if (trafficLayers.value != 0 || !string.IsNullOrWhiteSpace(trafficTag))
        {
            var allRBs = FindObjectsOfType<Rigidbody>(true);
            foreach (var rb in allRBs)
            {
                if (!rb) continue;
                // skip the player RB
                if (carRigidbody && rb == carRigidbody) continue;

                bool layerMatch = (trafficLayers.value != 0) && ((trafficLayers.value & (1 << rb.gameObject.layer)) != 0);
                bool tagMatch = !string.IsNullOrWhiteSpace(trafficTag) && rb.CompareTag(trafficTag);

                if (!layerMatch && !tagMatch) continue;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                if (makeTrafficKinematic) rb.isKinematic = true;

                // If a controller exists, disable it too
                var oc = rb.GetComponent<ObstacleCarController>();
                if (oc) oc.enabled = false;

                stoppedCount++;
            }
        }

        if (stoppedCount == 0)
        {
            Debug.Log("[RoundTimer] StopAllTraffic() found nothing to freeze. Check traffic layers/tags or controller type.");
        }
    }
}