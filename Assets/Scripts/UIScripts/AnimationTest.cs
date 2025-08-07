using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    Animator animator;
    private bool canFreeze = true;
    private bool beginning = true;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.speed = 0f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }
    void Update()
    {
        
        animator.Update(Time.unscaledDeltaTime);
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float currentTime = stateInfo.normalizedTime %1f;
        if (currentTime <= 0.9f)
        {
            canFreeze = true;
        }

        if (canFreeze)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                animator.speed = 1f;
                Debug.Log("Space");
                animator.SetFloat("Speed", 1);
                if (beginning)
                {
                    animator.Play("Forward_Rechage", 0, 0f);
                    beginning = false;
                }
                else
                {
                    animator.Play("Forward_Rechage", 0, currentTime);
                }
                
            }
        }

        if (canFreeze)
        {
            if (Input.GetKeyUp(KeyCode.Space))
            {
                animator.SetFloat("Speed", -1);
                animator.Play("Forward_Rechage", 0, currentTime);
            }
        }

        if (stateInfo.IsName("Forward_Rechage"))
        {
            if (animator.GetFloat("Speed") > 0 && currentTime >= 0.99f)
            {
                canFreeze = false; 
                animator.SetFloat("Speed", -1);
            }
            else if (animator.GetFloat("Speed") < 0 && currentTime <= 0.01f)
            {
                animator.SetFloat("Speed", 1);
                animator.SetTrigger("Backward");
                beginning = true;
            }
        }
    }
}
