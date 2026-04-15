using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// GameManager.cs
/// Dungeon Reversal - Central game state controller.
/// Handles win/lose conditions, scene transitions, pause.
/// Place on a persistent GameObject in the scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scenes")]
    public string mainMenuScene = "MainMenu";
    public string nextLevelScene = "";  // set per level

    [Header("UI Panels — assign in Inspector")]
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    public GameObject victoryPanel;
    public GameObject hudPanel;

    [Header("Timing")]
    public float gameOverDelay = 2f;
    public float victoryDelay  = 2f;

    // State
    public bool IsPaused   { get; private set; }
    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        Time.timeScale = 1f;
        SetPanels(hud: true, pause: false, over: false, victory: false);

        // Subscribe to player death
        PlayerHealth ph = FindObjectOfType<PlayerHealth>();
        if (ph != null) ph.OnDeath += () => StartCoroutine(TriggerGameOver());

        // Subscribe to all waves cleared
        WaveManager wm = FindObjectOfType<WaveManager>();
        if (wm != null) wm.OnAllWavesComplete += () => StartCoroutine(TriggerVictory());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver)
            TogglePause();
    }

    // ── Pause ──────────────────────────────────────────────────
    public void TogglePause()
    {
        IsPaused = !IsPaused;
        Time.timeScale = IsPaused ? 0f : 1f;
        SetPanels(hud: !IsPaused, pause: IsPaused, over: false, victory: false);
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = IsPaused;
    }

    public void Resume() => TogglePause();

    // ── Win / Lose ─────────────────────────────────────────────

    /// <summary>All waves survived — player wins this level.</summary>
    public void HeroesRetreated()
    {
        StartCoroutine(TriggerVictory());
    }

    /// <summary>Boss HP hit 0 — player loses.</summary>
    public void BossDefeated()
    {
        StartCoroutine(TriggerGameOver());
    }

    private IEnumerator TriggerGameOver()
    {
        if (IsGameOver) yield break;
        IsGameOver = true;
        yield return new WaitForSeconds(gameOverDelay);
        Time.timeScale = 0f;
        SetPanels(hud: false, pause: false, over: true, victory: false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private IEnumerator TriggerVictory()
    {
        yield return new WaitForSeconds(victoryDelay);
        Time.timeScale = 0f;
        SetPanels(hud: false, pause: false, over: false, victory: true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    // ── Button Callbacks (hook to UI buttons) ──────────────────
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
        if (!string.IsNullOrEmpty(nextLevelScene))
            SceneManager.LoadScene(nextLevelScene);
        else
            Debug.LogWarning("GameManager: No next level scene assigned!");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ── Helpers ────────────────────────────────────────────────
    private void SetPanels(bool hud, bool pause, bool over, bool victory)
    {
        if (hudPanel     != null) hudPanel.SetActive(hud);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(pause);
        if (gameOverPanel  != null) gameOverPanel.SetActive(over);
        if (victoryPanel   != null) victoryPanel.SetActive(victory);
    }
}