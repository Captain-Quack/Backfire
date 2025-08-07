using System;
using UnityEngine;

public class GravityEffect1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float gravity = 10f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private String gravityTag = "Player";

    void FixedUpdate()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (var col in colliders)
        {
            if (col.CompareTag(gravityTag))
            {
                Rigidbody2D rb = col.attachedRigidbody;
                Vector2 direction = (rb.transform.position-transform.position ).normalized;
                float distance = Vector2.Distance(transform.position, rb.transform.position);
                float force = gravity * (1 - (distance / radius));
                rb.AddForce(direction * force,ForceMode2D.Force);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
