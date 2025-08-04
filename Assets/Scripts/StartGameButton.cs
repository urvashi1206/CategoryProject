using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGameButton : MonoBehaviour
{
    [Tooltip("Exact name of your gameplay scene (case-sensitive).")]
    public string gameSceneName = "GameScene";

    public void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
