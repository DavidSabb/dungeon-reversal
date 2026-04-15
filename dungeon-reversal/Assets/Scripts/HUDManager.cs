using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    public Slider healthSlider;
    public Image  healthFill;
    public Color  healthColorFull = Color.green;
    public Color  healthColorLow  = Color.red;
    public TextMeshProUGUI healthText;

    [Header("Special 1 Cooldown (E)")]
    public Image           special1Fill;
    public TextMeshProUGUI special1ReadyText;

    [Header("Special 2 Cooldown (Q)")]
    public Image           special2Fill;
    public TextMeshProUGUI special2ReadyText;

    [Header("Wave / Phase")]
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI phaseText;
    public GameObject      phase2Indicator;

    [Header("Lock-On")]
    public Image lockOnCrosshair;

    private PlayerHealth _playerHealth;
    private PlayerCombat _playerCombat;
    private LockOnSystem _lockOn;
    private WaveManager  _waveManager;

    private void Start()
    {
        _playerHealth = FindObjectOfType<PlayerHealth>();
        _playerCombat = FindObjectOfType<PlayerCombat>();
        _lockOn       = FindObjectOfType<LockOnSystem>();
        _waveManager  = FindObjectOfType<WaveManager>();

        if (_playerHealth != null)
        {
            _playerHealth.OnHealthChanged += UpdateHealth;
            _playerHealth.OnPhase2Begin   += ShowPhase2;
            UpdateHealth(_playerHealth.CurrentHealth, _playerHealth.maxHealth);
        }
        if (_waveManager != null)
            _waveManager.OnWaveStart += UpdateWave;
    }

    private void Update()
    {
        UpdateCooldowns();
        if (lockOnCrosshair != null && _lockOn != null)
            lockOnCrosshair.gameObject.SetActive(_lockOn.HasTarget);
    }

    private void UpdateHealth(float current, float max)
    {
        if (healthSlider != null) healthSlider.value = current / max;
        if (healthFill   != null) healthFill.color   = Color.Lerp(healthColorLow, healthColorFull, current / max);
        if (healthText   != null) healthText.text    = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    private void UpdateCooldowns()
    {
        if (_playerCombat == null) return;

        float r1 = Mathf.Clamp01(_playerCombat.Special1Current / _playerCombat.special1Cooldown);
        if (special1Fill      != null) special1Fill.fillAmount = r1;
        if (special1ReadyText != null) special1ReadyText.gameObject.SetActive(r1 <= 0f);

        float r2 = Mathf.Clamp01(_playerCombat.Special2Current / _playerCombat.special2Cooldown);
        if (special2Fill      != null) special2Fill.fillAmount = r2;
        if (special2ReadyText != null) special2ReadyText.gameObject.SetActive(r2 <= 0f);
    }

    private void UpdateWave(int wave)
    {
        if (waveText != null) waveText.text = $"Wave {wave}";
    }

    private void ShowPhase2()
    {
        if (phaseText       != null) phaseText.text = "PHASE 2";
        if (phase2Indicator != null) phase2Indicator.SetActive(true);
    }

    private void OnDestroy()
    {
        if (_playerHealth != null) { _playerHealth.OnHealthChanged -= UpdateHealth; _playerHealth.OnPhase2Begin -= ShowPhase2; }
        if (_waveManager  != null)   _waveManager.OnWaveStart -= UpdateWave;
    }
}