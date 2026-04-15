using UnityEngine;

/// <summary>
/// HeroHealth.cs
/// Dungeon Reversal - Health component for the Crusader hero NPC.
/// Notifies HeroAI on death and reports kill to WaveManager.
/// Attach to same GameObject as HeroAI.
/// </summary>
public class HeroHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    [Header("Feedback")]
    public AudioClip hitClip;
    public AudioClip deathClip;
    public GameObject deathVFX;

    private HeroAI      _ai;
    private AudioSource _audio;
    private bool        _isDead;

    private void Awake()
    {
        _ai    = GetComponent<HeroAI>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (_audio != null && hitClip != null)
            _audio.PlayOneShot(hitClip);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        if (_audio != null && deathClip != null)
            _audio.PlayOneShot(deathClip);

        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        // Report kill to WaveManager
        WaveManager.Instance?.HeroKilled();

        // Tell AI to play death animation
        if (_ai != null) _ai.OnDeath();
    }
}