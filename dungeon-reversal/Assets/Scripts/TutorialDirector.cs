using UnityEngine;

[RequireComponent(typeof(WaveManager))]
public class TutorialDirector : MonoBehaviour
{
    WaveManager wm;
    PlayerHealth player;

    int lastWave;
    bool shownWin;
    string bannerText = "";
    float bannerTimer;

    void Start()
    {
        wm = GetComponent<WaveManager>();
        player = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (bannerTimer > 0f)
            bannerTimer -= Time.deltaTime;

        if (wm.CurrentWave != lastWave && wm.CurrentWave > 0)
        {
            lastWave = wm.CurrentWave;
            if (player != null) player.ResetToFull();
            bannerText = "WAVE " + wm.CurrentWave;
            bannerTimer = 2f;
        }

        if (wm.AllWavesDone && !shownWin)
        {
            shownWin = true;
            bannerText = "YOU WIN";
            bannerTimer = 2f;
        }
    }

    void OnGUI()
    {
        if (bannerTimer <= 0f) return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 60;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.white;

        Rect rect = new Rect(0f, Screen.height * 0.2f, Screen.width, 100f);
        GUI.Label(rect, bannerText, style);
    }
}
