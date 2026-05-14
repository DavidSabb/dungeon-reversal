using UnityEngine;
using TMPro;

public class PhaseIndicatorUI : MonoBehaviour
{
    public TextMeshProUGUI label;
    public string phase1Text = "";
    public string phase2Text = "ENRAGED";

    PlayerHealth health;

    void Start()
    {
        health = FindObjectOfType<PlayerHealth>();
        if (label != null) label.text = phase1Text;
        if (health != null) health.OnPhase2Begin += ShowPhase2;
    }

    void OnDestroy()
    {
        if (health != null) health.OnPhase2Begin -= ShowPhase2;
    }

    void ShowPhase2()
    {
        if (label != null) label.text = phase2Text;
    }
}
