using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 300f;
    public AudioClip deathClip;

    const float phase2Threshold = 0.5f;
    const float phase2SpeedBonus = 1.5f;
    const float phase2DamageBonus = 1.3f;
    const float deathDelay = 2f;

    public float CurrentHealth { get; private set; }
    public bool IsPhase2 { get; private set; }
    public bool IsDead { get; private set; }

    Animator anim;
    AudioSource audioSrc;
    PlayerController movement;
    PlayerCombat combat;

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

        if (!IsPhase2 && CurrentHealth / maxHealth <= phase2Threshold)
            TriggerPhase2();

        if (CurrentHealth <= 0f)
            StartCoroutine(Die());
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    public void ResetToFull()
    {
        if (IsDead) return;
        CurrentHealth = maxHealth;
    }

    void TriggerPhase2()
    {
        IsPhase2 = true;
        anim.SetTrigger("Phase2");
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
    }

    IEnumerator Die()
    {
        IsDead = true;
        anim.SetTrigger("Die");
        if (audioSrc != null && deathClip != null) audioSrc.PlayOneShot(deathClip);
        yield return new WaitForSeconds(deathDelay);
        if (GameManager.Instance != null) GameManager.Instance.BossDefeated();
    }
}
