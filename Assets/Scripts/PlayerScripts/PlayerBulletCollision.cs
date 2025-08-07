using System;
using UnityEngine;

public sealed class PlayerBulletCollision : MonoBehaviour
{ 
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<EnemyController>() != null)
        {
            EnemyController controller = other.gameObject.GetComponent<EnemyController>();
            controller.TakeDamage();
        }    
    }
}
