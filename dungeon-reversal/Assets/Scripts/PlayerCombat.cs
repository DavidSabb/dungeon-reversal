using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    public float meleeDamage = 40f;
    public float special1Damage = 60f;
    public float special2Damage = 80f;
    public float special1Cooldown = 5f;
    public float special2Cooldown = 10f;

    public LayerMask enemyLayer;
    public Transform weaponTip;

    public AudioClip meleeSwingClip;
    public AudioClip special1Clip;
    public AudioClip special2Clip;

    const string enemyTag = "Hero";
    const float meleeRange = 2.5f;
    const float weaponHitRadius = 3.5f;
    const float special1Range = 5f;
    const float special2Range = 7f;

    float sp1Timer;
    float sp2Timer;

    public bool IsAttacking { get; private set; }
    public float Special1Current { get { return sp1Timer; } }
    public float Special2Current { get { return sp2Timer; } }

    Animator anim;
    AudioSource audioSrc;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (sp1Timer > 0f) sp1Timer -= Time.deltaTime;
        if (sp2Timer > 0f) sp2Timer -= Time.deltaTime;
        if (IsAttacking) return;

        if (Input.GetMouseButtonDown(0)) StartCoroutine(DoMelee());
        else if (Input.GetKeyDown(KeyCode.E) && sp1Timer <= 0f) StartCoroutine(DoSpecial1());
        else if (Input.GetKeyDown(KeyCode.Q) && sp2Timer <= 0f) StartCoroutine(DoSpecial2());
    }

    IEnumerator DoMelee()
    {
        IsAttacking = true;
        anim.SetTrigger("Attack");
        PlaySound(meleeSwingClip);
        yield return new WaitForSeconds(0.45f);
        HitAtWeapon(meleeDamage);
        yield return new WaitForSeconds(0.55f);
        IsAttacking = false;
    }

    IEnumerator DoSpecial1()
    {
        IsAttacking = true;
        sp1Timer = special1Cooldown;
        anim.SetTrigger("Special1");
        PlaySound(special1Clip);
        yield return new WaitForSeconds(0.5f);
        HitInRadius(special1Damage, special1Range);
        yield return new WaitForSeconds(0.8f);
        IsAttacking = false;
    }

    IEnumerator DoSpecial2()
    {
        IsAttacking = true;
        sp2Timer = special2Cooldown;
        anim.SetTrigger("Special2");
        PlaySound(special2Clip);
        yield return new WaitForSeconds(0.7f);
        HitInRadius(special2Damage, special2Range);
        yield return new WaitForSeconds(0.9f);
        IsAttacking = false;
    }

    void HitAtWeapon(float dmg)
    {
        Vector3 origin = weaponTip != null ? weaponTip.position : transform.position + transform.forward * meleeRange;
        foreach (Collider col in Physics.OverlapSphere(origin, weaponHitRadius, enemyLayer))
        {
            if (col.CompareTag(enemyTag)) Hit(col, dmg);
        }
    }

    void HitInRadius(float dmg, float range)
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, range, enemyLayer))
        {
            if (col.CompareTag(enemyTag)) Hit(col, dmg);
        }
    }

    void Hit(Collider col, float dmg)
    {
        HeroHealth hp = col.GetComponent<HeroHealth>();
        if (hp != null) hp.TakeDamage(dmg);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSrc != null) audioSrc.PlayOneShot(clip);
    }
}
