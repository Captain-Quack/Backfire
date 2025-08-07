using System.Runtime.CompilerServices;
using Backfire;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class PassiveEnemy : MonoBehaviour
{
    [Header("Behaviour")]
    [SerializeField] float detectionRadius = 4f;
    [SerializeField] float wanderSpeed = 2f;
    [SerializeField] float fleeSpeed = 5f;
    [SerializeField] float hideTime = 3f;
    [Tooltip("Seconds between random wander‑direction tweaks.")]
    [SerializeField] float wanderDirChange  = 1.8f;

    [SerializeField] float wanderRadius = 6f;
    [SerializeField] Vector2 idleTimeRange = new(1.2f, 2f); 
    [SerializeField] Vector2 moveTimeRange = new(1.5f, 3f);

    [Header("Environment")]
    [SerializeField] LayerMask obstacleMask;
    [SerializeField] float avoidanceForce = 8f;
    [SerializeField] float lookAhead = 7f;
    [SerializeField] float obstacleAvoidanceRadius = 1f; 

    [Header("Optional Patrol")]
    [SerializeField] Transform[] patrolPoints;

    enum State { Wait, Float, Idle, Flee, Hide, Patrol }
    State  _state = State.Float;

    Rigidbody2D _rb;
    Transform _player;
    Vector2 _home;
    Vector2 _segmentTarget;

    float _stateTimer;
    float _segmentTimer;
    float _segmentDuration;
    float _wanderDirTimer;

    int _patrolIndex;
    Vector2 _patrolTarget => (Vector2)patrolPoints[_patrolIndex].position;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        var playerGo = GameObject.FindGameObjectWithTag("Player");
        if (playerGo != null) _player = playerGo.transform;

        _home = transform.position;
        PickNewWanderTarget();
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        _stateTimer += dt;
        _segmentTimer  += dt;
        _wanderDirTimer += dt;

        switch (_state)
        {
            case State.Wait:   DoWait(); break;
            case State.Float:  DoFloat();   break;
            case State.Idle:   DoIdle();    break;
            case State.Flee:   DoFlee();    break;
            case State.Hide:   DoHide();    break;
            case State.Patrol: DoPatrol();  break;
        }
    }

    void DoWait()
    {
        if(PlayerTooClose()) ChangeState(State.Flee);
    }

    void DoFloat()
    {
        if (_segmentTimer >= _segmentDuration || OutOfBounds())
            ChangeState(State.Idle);

        if (PlayerTooClose())
            ChangeState(State.Flee);

        Vector2 dir = _segmentTarget - _rb.position;
        Steer(dir.normalized * wanderSpeed);
    }

    void DoIdle()
    {
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, 4f * Time.fixedDeltaTime);

        if (_segmentTimer >= _segmentDuration)        ChangeState(NextFreeRoamState());
        else if (PlayerTooClose())                    ChangeState(State.Flee);
    }

    void DoPatrol()
    {
        if (patrolPoints.Length == 0) { ChangeState(State.Float); return; }

        Vector2 toTarget = _patrolTarget - _rb.position;

        if (toTarget.sqrMagnitude < 0.3f * 0.3f)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            toTarget = _patrolTarget - _rb.position;
        }

        Steer(toTarget.normalized * wanderSpeed);

        if (PlayerTooClose()) ChangeState(State.Flee);
    }

    void DoFlee()
    {
        if (_player == null) { ChangeState(State.Float); return; }

        Vector2 away = (Vector2)(transform.position - _player.position).normalized;
        Steer(away * fleeSpeed);

        if (_stateTimer >= 1.1f) ChangeState(State.Hide);
    }

    void DoHide()
    {
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, Vector2.zero, 4f * Time.fixedDeltaTime);

        if      (PlayerTooClose())      ChangeState(State.Flee);
        else if (_stateTimer >= hideTime) ChangeState(State.Idle);
    }

    void ChangeState(State next)
    {
        _state = next;
        _stateTimer = 0f;
        _segmentTimer = 0f;

        switch (next)
        {
            case State.Float:
                PickNewWanderTarget();
                break;

            case State.Idle:
                _segmentDuration = Random.Range(idleTimeRange.x, idleTimeRange.y);
                break;

            case State.Patrol:
                _patrolIndex = 0;
                break;

            case State.Hide:
                _rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    State NextFreeRoamState()
    {
        return patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Float;
    }

    void PickNewWanderTarget()
    {
        _segmentTarget  = _home + Random.insideUnitCircle * wanderRadius;
        _segmentDuration = Random.Range(moveTimeRange.x, moveTimeRange.y);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool PlayerTooClose()
    {
        if (_player == null) return false;
        return (_player.position - transform.position).sqrMagnitude <= detectionRadius * detectionRadius;
    }

    bool OutOfBounds()
    {
        return (_rb.position - _home).sqrMagnitude > wanderRadius * wanderRadius;
    }

    void Steer(Vector2 wishVelocity)
    {
        if (GameManager.Instance && !GameManager.Instance.GameIsRunning) return;

        Vector2 dir = wishVelocity.normalized;
        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position,
            obstacleAvoidanceRadius,
            dir,
            lookAhead,
            obstacleMask);

        if (hit)
        {
            Vector2 avoidDir  = hit.normal;
            Vector2 deflect   = Vector2.Perpendicular(avoidDir) * (Random.value > 0.5f ? 1f : -1f);
            Vector2 avoidance = (avoidDir + deflect * 0.3f).normalized * avoidanceForce;

            wishVelocity = Vector2.Lerp(wishVelocity, wishVelocity + avoidance, 5f * Time.fixedDeltaTime);
        }

        wishVelocity = Vector2.ClampMagnitude(wishVelocity, wanderSpeed);
        Vector2 steering = wishVelocity - _rb.linearVelocity;
        _rb.AddForce(steering, ForceMode2D.Force);
        _rb.linearVelocity = Vector2.ClampMagnitude(_rb.linearVelocity, Mathf.Max(wanderSpeed, fleeSpeed));

        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
            transform.up = _rb.linearVelocity;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerController>().TakeDamage();
        }
        
    }
}
