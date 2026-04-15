using UnityEngine;

/// <summary>
/// DestructibleObject.cs
/// Dungeon Reversal - For breakable pillars, rocks, and arena objects.
/// GDD: "Some environmental objects such as pillars or rocks can be broken
///       by strong boss attacks."
/// 
/// Attach to any breakable prop. Optionally assign an intactMesh and
/// a brokenVersion prefab (pre-fractured version of the same object).
/// </summary>
public class DestructibleObject : MonoBehaviour
{
    [Header("Health")]
    public float health = 50f;

    [Header("Broken Version")]
    public GameObject brokenPrefab;     // fractured prefab to spawn on break
    public float      debrisLifetime = 5f;

    [Header("Feedback")]
    public AudioClip  breakClip;
    public GameObject breakVFX;
    public float      breakVFXDuration = 3f;

    private bool _broken;
    private AudioSource _audio;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>Call this from PlayerCombat or any attack that should break objects.</summary>
    public void TakeDamage(float amount)
    {
        if (_broken) return;

        health -= amount;
        if (health <= 0f)
            Break();
    }

    private void Break()
    {
        _broken = true;

        if (_audio != null && breakClip != null)
            _audio.PlayOneShot(breakClip);

        if (breakVFX != null)
        {
            GameObject vfx = Instantiate(breakVFX, transform.position, Quaternion.identity);
            Destroy(vfx, breakVFXDuration);
        }

        if (brokenPrefab != null)
        {
            GameObject debris = Instantiate(brokenPrefab, transform.position, transform.rotation);
            // Add physics impulse to debris pieces
            Rigidbody[] rbs = debris.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rbs)
            {
                Vector3 dir = (rb.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * Random.Range(3f, 8f), ForceMode.Impulse);
            }
            Destroy(debris, debrisLifetime);
        }

        gameObject.SetActive(false);
    }

    // Allow the player's area attacks to break destructibles in range
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BossAttack"))
        {
            TakeDamage(health); // one-shot break on special attack trigger
        }
    }
}