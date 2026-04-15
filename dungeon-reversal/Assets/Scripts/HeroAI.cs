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
    public float sightRange  = 25f;
    public float attackRange = 3.5f;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float patrolWaitTime = 2f;

    [Header("Combat Base Values")]
    public float baseDamage         = 15f;
    public float baseReactionDelay  = 0.4f;
    public float baseAttackCooldown = 1.6f;
    public float heavyAttackMultiplier = 2.0f; // every 3rd attack
    public AudioClip  attackClip;
    public GameObject hitVFX;

    [Header("Movement")]
    public float moveSpeed       = 4f;
    public float fallbackTurnSpeed = 8f;

    private float _damage;
    private float _reactionDelay;
    private float _attackCooldown;

    private NavMeshAgent _agent;
    private Animator     _anim;
    private AudioSource  _audio;
    private Transform    _player;
    private PlayerHealth _playerHealth;
    private int          _patrolIndex;
    private float        _attackTimer;
    private float        _refindTimer;
    private bool         _reacting;
    private int          _attackCount;
    private bool         _useNavMesh;

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
        FindPlayer();
        TryEnableNavMesh();
        _state = patrolPoints.Length > 0 ? State.Patrol : State.Idle;
    }

    private void TryEnableNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            _agent.enabled = true;
            if (_agent.isOnNavMesh)
            {
                _agent.speed = moveSpeed;
                _agent.stoppingDistance = attackRange * 0.8f;
                _useNavMesh = true;
                return;
            }
            _agent.enabled = false;
        }
        _useNavMesh = false;
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p == null)
        {
            _playerHealth = FindObjectOfType<PlayerHealth>();
            if (_playerHealth != null) p = _playerHealth.gameObject;
        }
        else
        {
            _playerHealth = p.GetComponent<PlayerHealth>();
        }
        if (p != null) _player = p.transform;
    }

    private void Update()
    {
        if (_state == State.Dead) return;

        _attackTimer -= Time.deltaTime;
        _refindTimer -= Time.deltaTime;
        if (_player == null && _refindTimer <= 0f)
        {
            _refindTimer = 1f;
            FindPlayer();
        }

        switch (_state)
        {
            case State.Idle:   UpdateIdle();   break;
            case State.Patrol: UpdatePatrol(); break;
            case State.Chase:  UpdateChase();  break;
            case State.Attack: UpdateAttack(); break;
        }

        if (_anim.runtimeAnimatorController != null)
        {
            float rawSpeed = _useNavMesh ? _agent.velocity.magnitude
                                         : (_state == State.Chase ? moveSpeed : 0f);
            float normalized = moveSpeed > 0f ? rawSpeed / moveSpeed : 0f;
            float animSpeed  = normalized * 4f; // map to blend tree (idle=0, walk=1, run=4)
            _anim.SetFloat(HashSpeed, animSpeed, 0.1f, Time.deltaTime);
        }
    }

    private void UpdateIdle()
    {
        if (CanSeePlayer()) StartCoroutine(ReactAndChase());
    }

    private void UpdatePatrol()
    {
        if (CanSeePlayer()) { StartCoroutine(ReactAndChase()); return; }
        if (!_useNavMesh) return;
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            StartCoroutine(WaitAndNextPatrol());
    }

    private void UpdateChase()
    {
        if (_player == null) return;

        if (_useNavMesh)
            _agent.SetDestination(_player.position);
        else
            DirectMoveTowardPlayer();

        float dist = HorizontalDistance(transform.position, _player.position);
        if (dist <= attackRange)
        {
            _state = State.Attack;
            if (_useNavMesh) _agent.ResetPath();
        }
        else if (dist > sightRange * 1.8f)
        {
            _state = patrolPoints.Length > 0 ? State.Patrol : State.Idle;
        }
    }

    private void UpdateAttack()
    {
        if (_player == null) { _state = State.Idle; return; }

        FacePlayer();

        float dist = HorizontalDistance(transform.position, _player.position);
        if (dist > attackRange * 1.25f) { _state = State.Chase; return; }

        if (_attackTimer <= 0f)
        {
            _attackTimer = _attackCooldown;
            StartCoroutine(DoAttack());
        }
    }

    private void DirectMoveTowardPlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        transform.position += dir * moveSpeed * Time.deltaTime;
        FacePlayer();
    }

    private void FacePlayer()
    {
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, fallbackTurnSpeed * Time.deltaTime);
    }

    private IEnumerator ReactAndChase()
    {
        if (_reacting) yield break;
        _reacting = true;
        yield return new WaitForSeconds(_reactionDelay);
        if (_state != State.Dead) _state = State.Chase;
        _reacting = false;
    }

    private IEnumerator WaitAndNextPatrol()
    {
        if (_useNavMesh) _agent.ResetPath();
        yield return new WaitForSeconds(patrolWaitTime);
        if (patrolPoints.Length == 0 || !_useNavMesh) yield break;
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        _agent.SetDestination(patrolPoints[_patrolIndex].position);
    }

    private IEnumerator DoAttack()
    {
        _attackCount++;
        bool isHeavy = (_attackCount % 3) == 0;
        float windup = isHeavy ? 0.7f : 0.35f;
        float damage = isHeavy ? _damage * heavyAttackMultiplier : _damage;

        if (_anim.runtimeAnimatorController != null)
            _anim.SetTrigger(HashAttack);
        if (_audio != null && attackClip != null)
            _audio.PlayOneShot(attackClip);

        yield return new WaitForSeconds(windup);

        if (_state == State.Dead || _player == null) yield break;

        if (_playerHealth != null && !_playerHealth.IsDead &&
            HorizontalDistance(transform.position, _player.position) <= attackRange * 1.4f)
        {
            _playerHealth.TakeDamage(damage);
            if (hitVFX != null) Destroy(Instantiate(hitVFX, _player.position, Quaternion.identity), 2f);
        }
    }

    public void SetWaveScaling(int waveNumber)
    {
        WaveManager wm = WaveManager.Instance;
        float dmgS    = wm != null ? wm.GetDamageScale(waveNumber)   : 1f + (waveNumber - 1) * 0.20f;
        float hpS     = wm != null ? wm.GetHealthScale(waveNumber)   : 1f + (waveNumber - 1) * 0.25f;
        float spdS    = wm != null ? wm.GetSpeedScale(waveNumber)    : 1f + (waveNumber - 1) * 0.12f;
        float reactS  = wm != null ? wm.GetReactScale(waveNumber)    : Mathf.Pow(0.82f, waveNumber - 1);
        float cdS     = wm != null ? wm.GetCooldownScale(waveNumber) : Mathf.Pow(0.88f, waveNumber - 1);

        _damage         = baseDamage * dmgS;
        _reactionDelay  = Mathf.Max(0.1f, baseReactionDelay * reactS);
        _attackCooldown = Mathf.Max(0.4f, baseAttackCooldown * cdS);

        moveSpeed = moveSpeed * spdS;
        if (_useNavMesh && _agent != null && _agent.enabled)
            _agent.speed = moveSpeed;

        HeroHealth hp = GetComponent<HeroHealth>();
        if (hp != null)
        {
            hp.maxHealth *= hpS;
            hp.SendMessage("ResetToMax", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void OnDeath()
    {
        _state = State.Dead;
        if (_useNavMesh && _agent.enabled) _agent.enabled = false;
        if (_anim.runtimeAnimatorController != null)
            _anim.SetTrigger(HashDie);
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Destroy(gameObject, 3f);
    }

    private bool CanSeePlayer() =>
        _player != null && HorizontalDistance(transform.position, _player.position) <= sightRange;

    private static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
