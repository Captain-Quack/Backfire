using System.Collections;
using Backfire;
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyGunner : MonoBehaviour
{
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    #region Inspector
    [Header("Patrol")]
    [SerializeField] private float patrolRange    = 4f;
    [SerializeField] private float patrolSpeed    = 2.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform  muzzle;
    [SerializeField] private float  detectionRadius = 7f;
    [SerializeField] private float  leadTime        = .25f;
    [SerializeField] private int    burstCount      = 3;
    [SerializeField] private float  burstCadence    = .15f;
    [SerializeField] private float  cooldownTime    = 1.8f;
    [SerializeField] private float  bulletSpeed     = 12f;

    [Header("Re‑positioning / Cover")]
    [SerializeField] private float flankDistance    = 2f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidanceForce   = 8f;
    [SerializeField] private float lookAhead        = 1f;
    #endregion

    enum State { Wait, Patrol, Aim, Burst, Cooldown, Reposition }
    State state = State.Wait;

    Rigidbody2D rb;
    PlayerController player;
    Rigidbody2D playerRb;

    Vector2 homePos;
    Vector2 patrolTarget;
    float   stateTimer;

    public Animator anim;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player  = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRb= player?.GetComponent<Rigidbody2D>();

        homePos = transform.position;
        PickNewPatrolTarget();
    }

    #region Unity loop
    void FixedUpdate()
    {
        if (GameManager.Instance && !GameManager.Instance.GameIsRunning) return;

        stateTimer += Time.fixedDeltaTime;

        switch (state)
        {
            case State.Wait: DoWait(); break;
            case State.Patrol: DoPatrol();     break;
            case State.Aim: DoAim();        break;
            case State.Cooldown: DoCooldown();   break;
            case State.Reposition: DoReposition(); break;
            case State.Burst: /* coroutine */ break;
        }
    }


    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            col.collider.GetComponent<Controller>()?.TakeDamage();
            Destroy(gameObject);
            return;
        }

        if (!col.collider.CompareTag("PlayerBullet")) return;
    }
    
    void DoWait()
    {
        if (player.currentState is PlayingState)
        {
            ChangeState(State.Patrol);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(homePos == Vector2.zero ? transform.position : (Vector3)homePos, patrolRange);
    }
    #endregion

    #region State logic
    void DoPatrol()
    {
        MoveTowards(patrolTarget, patrolSpeed);

        if (Vector2.Distance(transform.position, patrolTarget) < .15f)
            PickNewPatrolTarget();

        if (PlayerInRangeAndLoS()) ChangeState(State.Aim);
    }

    void DoAim()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 dir = PredictedPlayerPos() - (Vector2)muzzle.position;
        transform.right = -dir;

        if (!PlayerInRangeAndLoS())           ChangeState(State.Reposition);
        else if (stateTimer > .4f)            ChangeState(State.Burst);
    }

    void DoCooldown()
    {
        if (stateTimer > cooldownTime)
        {
            if (PlayerInRangeAndLoS()) ChangeState(State.Aim);
            else  ChangeState(State.Patrol);
        }
    }

    void DoReposition()
    {
        Vector2 flankDir = ((Vector2)transform.position - (Vector2)player.transform.position).normalized;
        Vector2 goal = (Vector2)transform.position + Vector2.Perpendicular(flankDir).normalized * flankDistance;

        MoveTowards(goal, patrolSpeed * 1.3f);

        if (stateTimer > 1f || PlayerInRangeAndLoS())
            ChangeState(PlayerInRangeAndLoS() ? State.Aim : State.Patrol);
    }
    #endregion

    #region Transitions
    void ChangeState(State next)
    {
        StopAllCoroutines();
        state = next;
        stateTimer = 0f;

        switch (next)
        {
            case State.Patrol:     PickNewPatrolTarget(); break;
            case State.Burst:      StartCoroutine(BurstFire());  break;
            case State.Reposition: break;
        }
    }

    IEnumerator BurstFire()
    {
        state = State.Burst;

        for (int i = 0; i < burstCount; i++)
        {
            if (!PlayerInRangeAndLoS()) break;
            FireBullet();
            anim.SetTrigger(Shoot);
            yield return new WaitForSeconds(burstCadence);
        }

        ChangeState(State.Cooldown);
    }
    #endregion

    #region Helpers – movement / firing
    void MoveTowards(Vector2 target, float speed)
    {
        Vector2 desired = (target - (Vector2)transform.position).normalized * speed;
        Steer(desired);
    }

    void Steer(Vector2 desiredVel)
    {
        Vector2 dir = desiredVel.normalized;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position,
                                                .45f,
                                                dir,
                                                lookAhead,
                                                obstacleMask);
        if (hit)
        {
            Vector2 away = ((Vector2)transform.position - hit.point).normalized;
            desiredVel += away * avoidanceForce;
        }

        rb.AddForce(desiredVel - rb.linearVelocity, ForceMode2D.Force);
    }

    private void FireBullet()
    {
        Vector2 dir = (PredictedPlayerPos() - (Vector2)muzzle.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab);

        bullet.transform.position = muzzle.position;
        bullet.transform.rotation = Quaternion.FromToRotation(Vector3.right, dir);
        bullet.transform.rotation *= Quaternion.Euler(0f, 0f, Random.value * 5f);
        bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * bulletSpeed;
        RuntimeManager.PlayOneShot("event:/SFX/Game/Shooter Shoot");
    }

    #endregion

    #region Helpers – detection / math
    bool PlayerInRangeAndLoS()
    {
        if (player is null) return false;

        if ((player.transform.position - transform.position).sqrMagnitude >
            detectionRadius * detectionRadius) return false;

        // line of sight
        Vector2 dir = player.transform.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized,
                                             dir.magnitude, obstacleMask);
        return !hit; // no wall in between
    }

    Vector2 PredictedPlayerPos()
    {
        if (playerRb == null) return player.transform.position;
        return (Vector2)player.transform.position + playerRb.linearVelocity * leadTime;
    }

    void PickNewPatrolTarget()
    {
        patrolTarget = homePos + Random.insideUnitCircle * patrolRange;
    }
    #endregion

  
}
