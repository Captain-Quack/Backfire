using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using FMODUnity;

namespace Backfire
{
    /// <summary>Projectile spawned by <c>GunController</c>.</summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
    public class PlayerBullet : MonoBehaviour, IParryable
    {
        [HideInInspector] public BulletData Data;       

        public float funValue = 1;

        protected Rigidbody2D _rb;
        protected SpriteRenderer _sr;
        

        void Awake()
        {
            print("awake");
            if (Data == null) gameObject.SetActive(false);
        }

        [Header("Fancy Effects Stuffs")]

        [SerializeField] private CinemachineImpulseSource impulseSource;

        [SerializeField] private Transform bulletSpawnPoint;

        [SerializeField] private ParticleSystem muzzleFlash;

        [SerializeField] private PlayerController player;



        public void Initialize(BulletData bd, Vector2 dir, float massKg, float muzzleVel)
        {
            if (muzzleFlash != null)
            {
                muzzleFlash.transform.position = bulletSpawnPoint.position;
                muzzleFlash.transform.rotation = Quaternion.LookRotation(Vector3.forward, player.mouseAim.AimDirection);
                muzzleFlash.Play();
            }

            _rb = GetComponent<Rigidbody2D>();
            _sr = GetComponent<SpriteRenderer>();
            impulseSource = FindAnyObjectByType<CinemachineImpulseSource>();

            Data = bd;

            gameObject.SetActive(true);
            _sr.sprite = bd.sprite;
            Destroy(gameObject, bd.lifetime);
            _rb.mass = massKg;
            _rb.AddForce(dir * ((funValue / 2) * muzzleVel), ForceMode2D.Impulse);
            transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
            impulseSource.GenerateImpulseWithVelocity(dir * muzzleVel / 1.5f);

            
                
            TrailRenderer trail = Instantiate(bd.bulletTrail, transform.position, transform.rotation);
            trail.transform.SetParent(transform, true);
            trail.emitting = true;
        }
        
        
        

        public float bounceMultiplier = 15f;
        void OnCollisionEnter2D(Collision2D col)
        {
            print("bullet collided: " + col.gameObject.name);
            for (int i = 0; i < Data.damage; i++)
            {
                if (col.gameObject.CompareTag("Enemy"))
                {
                    col.collider.GetComponent<IDamageable>()?.TakeDamage();
                }
            }

            if (col.gameObject.CompareTag("Button"))
            {
                // ButtonGate.Instance.RemoveGates();
                // ButtonGate.Instance.ChangeButtonColor();
                gameObject.SetActive(false);
                //RuntimeManager.PlayOneShot("event:/SFX/Game/Button");
                return;
            }
            else if (col.gameObject.CompareTag("Bounce"))
            {
                Vector2 incomingVelocity = _rb.linearVelocity;
                Vector2 normal = col.contacts[0].normal;
                Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal) * bounceMultiplier;
                RuntimeManager.PlayOneShot("event:/SFX/Game/Bullet Bounce");
                _rb.linearVelocity = reflectedVelocity;
                return;
            }
            /*else if (col.gameObject.CompareTag("Environment"))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Wall Hit");
            }*/
            else if (col.gameObject.CompareTag("Laser"))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Laser Hit");
            }
            else if (col.gameObject.CompareTag("Shield"))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Shield Hit");
            }
            else if (col.gameObject.CompareTag("Enemy"))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Enemy Hit");
            }
            gameObject.SetActive(false);
         }
        public void Parry(Vector2 newDir, bool isPerfect = false)
        {
            _rb.linearVelocity = newDir * _rb.linearVelocity.magnitude;
            transform.rotation = Quaternion.LookRotation(Vector3.forward, newDir);
            StartCoroutine(FlashSprite());
            if (isPerfect)
            {
                StartCoroutine(FlashSprite());
            }
        }

        IEnumerator FlashSprite()
        {
            _sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            _sr.color = Color.red;      
        }
        
        
        
    }

    public class PlayerLaserBullet : PlayerBullet
    {
        private int pierceCount;
        
        public new void Initialize(BulletData bd, Vector2 dir, float massKg, float muzzleVel)
        {
            base.Initialize(bd, dir, massKg, muzzleVel);
            pierceCount = 0;
        }

        void OnCollisionEnter2D(Collision2D col)
        {
            print("laser collided: " + col.gameObject.name);
            for (int i = 0; i < Data.damage; i++) col.collider.GetComponent<IDamageable>()?.TakeDamage();
            
            if (col.gameObject.CompareTag("Button"))
            {
                gameObject.SetActive(false);
                RuntimeManager.PlayOneShot("event:/SFX/Game/Button");
                return;
            }
            else if (col.gameObject.CompareTag("Bounce"))
            {
                Vector2 incomingVelocity = _rb.linearVelocity;
                Vector2 normal = col.contacts[0].normal;
                Vector2 reflectedVelocity = Vector2.Reflect(incomingVelocity, normal) * bounceMultiplier;

                _rb.linearVelocity = reflectedVelocity;
                return;
            }
            else if (col.gameObject.CompareTag("Gate") || col.gameObject.CompareTag("EnemyBullet") || col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("PlayerBullet") || col.gameObject.CompareTag("Timer"))
            {
                pierceCount++;
                if (pierceCount >= 5)
                {
                    Destroy(gameObject);
                }

                return; 
            }
            
            gameObject.SetActive(false);
            RuntimeManager.PlayOneShot("event:/SFX/Game/Metal Hit");
        }
        
        
    }
}



