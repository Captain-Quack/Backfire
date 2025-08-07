using System.Collections;
using Backfire;
using UnityEngine;
using FMODUnity;


[RequireComponent(typeof(Rigidbody2D))]
public sealed class FlyingShooter : MonoBehaviour
{
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    #region Inspector
    [Header("Patrol")]
    [Tooltip("Leave null to wander randomly.")]
    [SerializeField] private Transform pointA;
    [SerializeField]  private Transform pointB;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float wanderRadius = 4f;
    [SerializeField] private float nodeTolerance = .25f;

    [Header("Shoot")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform upMuzzle;
    [SerializeField] private float bulletSpeed       = 10f;
    [SerializeField] private float leadTime          = .3f;
    [SerializeField] private int   burstCount        = 3;
    [SerializeField] private float burstCadence      = .12f;
    [SerializeField] private float cooldownTime      = 1.6f;
    [SerializeField] private float detectionRadius   = 6f;

    [Header("Avoidance")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidanceForce    = 10f;
    [SerializeField] private float avoidanceLookAhead = 1.2f;
    
    [Header("Hover")]
    [SerializeField] private float hoverDistance = 4f;
    
    #endregion

    enum AIState { Wait, Patrol, Aim, Shoot, Cooldown }
    AIState state = AIState.Wait;
    private Transform[] muzzles = new Transform[4];
    public Animator anim;


    Rigidbody2D rb;
    PlayerController player;
    Rigidbody2D playerRb;

    Vector2 currentPatrolTarget;
    float stateTimer;

    void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        float d = upMuzzle.position.y - transform.position.y;
        muzzles[0] = Instantiate(upMuzzle, transform.position + Vector3.up    * d,
            Quaternion.identity, transform);       
        muzzles[1] = Instantiate(upMuzzle, transform.position + Vector3.down  * d,
            Quaternion.Euler(0, 0, 180),        transform); // ↓
        muzzles[2] = Instantiate(upMuzzle, transform.position + Vector3.right * d,
            Quaternion.Euler(0, 0, -90),        transform); // →
        muzzles[3] = Instantiate(upMuzzle, transform.position + Vector3.left  * d,
            Quaternion.Euler(0, 0,  90),        transform); // ←
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRb = player?.GetComponent<Rigidbody2D>();
        
        
        if (pointA && pointB) currentPatrolTarget = pointB.position;
        else currentPatrolTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
    }
    
    

    void Start()
    {
        pointA?.SetParent(null);
        pointB?.SetParent(null);
    }

    #region Unity loop
    void FixedUpdate()
    {


        stateTimer += Time.fixedDeltaTime;

        switch (state)
        {
            case AIState.Wait: DoWait(); break;
            case AIState.Patrol:   DoPatrol();      break;
            case AIState.Aim:      DoAim();         break;
            case AIState.Shoot:    /* handled by coroutine */ break;
            case AIState.Cooldown: DoCooldown();    break;
        }
    }

    void DoWait()
    {
        if (player.currentState is PlayingState)
        {
            ChangeState(AIState.Patrol);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
    #endregion

    #region State Methods
    void DoPatrol()
    {
        SteerTowards(currentPatrolTarget, patrolSpeed);

        // reached node?
        if (Vector2.Distance(transform.position, currentPatrolTarget) < nodeTolerance)
        {
            PickNextPatrolTarget();
        }

        if (PlayerInRange()) ChangeState(AIState.Aim);
    }

    void DoAim()
    {
        Vector2 predicted = PredictedPlayerPos();
        Vector2 toPlayer = predicted - (Vector2)transform.position;
        float dist = toPlayer.magnitude;
        print(dist);

        Vector2 desired;
        float tolerance = 0.2f;
        float diff = dist - hoverDistance;

        if (Mathf.Abs(diff) > tolerance) {
            float sign = Mathf.Sign(diff); 
            desired = toPlayer.normalized * (patrolSpeed * sign);
        }
        else { 
            desired = Vector2.zero;
        }
        Steer(desired);
        Steer(desired);
        // transform.up = toPlayer;
        if (stateTimer > 1f) ChangeState(AIState.Shoot);
        else if (!PlayerInRange()) ChangeState(AIState.Patrol);
    }

    void DoCooldown()
    {
        if (stateTimer > cooldownTime)
        {
            ChangeState(PlayerInRange() ? AIState.Aim : AIState.Patrol);
        }
    }
    #endregion

    #region Transitions
    void ChangeState(AIState next)
    {
        StopAllCoroutines();
        state = next;
        stateTimer = 0f;

        if (next == AIState.Shoot) StartCoroutine(BurstFire());
    }

    IEnumerator BurstFire()
    {
        state = AIState.Shoot;

        for (int i = 0; i < burstCount; i++)
        {
            for (int j = 0; j < muzzles.Length; j++)
            {
                Transform muzzleTransform = muzzles[j].transform;
                Vector2 dir = muzzleTransform.up;
                FireBullet(dir, muzzleTransform);
            }

            yield return new WaitForSeconds(burstCadence);
        }

        ChangeState(AIState.Cooldown);
    }

    #region Shooting helpers
    void FireBullet(Vector2 dir, Transform muzzleTransform)
    {
        anim.SetTrigger(Shoot);
        EnemyBullet bullet = Instantiate(bulletPrefab, muzzleTransform.position, Quaternion.FromToRotation(Vector3.right, dir)).GetComponent<EnemyBullet>();
        bullet.follow = false;
        bullet.rb.linearVelocity = dir * bulletSpeed;
        RuntimeManager.PlayOneShot("event:/SFX/Game/Flying Shooter Shoot");
    }
    #endregion
    #endregion
    
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

    #region Steering & Utility
    bool PlayerInRange()
    {
        return (player.transform.position - transform.position).sqrMagnitude <= detectionRadius * detectionRadius;
    }

    Vector2 PredictedPlayerPos()
    {
        if (playerRb == null) return player.transform.position;
        return (Vector2)player.transform.position + playerRb.linearVelocity * leadTime;
    }

    void SteerTowards(Vector2 targetPos, float speed)
    {
        Vector2 desired = (targetPos - (Vector2)transform.position).normalized;
        if (Vector2.Distance(desired * speed, player.transform.position) > hoverDistance)
        {
            ChangeState(AIState.Aim);
        }
        Steer(desired);
    }

    void Steer(Vector2 desiredVel)
    {
        Vector2 dir = desiredVel.normalized;
        RaycastHit2D hit = Physics2D.CircleCast(transform.position,
                                                0.5f,
                                                dir,
                                                avoidanceLookAhead,
                                                obstacleMask);
        if (hit)
        {
            Vector2 away = ((Vector2)transform.position - hit.point).normalized;
            desiredVel += away * avoidanceForce;
        }

        Vector2 accel = desiredVel - rb.linearVelocity;
        rb.AddForce(accel, ForceMode2D.Force);
        
        if (rb.linearVelocity.sqrMagnitude > .01f) transform.up = rb.linearVelocity;
    }

    void PickNextPatrolTarget()
    {
        if (pointA && pointB)
        {
            currentPatrolTarget = Vector2.Distance(currentPatrolTarget, pointA.position) < 0.01f ? pointB.position : pointA.position;
        }
        else
        {
            currentPatrolTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRadius;
        }
    }
    #endregion
}
