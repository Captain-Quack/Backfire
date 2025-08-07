using Backfire;
using UnityEngine;

public sealed class Fog : MonoBehaviour
{
    
    //

    void OnParticleCollision(GameObject other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject?.GetComponent<PlayerController>().TakeDamage();
        }
    }
}
