using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 300f;

    [Range(0f, 1f)]
    public float phase2Threshold = 0.5f;
    public float phase2SpeedBonus = 1.5f;
    public float phase2DamageBonus = 1.3f;

    public float deathDelay = 2f;
    public AudioClip deathClip;

    public float CurrentHealth { get; private set; }
    public bool IsPhase2 { get; private set; }
    public bool IsDead { get; private set; }

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnPhase2Begin;
    public System.Action OnDeath;

    Animator anim;
    AudioSource audioSrc;
    PlayerController movement;
    PlayerCombat combat;

    static readonly int HashPhase2 = Animator.StringToHash("Phase2");
    static readonly int HashDie = Animator.StringToHash("Die");

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        movement = GetComponent<PlayerController>();
        combat = GetComponent<PlayerCombat>();
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (!IsPhase2 && CurrentHealth / maxHealth <= phase2Threshold)
            TriggerPhase2();

        if (CurrentHealth <= 0f)
            StartCoroutine(Die());
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    void TriggerPhase2()
    {
        IsPhase2 = true;
        anim.SetTrigger(HashPhase2);
        if (movement != null)
        {
            movement.walkSpeed *= phase2SpeedBonus;
            movement.runSpeed *= phase2SpeedBonus;
        }
        if (combat != null)
        {
            combat.meleeDamage *= phase2DamageBonus;
            combat.special1Damage *= phase2DamageBonus;
            combat.special2Damage *= phase2DamageBonus;
        }
        OnPhase2Begin?.Invoke();
    }

    IEnumerator Die()
    {
        IsDead = true;
        anim.SetTrigger(HashDie);
        if (audioSrc != null && deathClip != null) audioSrc.PlayOneShot(deathClip);
        OnDeath?.Invoke();
        yield return new WaitForSeconds(deathDelay);
        if (GameManager.Instance != null) GameManager.Instance.BossDefeated();
    }
}
