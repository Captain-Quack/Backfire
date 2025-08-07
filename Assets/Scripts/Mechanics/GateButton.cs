using System.Collections.Generic;
using System.Linq;
using Backfire;
using Unity.VisualScripting;
using UnityEngine;
using FMODUnity;

public sealed class GateButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private List<Laser> lasers;
    private readonly Collider2D[] buffer = new Collider2D[25];

    [Header("Configuration")]
    //[SerializeField] private float compressionScale = 0.45f;
    [SerializeField] private float radius = 5f;

    public Animator anim;
    void Start()
    {
        FindLasers();
    }

    private void FindLasers()
    {
        if (lasers.Count > 0) return;

        int hits = Physics2D.OverlapCircleNonAlloc(transform.position, radius, buffer);
        for (int i = 0; i < hits; i++)
        {
            var laser = buffer[i].GetComponent<Laser>();
            if (buffer[i].CompareTag("Laser") && laser != null)
            {
                lasers.Add(laser);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        print(other.gameObject.tag);
        //if (other.gameObject.CompareTag("Player")) other.gameObject.GetComponent<PlayerController>().TakeDamage();
        if (!other.gameObject.CompareTag("PlayerBullet") && !other.gameObject.CompareTag("Player")) return;
        print("press");
        lasers.ForEach(x => x.Retract());
        anim.SetTrigger("Hit");
        Destroy(other.gameObject);
    }

    /*private void CompressButton()
    {
        Transform buttonTransform = transform;
        Vector3 currentScale = buttonTransform.localScale;
        buttonTransform.localScale = new (currentScale.x, compressionScale, currentScale.z);
    }*/

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);
    }
    public void PressSFX()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Button");
    }
}
