using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Melee Attack")]
    public float meleeDamage  = 40f;
    public float meleeRange   = 2.5f;
    public float meleeAngle   = 90f;
    public LayerMask enemyLayer;
    public string enemyTag = "Hero";

    [Header("Weapon Hit (Melee)")]
    public Transform weaponTip;            // drag the club bone here; if null, falls back to a point in front of the troll
    public float     weaponHitRadius = 3.5f;

    [Header("Special Attack 1 - Shockwave (E)")]
    public float special1Damage   = 60f;
    public float special1Range    = 5f;
    public float special1Cooldown = 5f;
    public GameObject special1VFX;

    [Header("Special Attack 2 - Ground Smash (Q)")]
    public float special2Damage   = 80f;
    public float special2Range    = 7f;
    public float special2Cooldown = 10f;
    public GameObject special2VFX;

    [Header("Feedback")]
    public GameObject hitVFX;
    public AudioClip meleeSwingClip;
    public AudioClip special1Clip;
    public AudioClip special2Clip;

    // Plain private fields track cooldown — no [Header] on properties
    private float _sp1Timer;
    private float _sp2Timer;

    public bool  IsAttacking     { get; private set; }
    public float Special1Current => _sp1Timer;
    public float Special2Current => _sp2Timer;

    private Animator    _anim;
    private AudioSource _audio;

    private static readonly int HashAttack   = Animator.StringToHash("Attack");
    private static readonly int HashSpecial1 = Animator.StringToHash("Special1");
    private static readonly int HashSpecial2 = Animator.StringToHash("Special2");

    private void Awake()
    {
        _anim  = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (_sp1Timer > 0f) _sp1Timer -= Time.deltaTime;
        if (_sp2Timer > 0f) _sp2Timer -= Time.deltaTime;
        if (IsAttacking) return;

        if (Input.GetMouseButtonDown(0))                              StartCoroutine(DoMelee());
        else if (Input.GetKeyDown(KeyCode.E) && _sp1Timer <= 0f)     StartCoroutine(DoSpecial1());
        else if (Input.GetKeyDown(KeyCode.Q) && _sp2Timer <= 0f)     StartCoroutine(DoSpecial2());
    }

    private IEnumerator DoMelee()
    {
        IsAttacking = true;
        _anim.SetTrigger(HashAttack);
        PlaySound(meleeSwingClip);
        yield return new WaitForSeconds(0.45f);
        HitAtWeapon(meleeDamage, weaponHitRadius);
        yield return new WaitForSeconds(0.55f);
        IsAttacking = false;
    }

    private void HitAtWeapon(float dmg, float radius)
    {
        Vector3 origin = weaponTip != null
            ? weaponTip.position
            : transform.position + transform.forward * meleeRange;

        foreach (Collider col in Physics.OverlapSphere(origin, radius, enemyLayer))
        {
            if (col.CompareTag(enemyTag)) Hit(col, dmg);
        }
    }

    private IEnumerator DoSpecial1()
    {
        IsAttacking = true;
        _sp1Timer = special1Cooldown;
        _anim.SetTrigger(HashSpecial1);
        PlaySound(special1Clip);
        yield return new WaitForSeconds(0.5f);
        SpawnVFX(special1VFX, transform.position);
        HitInRadius(special1Damage, special1Range);
        yield return new WaitForSeconds(0.8f);
        IsAttacking = false;
    }

    private IEnumerator DoSpecial2()
    {
        IsAttacking = true;
        _sp2Timer = special2Cooldown;
        _anim.SetTrigger(HashSpecial2);
        PlaySound(special2Clip);
        yield return new WaitForSeconds(0.7f);
        SpawnVFX(special2VFX, transform.position);
        HitInRadius(special2Damage, special2Range);
        Knockback(special2Range, 8f);
        yield return new WaitForSeconds(0.9f);
        IsAttacking = false;
    }

    private void HitInCone(float dmg, float range, float angle)
    {
        Vector3 forward = transform.forward; forward.y = 0f; forward.Normalize();
        foreach (Collider col in Physics.OverlapSphere(transform.position, range, enemyLayer))
        {
            if (!col.CompareTag(enemyTag)) continue;
            Vector3 dir = col.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) { Hit(col, dmg); continue; }
            dir.Normalize();
            if (Vector3.Angle(forward, dir) <= angle * 0.5f)
                Hit(col, dmg);
        }
    }

    private void HitInRadius(float dmg, float range)
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, range, enemyLayer))
        {
            if (col.CompareTag(enemyTag)) Hit(col, dmg);
        }
    }

    private void Knockback(float range, float force)
    {
        foreach (Collider col in Physics.OverlapSphere(transform.position, range, enemyLayer))
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb == null) continue;
            rb.AddForce((col.transform.position - transform.position).normalized * force, ForceMode.Impulse);
        }
    }

    private void Hit(Collider col, float dmg)
    {
        HeroHealth hp = col.GetComponent<HeroHealth>();
        if (hp != null) hp.TakeDamage(dmg);
        SpawnVFX(hitVFX, col.transform.position);
    }

    private void SpawnVFX(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return;
        Destroy(Instantiate(prefab, pos, Quaternion.identity), 3f);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _audio != null) _audio.PlayOneShot(clip);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f);
        Vector3 origin = weaponTip != null
            ? weaponTip.position
            : transform.position + transform.forward * meleeRange;
        Gizmos.DrawWireSphere(origin, weaponHitRadius);
    }
}