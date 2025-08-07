using System;
using FMODUnity;
using UnityEngine;

public class GravityEffect : MonoBehaviour
{
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private String gravityTag = "Player";
    [SerializeField] private bool inwardPull = true;

    void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in colliders)
        {
            if (col.CompareTag(gravityTag))
            {
                RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Orb Push");
                Rigidbody2D rb = col.attachedRigidbody;
                if (inwardPull)
                {
                    Vector2 direction = (transform.position - rb.transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, rb.transform.position);
                    float force = gravity * (1 - (distance / radius));
                    rb.AddForce(direction * force, ForceMode2D.Force);
                }
                else
                {
                    Vector2 direction = (rb.transform.position - transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, rb.transform.position);
                    float force = gravity * (1 - (distance / radius));
                    rb.AddForce(direction * force, ForceMode2D.Force);
                }
                
                
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
