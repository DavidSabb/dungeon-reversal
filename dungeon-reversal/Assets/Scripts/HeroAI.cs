using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeroHealth))]
public class HeroAI : MonoBehaviour
{
    private enum State { Patrol, Chase, Attack, Dead }
    private State state = State.Patrol;

    [Header("Stats")]
    public float moveSpeed = 4f;
    public float sightRange = 25f;
    public float attackRange = 3f;
    public float damage = 8f;
    public float attackCooldown = 2f;

    [Header("Patrol")]
    public Transform[] patrolPoints;

    public AudioClip attackClip;

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSrc;
    private Transform player;
    private PlayerHealth playerHealth;
    private int patrolIndex;
    private float attackTimer;

    private static readonly int HashSpeed  = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie    = Animator.StringToHash("Die");

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSrc = GetComponent<AudioSource>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 50f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        FindPlayer();
        GotoNextPatrolPoint();
    }

    void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponentInParent<PlayerHealth>();
            if (playerHealth == null) playerHealth = p.GetComponentInChildren<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null) player = playerHealth.transform;
        }
    }

    void Update()
    {
        if (state == State.Dead) return;

        if (player == null) FindPlayer();

        attackTimer -= Time.deltaTime;

        Vector3 prev = transform.position;

        switch (state)
        {
            case State.Patrol: Patrol(); break;
            case State.Chase:  Chase();  break;
            case State.Attack: Attack(); break;
        }

        float speed = Time.deltaTime > 0f ? (transform.position - prev).magnitude / Time.deltaTime : 0f;
        anim.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);
    }

    void Patrol()
    {
        if (player != null)
        {
            state = State.Chase;
            return;
        }

        if (!agent.isOnNavMesh) return;
        if (patrolPoints.Length > 0 && !agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPatrolPoint();
    }

    void Chase()
    {
        if (player == null) { state = State.Patrol; return; }

        if (agent.isOnNavMesh) agent.SetDestination(player.position);
        else MoveDirectly();

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackRange) state = State.Attack;
    }

    void MoveDirectly()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        dir.Normalize();
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    void Attack()
    {
        if (player == null) { state = State.Patrol; return; }

        if (agent.isOnNavMesh) agent.SetDestination(transform.position);
        FacePlayer();

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange)
        {
            state = State.Chase;
            return;
        }

        if (attackTimer <= 0f)
        {
            attackTimer = attackCooldown;
            StartCoroutine(DoAttack());
        }
    }

    IEnumerator DoAttack()
    {
        anim.SetTrigger(HashAttack);
        if (audioSrc != null && attackClip != null) audioSrc.PlayOneShot(attackClip);

        yield return new WaitForSeconds(0.4f);

        if (state == State.Dead || player == null) yield break;
        if (Vector3.Distance(transform.position, player.position) <= attackRange * 1.4f
            && playerHealth != null && !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    void GotoNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        if (!agent.isOnNavMesh) return;
        agent.SetDestination(patrolPoints[patrolIndex].position);
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
    }

    bool CanSeePlayer()
    {
        return player != null && Vector3.Distance(transform.position, player.position) <= sightRange;
    }

    public void SetWaveScaling(int waveNumber, float difficulty)
    {
        damage *= (1f + (waveNumber - 1) * 0.10f) * difficulty;
        moveSpeed *= 1f + (waveNumber - 1) * 0.05f;
        attackCooldown *= Mathf.Pow(0.95f, waveNumber - 1);

        if (agent != null) agent.speed = moveSpeed;

        HeroHealth hp = GetComponent<HeroHealth>();
        if (hp != null)
        {
            hp.maxHealth *= (1f + (waveNumber - 1) * 0.15f) * difficulty;
            hp.SendMessage("ResetToMax", SendMessageOptions.DontRequireReceiver);
        }
    }
    public void OnDeath()
    {
        state = State.Dead;
        if (agent != null && agent.enabled) agent.isStopped = true;
        anim.SetTrigger(HashDie);
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 3f);
    }
}
