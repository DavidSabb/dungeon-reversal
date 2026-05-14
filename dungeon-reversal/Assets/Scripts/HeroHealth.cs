using UnityEngine;

public class HeroHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth { get; private set; }

    public AudioClip hitClip;
    public AudioClip deathClip;
    public GameObject deathVFX;

    HeroAI ai;
    AudioSource audioSrc;
    bool isDead;

    void Awake()
    {
        ai = GetComponent<HeroAI>();
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        currentHealth = maxHealth;
    }

    public void ResetToMax()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (audioSrc != null && hitClip != null) audioSrc.PlayOneShot(hitClip);
        if (currentHealth <= 0f) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (audioSrc != null && deathClip != null) audioSrc.PlayOneShot(deathClip);

        if (deathVFX != null)
        {
            GameObject vfx = Instantiate(deathVFX, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (WaveManager.Instance != null) WaveManager.Instance.HeroKilled();
        if (ai != null) ai.OnDeath();
    }
}
