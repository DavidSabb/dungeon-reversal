using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string mainMenuScene = "MainMenu";
    public string gameOverScene = "GameOverScene";
    public string victoryScene = "VictoryScene";
    public string nextLevelScene = "";

    public GameObject pauseMenuPanel;
    public GameObject hudPanel;

    public float gameOverDelay = 2f;
    public float victoryDelay = 2f;

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        Time.timeScale = 1f;
    }

    void Start()
    {
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) ph.OnDeath += () => StartCoroutine(TriggerGameOver());

        WaveManager wm = FindObjectOfType<WaveManager>();
        if (wm != null) wm.OnAllWavesComplete += () => StartCoroutine(TriggerVictory());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver) TogglePause();
    }

    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(IsPaused);
        if (hudPanel != null) hudPanel.SetActive(!IsPaused);
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsPaused;
    }

    public void Resume() { TogglePause(); }
    public void HeroesRetreated() { StartCoroutine(TriggerVictory()); }
    public void BossDefeated() { StartCoroutine(TriggerGameOver()); }

    IEnumerator TriggerGameOver()
    {
        if (IsGameOver) yield break;
        IsGameOver = true;
        yield return new WaitForSecondsRealtime(gameOverDelay);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(gameOverScene);
    }

    IEnumerator TriggerVictory()
    {
        yield return new WaitForSecondsRealtime(victoryDelay);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(victoryScene);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextLevelScene)) SceneManager.LoadScene(nextLevelScene);
    }

    public void QuitGame() { Application.Quit(); }
}
