using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// MainMenuManager.cs
/// Dungeon Reversal - Controls the main menu start screen.
/// Place on a GameObject in the MainMenu scene.
/// Hook buttons to the public methods via the Inspector.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scenes")]
    public string firstLevelScene = "Level1_Training";

    [Header("UI Panels")]
    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;

    [Header("Transition")]
    public float fadeInDuration = 1f;
    public CanvasGroup fadeCanvasGroup; // black overlay fading out on start

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        Time.timeScale   = 1f;

        ShowPanel(mainPanel);

        if (fadeCanvasGroup != null)
            StartCoroutine(FadeIn());
    }

    // ── Button Handlers ────────────────────────────────────────

    public void PlayGame()
    {
        StartCoroutine(LoadLevelWithFade(firstLevelScene));
    }

    public void OpenCredits()
    {
        ShowPanel(creditsPanel);
    }

    public void OpenSettings()
    {
        ShowPanel(settingsPanel);
    }

    public void BackToMain()
    {
        ShowPanel(mainPanel);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // ── Helpers ────────────────────────────────────────────────
    private void ShowPanel(GameObject target)
    {
        if (mainPanel     != null) mainPanel.SetActive(target == mainPanel);
        if (creditsPanel  != null) creditsPanel.SetActive(target == creditsPanel);
        if (settingsPanel != null) settingsPanel.SetActive(target == settingsPanel);
    }

    private IEnumerator FadeIn()
    {
        fadeCanvasGroup.alpha = 1f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            fadeCanvasGroup.alpha = 1f - (elapsed / fadeInDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator LoadLevelWithFade(string scene)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                fadeCanvasGroup.alpha = elapsed / fadeInDuration;
                elapsed += Time.deltaTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
        SceneManager.LoadScene(scene);
    }
}