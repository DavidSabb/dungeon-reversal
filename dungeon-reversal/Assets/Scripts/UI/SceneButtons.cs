using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtons : MonoBehaviour
{
    public string mainMenuScene = "MainMenu";
    public string gameplayScene = "DesertScene";
    public string nextLevelScene = "";

    void Awake()
    {
        Time.timeScale = 1f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplayScene);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextLevelScene)) SceneManager.LoadScene(nextLevelScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
