using UnityEngine;
using TMPro;

public class PhaseIndicatorUI : MonoBehaviour
{
    public TextMeshProUGUI label;

    const string phase1Text = "";
    const string phase2Text = "ENRAGED";

    PlayerHealth health;

    void Start()
    {
        health = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (health == null || label == null) return;

        if (health.IsPhase2)
            label.text = phase2Text;
        else
            label.text = phase1Text;
    }
}
