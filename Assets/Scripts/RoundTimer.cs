using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoundTimer : MonoBehaviour
{
    public float roundDuration = 60f;
    private float timeLeft;
    private bool isRunning = true;

    public TMP_Text timerText;
    public GameObject gameOverPanel;
    public TMP_Text finalScoreText;
    public GameManager gameManager;
    public GameObject player;
    public GameObject spawner;

    // Start is called before the first frame update
    void Start()
    {
        timeLeft = roundDuration;
        UpdateTimerUI();
        gameOverPanel.SetActive(false);
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
        timerText.text = "Time: " + Mathf.CeilToInt(timeLeft).ToString();
    }

    void EndGame()
    {
        Debug.Log("Time’s up! Showing Game Over screen.");

        // Show game over panel
        gameOverPanel.SetActive(true);

        // Show final score
        finalScoreText.text = "Final Score: " + gameManager.GetScore().ToString();

        // Stop car and spawner
        if (player.TryGetComponent<CarController>(out var car))
        {
            car.enabled = false;
        }

        if (spawner != null)
        {
            spawner.SetActive(false);
        }
    }
}
