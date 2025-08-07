using System.Collections;
using Backfire;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class FlyingEnemy : MonoBehaviour
{
    private static readonly int Boom = Animator.StringToHash("BOOM");

    #region Inspector

    [Header("Explosion")]
    [SerializeField] private float explosionRadius;

    [SerializeField] private bool boomBoom;

    [Header("General")]
    [SerializeField] private float detectionRadius = 50f;
    [SerializeField] private float wanderSpeed = 3f;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float avoidanceForce = 10f;
    [SerializeField] private float avoidanceLookAhead = 1.5f;

    [Header("Attack")]
    [Tooltip("How far ahead of the player we aim (in seconds).")]
    [SerializeField] private float leadTime = 0.25f;
    [SerializeField] private float windUpTime = 0.35f;
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float cooldownTime = 1.75f;

    [Header("FX hooks")]
    [SerializeField] private ParticleSystem chargeFx;
    [SerializeField] private ParticleSystem dashFx;
    #endregion

    enum AIState { Wait, Wander, Chase, WindUp, Dash, Cooldown }
    AIState state = AIState.Wait;

    Rigidbody2D rb;
    PlayerController player;
    Rigidbody2D playerRb;

    float stateTimer;
    Vector2 wanderDir;
    public Animator anim;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRb = player?.GetComponent<Rigidbody2D>();
        PickNewWanderDir();
    }

    #region Unity loops
    void FixedUpdate()
    {
        stateTimer += Time.fixedDeltaTime;
        switch (state)
        {
            case AIState.Wait: DoWait(); break;
            case AIState.Wander: DoWander(); break;
            case AIState.Chase: DoChase(); break;
            case AIState.WindUp:   /* stand still */ break;
            case AIState.Dash:     /* physics handled in routine */ break;
            case AIState.Cooldown: break;
        }
    }



    void DoWait()
    {
        if (player.currentState is PlayingState)
        {
            ChangeState(AIState.Wander);
        }
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
        {
            StartCoroutine(Touch());
            return;
        }


    }
    #endregion
    private IEnumerator Touch()
    {
        anim.SetTrigger(Boom);
        player.IsFrozen = true;
        StartCoroutine(FlashRed());
        player.body.gravityScale = 0f;
        player.body.linearVelocity = Vector2.zero;
        yield return new WaitForSecondsRealtime(0.3f);
        RuntimeManager.PlayOneShot("event:/SFX/Game/Bomber Explode");
        Destroy(gameObject);

        player.TakeDamage();
        yield return null;
    }

    IEnumerator FlashRed()
    {
        Color normal = Color.white;
        Color red = Color.red;
        SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();
        SpriteRenderer enemySprite = gameObject.GetComponent<SpriteRenderer>();


        while (true)
        {
            playerSprite.color = red;
            enemySprite.color = red;
            yield return new WaitForSecondsRealtime(0.2f);
            playerSprite.color = normal;
            enemySprite.color = normal;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    #region Behaviours
    void DoWander()
    {
        if (stateTimer > 1.5f) PickNewWanderDir();

        Steer(wanderDir * wanderSpeed);

        if (PlayerInRange()) ChangeState(AIState.Chase);
    }

    void DoChase()
    {
        Vector2 toPredicted = PredictedPlayerPos() - (Vector2)transform.position;
        Vector2 desired = toPredicted.normalized * chaseSpeed;
        Steer(desired);

        if (toPredicted.magnitude < 2.5f) ChangeState(AIState.WindUp);
        else if (!PlayerInRange()) ChangeState(AIState.Wander);
    }
    #endregion

    #region Helpers
    bool PlayerInRange()
    {
        var dist = (player.transform.position - transform.position).sqrMagnitude;
        if (dist <= explosionRadius * explosionRadius) StartCoroutine(Touch());
        return dist <= detectionRadius * detectionRadius;
    }

    Vector2 PredictedPlayerPos()
    {
        if (playerRb == null) return player.transform.position;
        return (Vector2)player.transform.position + playerRb.linearVelocity * leadTime;
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

        Vector2 neededAccel = desiredVel - rb.linearVelocity;
        rb.AddForce(neededAccel, ForceMode2D.Force);
        transform.up = rb.linearVelocity;
    }

    void PickNewWanderDir()
    {
        wanderDir = Random.insideUnitCircle.normalized;
        stateTimer = 0f;
    }

    void ChangeState(AIState next)
    {
        StopAllCoroutines();
        state = next;
        stateTimer = 0f;

        switch (next)
        {
            case AIState.WindUp: StartCoroutine(WindUpAndDash()); break;
            case AIState.Wander: PickNewWanderDir(); break;
        }
    }
    #endregion

    #region Attack routines
    IEnumerator WindUpAndDash()
    {
        rb.linearVelocity = Vector2.zero;
        // chargeFx?.Play();

        yield return new WaitForSeconds(windUpTime);

        // Dash
        Vector2 dir = (PredictedPlayerPos() - (Vector2)transform.position).normalized;
        rb.AddForce(dir * dashForce, ForceMode2D.Impulse);
        // dashFx?.Play();
        ChangeState(AIState.Dash);

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity *= 0.25f;
        ChangeState(AIState.Cooldown);

        yield return new WaitForSeconds(cooldownTime);
        ChangeState(PlayerInRange() ? AIState.Chase : AIState.Wander);
    }
    #endregion
}
