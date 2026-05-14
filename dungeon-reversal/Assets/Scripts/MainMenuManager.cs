using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public string firstLevelScene = "DesertScene";

    public GameObject mainPanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;

    public float fadeInDuration = 1f;
    public CanvasGroup fadeCanvasGroup;

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
        ShowPanel(mainPanel);
        if (fadeCanvasGroup != null) StartCoroutine(FadeIn());
    }

    public void PlayGame()
    {
        StartCoroutine(LoadWithFade(firstLevelScene));
    }

    public void OpenCredits() { ShowPanel(creditsPanel); }
    public void OpenSettings() { ShowPanel(settingsPanel); }
    public void BackToMain() { ShowPanel(mainPanel); }
    public void QuitGame() { Application.Quit(); }

    void ShowPanel(GameObject target)
    {
        if (mainPanel != null) mainPanel.SetActive(target == mainPanel);
        if (creditsPanel != null) creditsPanel.SetActive(target == creditsPanel);
        if (settingsPanel != null) settingsPanel.SetActive(target == settingsPanel);
    }

    IEnumerator FadeIn()
    {
        fadeCanvasGroup.alpha = 1f;
        float t = 0f;
        while (t < fadeInDuration)
        {
            fadeCanvasGroup.alpha = 1f - (t / fadeInDuration);
            t += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    IEnumerator LoadWithFade(string scene)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            float t = 0f;
            while (t < fadeInDuration)
            {
                fadeCanvasGroup.alpha = t / fadeInDuration;
                t += Time.deltaTime;
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }
        SceneManager.LoadScene(scene);
    }
}
