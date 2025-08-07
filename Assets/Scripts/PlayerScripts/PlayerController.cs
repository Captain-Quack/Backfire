using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using FMODUnity;

namespace Backfire
{
    public enum PlayerStateType
    {
        PreState,
        Playing,
        Death
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : Controller
    {
        // Zoom curve constants
        public float ZoomA = -1.5f;
        public float ZoomB = 0.1f;
        public float ZoomK = 60f;
        public float ZoomOffset = -6f;
    

        [Header("References")]
        [SerializeField] public Rigidbody2D body;
        [SerializeField] public MouseAim mouseAim;
        [SerializeField] private CinemachineCamera virtualCamera;

        [Header("Tuning")]
        [Range(0.01f, 1f)]
        public float slowMotionScale = 0.2f;
        public float angularSpeedDegPerSec = 720f;

        public PlayerState currentState;
        public bool IsFrozen { get; set; } = false;
        private void Awake()
        {
            body ??= GetComponent<Rigidbody2D>();
            mouseAim ??= GetComponent<MouseAim>();
            virtualCamera ??= FindAnyObjectByType<CinemachineCamera>();
            if (virtualCamera == null)
                Debug.LogError("PlayerController: No CinemachineVirtualCamera found.");
            SetState(PlayerStateType.PreState);

        }

        private void Update()
        {
            if (NewUIManager.Instance && NewUIManager.Instance.isSettingsOpen) return;
            if (IsFrozen) return;

            if (virtualCamera != null)
            {
                var speed = body.linearVelocity.magnitude;
                var cur = virtualCamera.Lens.OrthographicSize;
                var tgt = (ZoomK / (1 + Mathf.Exp(ZoomA + ZoomB * Mathf.Abs(speed)))) - ZoomOffset;
                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(cur, tgt, Time.deltaTime);
            }

            currentState?.Update();
        }
        private void FixedUpdate()
        {
            if (IsFrozen) return;
            currentState?.FixedUpdate();
        }
        public void SetState(PlayerStateType newStateType)
        {
            currentState?.OnExit();
            currentState = newStateType switch
            {
                PlayerStateType.PreState => new PreState(this),
                PlayerStateType.Playing => new PlayingState(this),
                PlayerStateType.Death => new DeathState(this),
                _ => throw new ArgumentOutOfRangeException(nameof(newStateType), newStateType, null)
            };
            currentState.OnEnter();
        }

        public override bool TakeDamage()
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Death");
            return currentState?.TakeDamage() ?? false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Environment"))
            {
                Debug.Log("Player hit walls");
                TakeDamage();
                return;
            }

            if (other.CompareTag("Win"))
            {
                if (GameManager.Instance)
                {
                    int killed = GameManager.Instance.numEnemiesKilled;
                    int total = GameManager.Instance.numEnemies;
                    Debug.Log($"Entered win zone: {killed}/{total} enemies killed");
                    if (killed < total) 
                    { 
                        StartCoroutine(FlashSprite(other.gameObject));
                        return;
                        
                    }
                    GameManager.Instance.WinGame();
                    GameManager.Instance.GameIsRunning = false;
                }
                else
                {
                    Debug.LogError("You won this level but there is no game manager ");
                }

                NewUIManager.Instance.winScreen.SetActive(true);
                StartCoroutine(FlashSprite(other.gameObject));
            }
        }


        IEnumerator FlashSprite(GameObject obj)
        {
            var _sr =  obj.GetComponent<SpriteRenderer>();
            _sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            _sr.color = Color.red;   
            _sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            _sr.color = Color.red;   
        }

        



        private void OnCollisionEnter2D(Collision2D other)
        {
            
            if (other.gameObject.CompareTag("EnemyBullet"))
            {
                Destroy(other.gameObject);
            }

            if (other.gameObject.CompareTag("Trap"))
            {
                TakeDamage();
            }

            if (other.gameObject.CompareTag("Environment"))
            {
                Debug.Log("Player hit walls");
                TakeDamage();
            }
            if (other.gameObject.CompareTag("Button"))
            {
                Debug.Log("player hit the button womp womp");
                TakeDamage();
            }
        }

        // Atan2 approximation constants
        private const float PI_HALF = Mathf.PI / 2f;
        private const float a1 = 0.99997726f;
        private const float a3 = -0.33262347f;
        private const float a5 = 0.19354346f;
        private const float a7 = -0.11643287f;
        private const float a9 = 0.05265332f;
        private const float a11 = -0.01172120f;

        public float atan2(float x, float y)
        {
            bool swap = Mathf.Abs(x) < Mathf.Abs(y);
            float input = (swap ? x : y) / (swap ? y : x);
            float res = atan_scalar_approximation(input);

            res = swap ? (input >= 0.0f ? PI_HALF : -PI_HALF) - res : res;

            if (x >= 0.0f && y >= 0.0f) { }                     // 1st quadrant
            else if (x < 0.0f && y >= 0.0f) { res = Mathf.PI + res; } // 2nd quadrant
            else if (x < 0.0f && y < 0.0f) { res = Mathf.PI + res; } // 3rd quadrant
            else if (x >= 0.0f && y < 0.0f) { }                     // 4th quadrant
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float atan_scalar_approximation(float x)
        {
            float x_sq = x * x;
            return
                x * (a1 + x_sq * (a3 + x_sq * (a5 + x_sq * (a7 + x_sq * (a9 + x_sq * a11)))));
        }
        public void TickUp()
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Tick Up");
        }
        public void TickDown()
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Tick Down");
        }

    }
    
    
    
}
