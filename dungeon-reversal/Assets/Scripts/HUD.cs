using UnityEngine;

public class HUD : MonoBehaviour
{
    [Header("Refs (auto-found if empty)")]
    public PlayerHealth playerHealth;
    public PlayerCombat playerCombat;

    [Header("Style")]
    public Color healthColor = new Color(0.85f, 0.15f, 0.15f);
    public Color textColor   = Color.white;

    private GUIStyle _text;
    private Texture2D _white;

    private void Awake()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerCombat == null) playerCombat = FindObjectOfType<PlayerCombat>();

        _white = new Texture2D(1, 1);
        _white.SetPixel(0, 0, Color.white);
        _white.Apply();
    }

    private void EnsureStyles()
    {
        if (_text != null) return;
        _text = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            normal   = { textColor = textColor }
        };
    }

    private void OnGUI()
    {
        EnsureStyles();
        DrawHealth();
        DrawCooldowns();
        DrawScore();
        DrawControls();
    }

    private void DrawHealth()
    {
        if (playerHealth == null) return;

        Rect bar = new Rect(20f, 20f, 240f, 14f);
        DrawRect(bar, new Color(0f, 0f, 0f, 0.5f));

        float pct = playerHealth.maxHealth > 0f
            ? Mathf.Clamp01(playerHealth.CurrentHealth / playerHealth.maxHealth)
            : 0f;
        DrawRect(new Rect(bar.x, bar.y, bar.width * pct, bar.height), healthColor);

        string txt = $"HP {Mathf.CeilToInt(playerHealth.CurrentHealth)}/{Mathf.CeilToInt(playerHealth.maxHealth)}";
        if (playerHealth.IsPhase2) txt += "  ENRAGED";
        GUI.Label(new Rect(20f, 36f, 400f, 20f), txt, _text);
    }

    private void DrawCooldowns()
    {
        if (playerCombat == null) return;
        GUI.Label(new Rect(20f, 56f, 400f, 20f), CooldownText("E", playerCombat.Special1Current), _text);
        GUI.Label(new Rect(20f, 74f, 400f, 20f), CooldownText("Q", playerCombat.Special2Current), _text);
    }

    private string CooldownText(string key, float remaining)
        => remaining <= 0f ? $"{key}: Ready" : $"{key}: {remaining:0.0}s";

    private void DrawScore()
    {
        WaveManager wm = WaveManager.Instance;
        int score      = wm != null ? wm.Score : 0;
        int wave       = wm != null ? wm.CurrentWave : 0;
        int totalWaves = wm != null ? wm.totalWaves : 0;
        int alive      = wm != null ? wm.HeroesAlive : 0;

        float x = Screen.width - 220f;
        GUI.Label(new Rect(x, 20f, 200f, 20f), $"Score: {score}",            _text);
        GUI.Label(new Rect(x, 38f, 200f, 20f), $"Wave: {wave}/{totalWaves}", _text);
        GUI.Label(new Rect(x, 56f, 200f, 20f), $"Heroes: {alive}",           _text);
    }

    private static readonly string[] _controls =
    {
        "WASD - Move",
        "Shift - Sprint",
        "Space - Jump",
        "L-Click - Melee",
        "E - Shockwave",
        "Q - Ground Smash",
    };

    private void DrawControls()
    {
        float x = Screen.width - 220f;
        float y = Screen.height - 20f - _controls.Length * 18f;
        for (int i = 0; i < _controls.Length; i++)
            GUI.Label(new Rect(x, y + i * 18f, 200f, 20f), _controls[i], _text);
    }

    private void DrawRect(Rect r, Color c)
    {
        Color prev = GUI.color;
        GUI.color = c;
        GUI.DrawTexture(r, _white);
        GUI.color = prev;
    }
}
