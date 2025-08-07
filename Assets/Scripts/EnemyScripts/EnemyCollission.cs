using Backfire;
using UnityEngine;

public sealed class EnemyCollission : MonoBehaviour
{ 
    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(other.gameObject);
            gameObject.GetComponent<EnemyController>().TakeDamage();
        }
    }
}
