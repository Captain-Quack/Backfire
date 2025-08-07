using UnityEngine;

public class ButtonAnimator : MonoBehaviour
{
    public Animator animator;

    public void PlayIdleConfig()
    {
        animator.SetTrigger("Idle_Config");
    }

    public void PlayIdleLevelSelection()
    {
        animator.SetTrigger("Idle_LevelSelection");
    }
    
    public void PlayIdleStartScene()
    {
        animator.SetTrigger("Idle_StartScene");
    }

    public void PlayInConfig()
    {
        animator.SetTrigger("In_Config");
    }

    public void PlayLoopOpening()
    {
        animator.SetTrigger("Loop_Opening");
    }

    public void PlayInLevelSelection()
    {
        animator.SetTrigger("In_LevelSelection");
    }

    public void PlayInStartScene()
    {
        animator.SetTrigger("In_StartScene");
    }

    public void PlayOutOpening()
    {
        animator.SetTrigger("Out_Opening");
    }
    
    public void PlayOutStartScene()
    {
        animator.SetTrigger("Out_StartScene");
    }
}
