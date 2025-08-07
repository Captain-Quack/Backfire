using System;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using UnityEngine;
using FMODUnity;
using UnityEngine.SceneManagement;

namespace Backfire
{
    using UnityEngine;
    public sealed class GunController : MonoBehaviour
    {
        public Animator shootAnim;
        public float funValue = 100f;

        [Header("Assign in Scene/Code")] public GunData gunData;

        
        [Header("Assign in Prefab")] public GameObject bulletPrefab;
        public GameObject circlePrefab;
        
        public Transform bulletSpawnPoint;
        private float timeUntilNextShot = 0f;
        
        PlayerController player;

        // Rigidbody2D gunRb;
        private NewUIManager uiManager;

        void Awake()
        {
            player = GetComponentInParent<PlayerController>();
            // gunRb  = GetComponent<Rigidbody2D>();
            // gunRb.mass = gunData.weight;
            if (!TryGetComponent(out SpriteRenderer existingSprite))
            {
                SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = gunData.sprite;
            }
            else
            {
                existingSprite.sprite = gunData.sprite;
            }

            
            SceneManager.sceneLoaded += OnSceneLoad;

        }

        void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            uiManager = NewUIManager.Instance;
            gunData = GunSpawner.Instance.curentGunSelected ?? Resources.Load<GunData>("Guns/00 Glock 17");


        }
        
        void Update()
        {
            
            if (uiManager is  null || uiManager.isSettingsOpen) return;
    
            if (timeUntilNextShot > 0)
            {
                timeUntilNextShot -= Time.unscaledDeltaTime;
            }

            if (Input.GetMouseButtonDown(0) && timeUntilNextShot <= 0 && player.currentState is PlayingState state)
            {
                Shoot(state);
                Destroy(Instantiate(circlePrefab, player.transform.position, Quaternion.identity, null), 1f);

            }
                
                

        }

        const float GrainsToKg = 0.06479891f / 1000f; 
        void Shoot(PlayingState state)
        {
            // Reset cooldown timer
            //Debug.Log("stuff is happeing");
            timeUntilNextShot = gunData.cooldown;

            // the discord has all of my research on this
            //var go = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
            //var bullet = go.GetComponent<PlayerBullet>();
            shootAnim.SetTrigger("Shot");
            float bulletMassKg = gunData.bulletData.weightInGrains * GrainsToKg;
            // muzzle velocity in meters per second
            float muzzleVelMps = gunData.muzzleVelocityFPS * 0.3048f;
            // kinetic energy imparted to bullet 
            float kineticEnergy = 0.5f * bulletMassKg * muzzleVelMps * muzzleVelMps;
            // assume 33% of propellant energy goes into bullet KE:
            float propellantEnergy = kineticEnergy / 0.33f;
            // typical energetic density of smokeless powder ~ 4 MJ/kg -> 4 000 000 J/kg
            float powderEnergyDensity = 4_000_000f;
            // mass of powder burned:
            float powderMassKg = propellantEnergy / powderEnergyDensity;
            // gas exit velocity (roughly 1.7x the bullet's muzzle velocity):
            float gasExitVelMps = muzzleVelMps * 1.7f;

            float p_bullet = bulletMassKg * muzzleVelMps;
            float p_gas = powderMassKg * gasExitVelMps;
            float p_total = p_bullet + p_gas;

            Debug.Log($"[Shoot] bulletMass={bulletMassKg:F4} kg, muzzleVel={muzzleVelMps:F1} m/s, " +
                      $"powderMass={powderMassKg:E3} kg, p_bullet={p_bullet:F2}, p_gas={p_gas:F2}, p_total={p_total:F2}");
            
            Vector2 dir = player.mouseAim.AimDirection.normalized;
            int pelletCount = 1;

            if (gunData.action == GunAction.PumpAction)
            {
                pelletCount  = Random.Range(3, 8);
        
                for (int i = 0; i < pelletCount; i++)
                {
                    float angle = Random.Range(-60f, 60f);
                    Vector2 rotatedDir = RotateVector(dir, angle);            
                    var pelletGo = Instantiate(bulletPrefab, bulletSpawnPoint.position + Vector3.up, Quaternion.identity);
                    var pelletBullet = pelletGo.GetComponent<PlayerBullet>();
                    pelletBullet.Initialize(gunData.bulletData, rotatedDir, bulletMassKg, p_total / pelletCount);
                    p_total *= 1.2f;
                }
            }
            else
            {
                var singleBullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);
                if (gunData.action == GunAction.Laser)
                {
                    Destroy(singleBullet.GetComponent<PlayerBullet>());
                    singleBullet.AddComponent<PlayerLaserBullet>().Initialize(gunData.bulletData, dir, bulletMassKg, p_total);
                    
                }
                else
                {
                    singleBullet.GetComponent<PlayerBullet>().Initialize(gunData.bulletData, dir, bulletMassKg, p_total);
                }
            }

            float playerMassKg = player.body.mass;
            float recoilVel = p_total / playerMassKg;
            print(state.isSlowMotion);
            var slowMod = player.body.linearVelocity * (state.isSlowMotion ? SlowMotionVelocityRetention : RegularVelocityRetention); 
            player.body.linearVelocity = -dir * (recoilVel * funValue) + slowMod;
            RuntimeManager.PlayOneShot(gunData.sfxPath);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector2 RotateVector(Vector2 v, float angleDegrees)
        {
            float angleRadians = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angleRadians);
            float sin = Mathf.Sin(angleRadians);

            float x = v.x * cos - v.y * sin;
            float y = v.x * sin + v.y * cos;

            return new Vector2(x, y);
        }
        [Range(0f, 2f)] public float RegularVelocityRetention = 1f;
        [Range(0f, 2f)] public float SlowMotionVelocityRetention = 1f;
    }
    
    public enum GunAction
    {
        /// <summary>

        /// Single-shot actions are the simplest firearm actions, requiring manual loading of a single cartridge directly into the chamber before each shot. After firing, the user manually extracts the spent cartridge.

        /// </summary>

        SingleShot,
        
        /// <summary>

        /// Bolt-action firearms utilize a manually operated bolt to chamber, lock, and unlock cartridges. The bolt is typically manipulated by a handle on the side of the receiver.

        /// </summary>

        BoltAction,
        
        /// <summary>

        /// Pump-action firearms, also known as slide-action firearms, utilize a sliding fore-end to cycle the action. Pushing the fore-end forward chambers a new cartridge, while pulling it back extracts the spent casing.

        /// </summary>

        PumpAction,
        
        /// <summary>

        /// Revolvers utilize a rotating cylinder containing multiple chambers. Each chamber holds a single cartridge, which is rotated into alignment with the barrel for firing.

        /// </summary>

        Laser,
        
        /// <summary>

        /// Semi-automatic firearms, also known as self-loading firearms, automatically reload the next cartridge after each shot. The energy from the firing of the previous round cycles the action, extracting the spent casing and chambering a new cartridge.

        /// </summary>

        SemiAuto,
    }
}