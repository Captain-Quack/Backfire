using System;
using System.Collections;
using Backfire;
using UnityEngine;
using FMODUnity;

public class FogEnemy : MonoBehaviour
{
    [Header("Fog Params")] 
    [SerializeField] private ParticleSystem fog;
    [SerializeField] private int particlesPerBurst = 10;
    [SerializeField] private float fogDamping = 1f;

    [Header("Regular Exudation")]
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float fogCadence = 0.5f;
    [SerializeField] private float fogCooldown = 5f;
    [SerializeField] private float fogRange = 10f;

    [Header("Aggressive Exudation")]
    [SerializeField] private float aggressionRadius = 15f;
    [SerializeField] private int aggressionBurstCount = 5;
    [SerializeField] private float aggressionCadence = 0.3f;

    
    private enum State { Release, Cooldown, Aggro }
    private State state = State.Release;
    private float stateTimer = 0f;
    private Transform target;
    private Coroutine stateRoutine;

    private void Awake()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null) target = playerGO.transform;
    }

    private void Start()
    {
        EnterState(State.Release);
    }

    private void Update()
    {
        stateTimer += Time.deltaTime;
    }

    private void EnterState(State newState)
    {
        if (stateRoutine != null)
            StopCoroutine(stateRoutine);

        state = newState;
        stateTimer = 0f;

        stateRoutine = state switch
        {
            State.Release => StartCoroutine(DoRelease()),
            State.Cooldown => StartCoroutine(DoCooldown()),
            State.Aggro => StartCoroutine(DoAggro()),
            _ => null
        };
    }

    private IEnumerator DoRelease()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Fogger Fog");
        for (int i = 0; i < burstCount; i++)
        {
            fog.Emit(particlesPerBurst);
            yield return new WaitForSeconds(fogCadence);
        }
        EnterState(State.Cooldown);
    }

    private IEnumerator DoCooldown()
    {
        while (stateTimer < fogCooldown)
            yield return null;

        EnterState(PlayerInRange() ? State.Aggro : State.Release);
    }

    private IEnumerator DoAggro()
    {
        for (int i = 0; i < aggressionBurstCount; i++)
        {
            fog.Emit(particlesPerBurst);
            yield return new WaitForSeconds(aggressionCadence);
        }
        EnterState(State.Cooldown);
    }

    private bool PlayerInRange()
    {
        if (target == null) return false;

        return (target.position - transform.position).sqrMagnitude
               <= aggressionRadius * aggressionRadius;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Vector2 offset = box.offset / 2;
            Vector2 size = box.size / 2;

            Vector3 position = transform.position + (Vector3)offset;
            Gizmos.DrawWireCube(position, size);
        }

        Gizmos.color = new (1f, 0.5f, 0f, 0.3f); // orange-ish
        Gizmos.DrawWireSphere(transform.position, aggressionRadius);
    }

    private Coroutine deathRoutine;
    private float originalDamping;
    private void OnTriggerEnter2D(Collider2D other)
    {
        print("Entered Trigger");
        if (!other.CompareTag("Player") || deathRoutine != null) return;
        if (!other.TryGetComponent(out PlayerController player)) return;
        
        originalDamping = player.body.linearDamping;
        player.body.linearDamping = fogDamping;

        deathRoutine = StartCoroutine(Corrode(player));
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        other.attachedRigidbody.linearVelocity /= 1.5f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        print("Exit Trigger");
        if (!other.CompareTag("Player") || deathRoutine == null) return;
        if (!other.TryGetComponent(out PlayerController player)) return;
        StopCoroutine(deathRoutine);
        player.body.linearDamping = originalDamping;
        deathRoutine = null;
    }
    
    

    private IEnumerator Corrode(PlayerController player)
    {
        yield return new WaitForSeconds(fogCooldown);
        player.TakeDamage();
        player.body.linearDamping = originalDamping;
        deathRoutine = null;
    }

}
