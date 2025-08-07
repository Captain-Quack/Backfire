using System.Collections;
using UnityEngine;


public class ShieldDissapear : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int health = 2;
    void Start()
    {
        StartCoroutine(Wait(30f));
    }


    // Update is called once per frame
    void Update()
    {
      
    }


    private IEnumerator Wait(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBullet"))
        {
            health--;
            Destroy(other.gameObject);
            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}