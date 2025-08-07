using UnityEngine;

public class ShieldHit : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int health = 100;
    public Animator anim;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            anim.SetTrigger("Hit");
            health--;
            Destroy(other.gameObject);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
