using System.Collections;
using Backfire;
using UnityEngine;
using FMODUnity;
public sealed class EnemyController : Controller
{
    private static readonly int Death = Animator.StringToHash("Death");
    public Animator anim;
    public float deathTime;
    private void Start()
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.AddEnemy();
        }
        else
        {
            Debug.LogError("!!No GameManager found!!");
        }
    }

    public override bool TakeDamage()
    {
        health--;
        if(health <= 0)
        {
            if (gameObject.name.Contains("Boss"))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Die");
            }
            else
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Enemy Death");
            }
            if (anim != null)
            {
                anim.SetTrigger(Death);
                StartCoroutine(Wait(deathTime));
                GameManager.Instance?.AddEnemiesKilled();
            }
            else
            {
                Destroy(gameObject);
                GameManager.Instance?.AddEnemiesKilled();
            }
        }
        return true;
    }

    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}


