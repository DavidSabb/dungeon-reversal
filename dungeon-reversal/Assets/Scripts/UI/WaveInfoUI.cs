using UnityEngine;
using TMPro;

public class WaveInfoUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI heroesText;

    WaveManager wm;

    void Start()
    {
        wm = FindObjectOfType<WaveManager>();
    }

    void Update()
    {
        int score = wm != null ? wm.Score : 0;
        int wave = wm != null ? wm.CurrentWave : 0;
        int totalWaves = wm != null ? wm.totalWaves : 0;
        int alive = wm != null ? wm.HeroesAlive : 0;

        if (scoreText != null) scoreText.text = "Score: " + score;
        if (waveText != null) waveText.text = "Wave: " + wave + "/" + totalWaves;
        if (heroesText != null) heroesText.text = "Heroes: " + alive;
    }
}
