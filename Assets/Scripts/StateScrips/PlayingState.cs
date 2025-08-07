using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

using UnityEngine.InputSystem;

using UnityEngine.Rendering;

using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;


namespace Backfire
{
    public class PlayingState : PlayerState
    {
        public override PlayerStateType StateType => PlayerStateType.Playing;
        
        public PlayingState(PlayerController player) : base(player) {}

        float _normalTimeScale;

        float _normalFixedDeltaTime;

        public bool isSlowMotion;
        public Volume volume;

        private ChromaticAberration chromaticAberration;
        private EventInstance timeSlowInst;
        private EventInstance tsSnapshotInst;
        private ClockAnimations clock;
        private Camera camera;

        private void Start()
        {
            camera = Camera.main;

            
        }

        public override void OnEnter()
        {

            player.GetComponent<Rigidbody2D>().gravityScale = 3f;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f; 
            _normalTimeScale = Time.timeScale;
            _normalFixedDeltaTime = Time.fixedDeltaTime;
            isSlowMotion = false;
            clock = Object.FindAnyObjectByType<ClockAnimations>();
            clock.Reset();


        }

        void CreateVolume()
        {
            var volumeGO = new GameObject("Post Processing Volume");

            volume = volumeGO.AddComponent<Volume>();

            volume.isGlobal = true;

            volume.priority = 1;

            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

            volumeGO.transform.parent = camera.transform;
            
            var chroma = volume.profile.Add<ChromaticAberration>(true);
            var bloom = volume.profile.Add<Bloom>(true);
            bloom.intensity.value = 0.5f;
            bloom.threshold.value = 0.5f;
            bloom.active = true;
            chroma.intensity.value = 0.5f;

            //chroma.active = true;
        }
        bool _dying;   // NEW

        public override bool TakeDamage()
        {
            if (_dying) return false;      // already processing a death
            _dying = true;

            // make absolutely sure we’re back to real-time
            Time.timeScale      = 1f;
            Time.fixedDeltaTime = 0.02f;

            player.SetState(PlayerStateType.PreState);   // freeze controls & animations
            player.StartCoroutine(WhompWhomp());
            return true;
        }


        private IEnumerator WhompWhomp()
        {
            LevelSelectManager.Instance.CurrentLevel.Deaths++;
            Time.timeScale = _normalTimeScale;
            Time.fixedDeltaTime = _normalFixedDeltaTime;
            return NewUIManager.Instance.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        public override void OnExit()
        {



        }



        



        

        public override void Update()
        {
            if (_dying) return;
            bool spaceHeld = Keyboard.current.spaceKey.isPressed;
            
            if (spaceHeld && clock.canFreeze)
            {
                Time.timeScale = player.slowMotionScale;
                Time.fixedDeltaTime = _normalFixedDeltaTime * player.slowMotionScale;

                if (!isSlowMotion)
                {
                    timeSlowInst = RuntimeManager.CreateInstance("event:/SFX/Game/Time Slow");
                    timeSlowInst.start();
                    tsSnapshotInst = RuntimeManager.CreateInstance("snapshot:/Timeslow");
                    tsSnapshotInst.start();
                }

                isSlowMotion = true;

                if (chromaticAberration != null)
                {
                    chromaticAberration.active = true;
                    chromaticAberration.intensity.value = 0.7f;
                }
            }
            else if (!Mathf.Approximately(Time.timeScale, _normalTimeScale))
            {
                Time.timeScale = _normalTimeScale;
                Time.fixedDeltaTime = _normalFixedDeltaTime;
                isSlowMotion = false;

                timeSlowInst.setParameterByName("Resume", 1);
                timeSlowInst.release();
                tsSnapshotInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                tsSnapshotInst.release();

                // Disable chromatic aberration
                if (chromaticAberration != null)
                {
                    chromaticAberration.active = false;
                }
            }
        }

        public override void FixedUpdate()
        {
            Vector2 dir = player.mouseAim.AimDirection;
            float desired = player.atan2(dir.x, dir.y) * Mathf.Rad2Deg - 90f;
            float step = player.angularSpeedDegPerSec * Time.unscaledDeltaTime;
            float newRot = Mathf.MoveTowardsAngle(player.body.rotation, desired, step);
            player.body.rotation = newRot;
        }

        

        

    }

}