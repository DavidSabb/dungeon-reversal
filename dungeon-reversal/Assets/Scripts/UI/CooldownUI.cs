using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CooldownUI : MonoBehaviour
{
    public enum Slot { Special1, Special2 }
    public Slot slot = Slot.Special1;

    public Image fill;
    public TextMeshProUGUI readyLabel;
    public TextMeshProUGUI timerLabel;

    PlayerCombat combat;

    void Start()
    {
        combat = FindObjectOfType<PlayerCombat>();
    }

    void Update()
    {
        if (combat == null) return;

        float remaining = slot == Slot.Special1 ? combat.Special1Current : combat.Special2Current;
        float total = slot == Slot.Special1 ? combat.special1Cooldown : combat.special2Cooldown;
        float ratio = total > 0f ? Mathf.Clamp01(remaining / total) : 0f;

        if (fill != null) fill.fillAmount = ratio;

        bool ready = remaining <= 0f;
        if (readyLabel != null) readyLabel.gameObject.SetActive(ready);
        if (timerLabel != null)
        {
            timerLabel.gameObject.SetActive(!ready);
            if (!ready) timerLabel.text = remaining.ToString("0.0") + "s";
        }
    }
}
