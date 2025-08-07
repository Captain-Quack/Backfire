using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Backfire;
using FMODUnity;

public class ClockAnimations : MonoBehaviour
{
    public Animator animator;
    public bool canFreeze = true;
    private bool beginning = true;
    private PlayerController player;
    private bool rewinding = false;

    private bool? IsSlowed
    {
        get
        {
            if (player.currentState is PlayingState state) return state.isSlowMotion;
            return null;
        }
    }
    [Header("SlowMo Resource Settings")]
    [SerializeField] float chargeMax = 3f;
    [SerializeField] float drainPerSecond = 1f;
    [SerializeField] float rechargePerSecond = 0.5f;
    [SerializeField] float overheatLockout = 2f;

    [SerializeField] float chargeCurrent;
    bool overheated;

    SpriteRenderer playerSprite;
    static readonly Color OverheatColour = new(1f, 0.25f, 0.25f, 1f);
    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int SlowTime = Animator.StringToHash("SlowTime");
    private static readonly int Backward = Animator.StringToHash("Backward");

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
        player = FindAnyObjectByType<PlayerController>();
        chargeCurrent = chargeMax;

        playerSprite = player.GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        transform.position = player.gameObject.transform.position;

        animator.Update(dt);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float currentTime = stateInfo.normalizedTime % 1f;

        if (rewinding || !Keyboard.current.spaceKey.isPressed)
            chargeCurrent = Mathf.Min(chargeMax, chargeCurrent + rechargePerSecond * dt);

        bool wantsSlow = Keyboard.current.spaceKey.isPressed &&
                         !overheated && canFreeze && player.currentState is PlayingState;

        if (wantsSlow)
        {
            chargeCurrent -= drainPerSecond * dt;
            if (chargeCurrent <= 0.02f)
            {
                chargeCurrent = 0f;
                overheated = true;
                StartCoroutine(HandleOverheat());
                return;
            }

            animator.speed = 1f;
            animator.SetFloat(Speed, 2f);
            rewinding = false;

            if (beginning)
            {
                animator.Play("SlowTime", 0, 0f);
                beginning = false;
            }
            else
            {
                animator.Play("SlowTime", 0, currentTime);
            }
        }

        if (player.currentState is PlayingState && Keyboard.current.spaceKey.wasReleasedThisFrame && !overheated)
        {
            animator.SetFloat(Speed, -1f);
            animator.speed = 1f;
            animator.Play(SlowTime, 0, currentTime);
            rewinding = true;
            canFreeze = false;
            StartCoroutine(ReenableFreezeWindow(.1f));
        }

        if (!stateInfo.IsName("SlowTime")) return;
        switch (rewinding)
        {
            case false when !Keyboard.current.spaceKey.isPressed && currentTime >= 0.99f:
                canFreeze = false;
                animator.SetFloat(Speed, -1f);
                rewinding = true;
                break;
            case true when currentTime <= 0.01f:
                animator.SetFloat(Speed, 2f);
                animator.SetTrigger(Backward);
                beginning = true;
                rewinding = false;
                canFreeze = true;
                break;
        }
    }

    IEnumerator HandleOverheat()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Out of Time");
        overheated = true;
        canFreeze = false;

        animator.SetFloat(Speed, -1f);
        rewinding = true;

        var flash = StartCoroutine(FlashRed());

        yield return new WaitForSecondsRealtime(overheatLockout);

        StopCoroutine(flash);

        print("Done with overheating");
        playerSprite.color = Color.white;
        overheated = false;

        canFreeze = true;
    }

    IEnumerator FlashRed()
    {
        Color normal = playerSprite.color;
        while (true)
        {
            playerSprite.color = OverheatColour;
            yield return new WaitForSecondsRealtime(0.2f);
            playerSprite.color = normal;
            yield return new WaitForSecondsRealtime(0.2f);
        }
    }

    IEnumerator ReenableFreezeWindow(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        canFreeze = true;
    }
    public void TickUp()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Tick Up");
    }
    public void TickDown()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Tick Down");
    }

    public void Reset()
    {
        chargeCurrent = chargeMax;

        overheated = false;
        canFreeze  = true;
        rewinding  = false;
        beginning  = true;

        if (animator)
        {
            animator.speed = 0f;
            animator.Rebind();
        }

        if (playerSprite != null) playerSprite.color = Color.white;
    }
}
