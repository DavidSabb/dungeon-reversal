using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 300f;

    [Header("Phase 2")]
    [Range(0f, 1f)]
    public float phase2Threshold  = 0.5f;
    public float phase2SpeedBonus  = 1.5f;
    public float phase2DamageBonus = 1.3f;
    public GameObject phase2VFX;
    public AudioClip  phase2Clip;

    [Header("Death")]
    public float     deathDelay = 2f;
    public AudioClip deathClip;

    public float CurrentHealth { get; private set; }
    public bool  IsPhase2      { get; private set; }
    public bool  IsDead        { get; private set; }
    public int   CurrentPhase  => IsPhase2 ? 2 : 1;

    public System.Action<float, float> OnHealthChanged;
    public System.Action OnPhase2Begin;
    public System.Action OnDeath;

    private Animator         _anim;
    private AudioSource      _audio;
    private PlayerController _movement;
    private PlayerCombat     _combat;

    private static readonly int HashPhase2 = Animator.StringToHash("Phase2");
    private static readonly int HashDie    = Animator.StringToHash("Die");

    private void Awake()
    {
        _anim     = GetComponent<Animator>();
        _audio    = GetComponent<AudioSource>();
        _movement = GetComponent<PlayerController>();
        _combat   = GetComponent<PlayerCombat>();
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (!IsPhase2 && CurrentHealth / maxHealth <= phase2Threshold)
            StartCoroutine(TriggerPhase2());
        if (CurrentHealth <= 0f)
            StartCoroutine(Die());
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private IEnumerator TriggerPhase2()
    {
        IsPhase2 = true;
        _anim.SetTrigger(HashPhase2);
        if (phase2VFX != null) Destroy(Instantiate(phase2VFX, transform.position, Quaternion.identity), 4f);
        if (_audio != null && phase2Clip != null) _audio.PlayOneShot(phase2Clip);
        if (_movement != null) { _movement.walkSpeed *= phase2SpeedBonus; _movement.runSpeed *= phase2SpeedBonus; }
        if (_combat   != null) { _combat.meleeDamage *= phase2DamageBonus; _combat.special1Damage *= phase2DamageBonus; _combat.special2Damage *= phase2DamageBonus; }
        OnPhase2Begin?.Invoke();
        yield return null;
    }

    private IEnumerator Die()
    {
        IsDead = true;
        _anim.SetTrigger(HashDie);
        if (_audio != null && deathClip != null) _audio.PlayOneShot(deathClip);
        OnDeath?.Invoke();
        yield return new WaitForSeconds(deathDelay);
        GameManager.Instance?.BossDefeated();
    }
}