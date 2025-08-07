using System.Collections;
using UnityEngine;

public sealed class Spike : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(waitTime(10f));
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<Controller>().TakeDamage();
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Bounce"))
        {
            
        }
        else Destroy(gameObject);
    }

    private IEnumerator waitTime(float yay)
    {
        yield return new WaitForSeconds(yay);
        Destroy(gameObject);
    }

}
