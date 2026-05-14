using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public TextMeshProUGUI label;
    public Color fullColor = new Color(0.2f, 0.85f, 0.2f);
    public Color lowColor = new Color(0.85f, 0.15f, 0.15f);

    PlayerHealth health;

    void Start()
    {
        health = FindObjectOfType<PlayerHealth>();
        if (health == null) return;
        health.OnHealthChanged += Refresh;
        Refresh(health.CurrentHealth, health.maxHealth);
    }

    void OnDestroy()
    {
        if (health != null) health.OnHealthChanged -= Refresh;
    }

    void Refresh(float current, float max)
    {
        float pct = max > 0f ? current / max : 0f;
        if (slider != null) slider.value = pct;
        if (fill != null) fill.color = Color.Lerp(lowColor, fullColor, pct);
        if (label != null) label.text = "HP " + Mathf.CeilToInt(current) + " / " + Mathf.CeilToInt(max);
    }
}
