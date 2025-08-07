using System.Collections;
using System.Linq;
using UnityEngine;
using FMODUnity;
using UnityEngine.SceneManagement;


namespace Backfire

{

    public class PreState : PlayerState

    {
        private Animator bounceAnim;
        public override PlayerStateType StateType => PlayerStateType.PreState;
        
        private RaycastHit2D[] hitBuffer = new RaycastHit2D[5];
        
        private Rigidbody2D body;

        public PreState(PlayerController player) : base(player)
        {
        }

        public override void Update()
        {
            try
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Launch");
            }
            catch (SystemNotInitializedException)
            {
                Debug.ClearDeveloperConsole();
#if UNITY_EDITOR
                NewUIManager.Instance.ForceOpenConsole();
#endif
                Debug.LogWarning("FMOD is tweaking out!");
            }
                
               
            int results = Physics2D.CircleCastNonAlloc(player.transform.position, 20f, Vector2.down, hitBuffer, 5f);

            GameObject portal = null;
            for (int i = 0; i < results; i++)
            {
                GameObject obj = hitBuffer[i].transform.gameObject;
                if (!obj.name.Contains("Launcher", System.StringComparison.OrdinalIgnoreCase)) continue;
                portal = obj;
                break;
            }
            Debug.Log($"Portal is null: {portal is null}");
            player.SetState(PlayerStateType.Playing);
            bounceAnim?.SetTrigger("Launch");
           player.StartCoroutine(LaunchWait(portal));
        }

        public override void OnEnter()
        {
            body = player.body;
            bounceAnim = GameObject.Find("Launcher")?.GetComponent<Animator>();

        }
        public override void OnExit()
        { 
        }
        
        public override void FixedUpdate() { }

        private IEnumerator LaunchWait(GameObject portal)
        {
            yield return new WaitForSeconds(0.1f);
            if (!SceneManager.GetActiveScene().name.Contains("Level 7"))
            {
                body.AddForce((portal?.transform.up ?? Vector2.up) * 15f, ForceMode2D.Impulse);
            }
            else if (!SceneManager.GetActiveScene().name.Contains("Level 6"))
            {
                body.AddForce(Vector2.right * 7.5f, ForceMode2D.Impulse);
            }
            else
            {
                body.AddForce(Vector2.right * 15f, ForceMode2D.Impulse);
            }
        }
        
    }

}