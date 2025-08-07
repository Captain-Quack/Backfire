using System;
using Backfire;
using UnityEngine;

public sealed class GravityFieldForce : MonoBehaviour
{
    [SerializeField] Vector3 _constantForce;
    [SerializeField] ParticleSystem _particleSystem;
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + _constantForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Rigidbody2D>().AddForce(_constantForce);
        }
    }
}
