using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public TextMeshProUGUI label;

    readonly Color fullColor = new Color(0.2f, 0.85f, 0.2f);
    readonly Color lowColor = new Color(0.85f, 0.15f, 0.15f);

    PlayerHealth health;

    void Start()
    {
        health = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (health == null) return;

        float pct = health.maxHealth > 0f ? health.CurrentHealth / health.maxHealth : 0f;
        if (slider != null) slider.value = pct;
        if (fill != null) fill.color = Color.Lerp(lowColor, fullColor, pct);
        if (label != null) label.text = "HP " + Mathf.CeilToInt(health.CurrentHealth) + " / " + Mathf.CeilToInt(health.maxHealth);
    }
}
