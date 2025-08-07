using System;
using System.Collections;
using Backfire;
using UnityEngine;

public sealed class Saw : MonoBehaviour
{ 
    void FixedUpdate()
    {
        transform.Rotate(0,0,7);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("PlayerBullet"))
        {
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("Player"))
        {
           StartCoroutine(waitTime(2.5f));
           GameManager.Instance.ResetGame();
           other.gameObject.GetComponent<PlayerController>().TakeDamage();
        }
    }

    private IEnumerator waitTime(float yay)
    {
        yield return new WaitForSeconds(yay);
    }
}
