using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class RoundTimer : MonoBehaviour
{
    [Header("Timer")]
    public float roundDuration = 60f;
    private float timeLeft;
    private bool isRunning = true;

    [Header("Refs")]
    public TMP_Text timerText;          // "Time: 60"
    public GameManager gameManager;     // to read score
    public GameOverUI gameOverUI;       // << drag your GameOverUI here

    [Header("Stop on End")]
    public MonoBehaviour player; // drag your CarController (or ArcadeCarController)
    public Rigidbody carRigidbody;      // drag the car Rigidbody (to zero velocities)
    public GameObject spawner;          // item spawner root to disable

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = roundDuration;
        UpdateTimerUI();
    }

    // Update is called once per frame
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

    void EndGame()
    {
        if (player) player.enabled = false;
        if (carRigidbody)
        {
            carRigidbody.velocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
        }

        if (spawner) spawner.SetActive(false);
        // Show animated Game Over with score
        int finalScore = gameManager ? gameManager.GetScore() : 0;
        if (gameOverUI) gameOverUI.Show(finalScore);
        else Debug.LogWarning("GameOverUI reference not assigned on RoundTimer.");
    }
}
