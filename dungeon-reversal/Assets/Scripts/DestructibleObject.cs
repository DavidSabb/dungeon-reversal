using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public float health = 50f;
    public GameObject brokenPrefab;
    public float debrisLifetime = 5f;
    public AudioClip breakClip;
    public GameObject breakVFX;
    public float breakVFXDuration = 3f;

    bool broken;
    AudioSource audioSrc;

    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
    }

    public void TakeDamage(float amount)
    {
        if (broken) return;
        health -= amount;
        if (health <= 0f) Break();
    }

    void Break()
    {
        broken = true;
        if (audioSrc != null && breakClip != null) audioSrc.PlayOneShot(breakClip);

        if (breakVFX != null)
        {
            GameObject vfx = Instantiate(breakVFX, transform.position, Quaternion.identity);
            Destroy(vfx, breakVFXDuration);
        }

        if (brokenPrefab != null)
        {
            GameObject debris = Instantiate(brokenPrefab, transform.position, transform.rotation);
            foreach (Rigidbody rb in debris.GetComponentsInChildren<Rigidbody>())
            {
                Vector3 dir = (rb.transform.position - transform.position).normalized + Vector3.up * 0.5f;
                rb.AddForce(dir * Random.Range(3f, 8f), ForceMode.Impulse);
            }
            Destroy(debris, debrisLifetime);
        }

        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BossAttack")) TakeDamage(health);
    }
}
