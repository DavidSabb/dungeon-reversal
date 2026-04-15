using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HeroHealth))]
public class HeroAI : MonoBehaviour
{
    private enum State { Idle, Patrol, Chase, Attack, Dead }
    private State _state = State.Idle;

    [Header("Detection")]
    public float sightRange  = 15f;
    public float attackRange = 2f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    [Header("Combat Base Values")]
    public float baseDamage         = 15f;
    public float baseReactionDelay  = 1.5f;
    public float baseAttackCooldown = 1.8f;
    public AudioClip  attackClip;
    public GameObject hitVFX;

    // Runtime scaled values — plain private fields (no [Header] on these)
    private float _damage;
    private float _reactionDelay;
    private float _attackCooldown;

    private NavMeshAgent _agent;
    private Animator     _anim;
    private AudioSource  _audio;
    private Transform    _player;
    private int          _patrolIndex;
    private float        _attackTimer;
    private bool         _reacting;

    private static readonly int HashSpeed  = Animator.StringToHash("Speed");
    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashDie    = Animator.StringToHash("Die");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _anim  = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
        _damage         = baseDamage;
        _reactionDelay  = baseReactionDelay;
        _attackCooldown = baseAttackCooldown;
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
        _state = patrolPoints.Length > 0 ? State.Patrol : State.Idle;
    }

    private void Update()
    {
        if (_state == State.Dead) return;
        _attackTimer -= Time.deltaTime;

        switch (_state)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }

        _anim.SetFloat(HashSpeed, _agent.velocity.magnitude, 0.1f, Time.deltaTime);
    }

    private void UpdateIdle()   { if (CanSeePlayer()) StartCoroutine(ReactAndChase()); }

    private void UpdatePatrol()
    {
        if (CanSeePlayer()) { StartCoroutine(ReactAndChase()); return; }
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            StartCoroutine(WaitAndNextPatrol());
    }

    private void UpdateChase()
    {
        if (_player == null) return;
        _agent.SetDestination(_player.position);
        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist <= attackRange)           { _state = State.Attack; _agent.ResetPath(); }
        else if (dist > sightRange * 1.5f) { _state = patrolPoints.Length > 0 ? State.Patrol : State.Idle; }
    }

    private void UpdateAttack()
    {
        if (_player == null) return;
        float dist = Vector3.Distance(transform.position, _player.position);
        Vector3 dir = (_player.position - transform.position).normalized; dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 10f * Time.deltaTime);
        if (dist > attackRange * 1.2f) { _state = State.Chase; return; }
        if (_attackTimer <= 0f) { _attackTimer = _attackCooldown; StartCoroutine(DoAttack()); }
    }

    private IEnumerator ReactAndChase()
    {
        if (_reacting) yield break;
        _reacting = true;
        yield return new WaitForSeconds(_reactionDelay);
        _state = State.Chase;
        _reacting = false;
    }

    private IEnumerator WaitAndNextPatrol()
    {
        _agent.ResetPath();
        yield return new WaitForSeconds(patrolWaitTime);
        if (patrolPoints.Length == 0) yield break;
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        _agent.SetDestination(patrolPoints[_patrolIndex].position);
    }

    private IEnumerator DoAttack()
    {
        _anim.SetTrigger(HashAttack);
        if (_audio != null && attackClip != null) _audio.PlayOneShot(attackClip);
        yield return new WaitForSeconds(0.4f);
        if (_player != null && Vector3.Distance(transform.position, _player.position) <= attackRange * 1.3f)
        {
            PlayerHealth ph = _player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(_damage);
            if (hitVFX != null) Destroy(Instantiate(hitVFX, _player.position, Quaternion.identity), 2f);
        }
    }

    public void SetWaveScaling(int waveNumber)
    {
        float scale = 1f + (waveNumber - 1) * 0.1f;
        float react = Mathf.Max(0.2f, 1f - (waveNumber - 1) * 0.12f);
        _damage         = baseDamage * scale;
        _reactionDelay  = baseReactionDelay * react;
        _attackCooldown = Mathf.Max(0.5f, baseAttackCooldown - (waveNumber - 1) * 0.1f);
        _agent.speed    = 3.5f * scale;
    }

    public void OnDeath()
    {
        _state = State.Dead;
        _agent.enabled = false;
        _anim.SetTrigger(HashDie);
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, 3f);
    }

    private bool CanSeePlayer() =>
        _player != null && Vector3.Distance(transform.position, _player.position) <= sightRange;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}