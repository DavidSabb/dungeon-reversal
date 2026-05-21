using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public string firstLevelScene = "DesertScene";

    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(firstLevelScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
