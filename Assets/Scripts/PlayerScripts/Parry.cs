using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

namespace Backfire
{
    [DisallowMultipleComponent]
    public sealed class Parry : MonoBehaviour
    {
        [Header("Windows (seconds)")]
        [SerializeField] float startup = 0.05f;
        [SerializeField] float recovery = 0.15f;

        [Header("Gameplay")]
        [SerializeField] float parryRange = 1.8f;
        [SerializeField] float parryLaunchForce = 1.2f;
        [SerializeField] private LayerMask parryMask;

        [Header("VFX")]
    [SerializeField] private Material parrySuccessMaterial;
    [SerializeField] private Material parryFailMaterial;
    [SerializeField] private Renderer parryRenderer;    
    [SerializeField] private ParticleSystem parryParticles;

        readonly Collider2D[] hitCache = new Collider2D[16];

        Rigidbody2D body;
        ShockwaveManager shockwave;
        PlayerController playerController;

        [SerializeField] float inputBuffer = 0.08f;
        bool bufferedInput;
        float lastInputTime;
        bool parryRunning;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            playerController = GetComponent<PlayerController>();

            if (!parryParticles)
                parryParticles = GameObject.Find("Parry!").GetComponent<ParticleSystem>();

            if (ShockwaveManager.Instance)
                shockwave = ShockwaveManager.Instance;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                bufferedInput = true;
                lastInputTime = Time.unscaledTime;
            }
            
            if (parryRunning) return;

            if (bufferedInput && Time.unscaledTime - lastInputTime <= inputBuffer)
            {
                bufferedInput = false;
                parryRunning = true;
                StartCoroutine(ParryRoutine());
            }
        }
        IEnumerator ParryRoutine()
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;

            AudioListener.pause = true;

            var rbs = Physics2D.OverlapCircleAll(transform.position, 20f, parryMask)
                .Select(x => x.attachedRigidbody)
                .Where(rb => rb != null)
                .ToList();

            rbs.Add(playerController.body);

            foreach (var rb in rbs)
            {
                rb.linearVelocity = Vector2.zero;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                rb.bodyType = RigidbodyType2D.Static;
                rb.Sleep();
            }

            yield return new WaitForSecondsRealtime(startup);

            PerformParryCheck();

            yield return new WaitForSecondsRealtime(recovery);

            AudioListener.pause = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
            var playerRb = playerController.body;
            playerRb.constraints = RigidbodyConstraints2D.None;
            playerRb.bodyType = RigidbodyType2D.Dynamic;
            playerRb.WakeUp();
            StartCoroutine(UnfreezeEnemiesAfterDelay(rbs.Except(new[] { playerRb }), extraEnemyPauseTime: 1f));

            parryRunning = false;
        }
        
        IEnumerator UnfreezeEnemiesAfterDelay(IEnumerable<Rigidbody2D> enemyRbs, float extraEnemyPauseTime)
        {
            yield return new WaitForSeconds(extraEnemyPauseTime);

            foreach (var rb in enemyRbs)
            {
                if (!rb) continue;
                rb.constraints = RigidbodyConstraints2D.None;
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.WakeUp();
            }
        }


        bool PerformParryCheck()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, parryRange, hitCache, parryMask);

            bool success = false;

            for (int i = 0; i < count; ++i)
            {
                Collider2D col = hitCache[i];
                if (col.attachedRigidbody == body) continue;

                if (col.TryGetComponent(out IParryable p))
                {
                    Vector2 away = (col.transform.position - transform.position).normalized;
                    p.Parry(away);
                    success = true;
                }

                if (col.TryGetComponent(out EnemyController enemy))
                {
                    success = true;
                }
                
                else if (col.TryGetComponent(out EnemyBullet eb))
                {
                    Destroy(col.gameObject);
                }
            }

            if (success)
            {
                body.linearVelocity += playerController.mouseAim.AimDirection * parryLaunchForce;
                RuntimeManager.PlayOneShot("event:/SFX/Game/Parry");
                shockwave?.CallShockWave(transform.position);
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Parry Attempt");
            }

            var main = parryParticles.main;
            main.startColor = success ? Color.green : Color.gray;
            parryParticles.Play();
            if (parryRenderer != null)
            {
                parryRenderer.material = success ? parrySuccessMaterial : parryFailMaterial;
            }

            return success;
        }
        
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, parryRange);
        }
#endif
    }
}
