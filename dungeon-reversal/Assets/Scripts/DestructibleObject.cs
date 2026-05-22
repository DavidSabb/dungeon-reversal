using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public float health = 50f;
    public AudioClip breakClip;

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
        gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BossAttack")) TakeDamage(health);
    }
}
