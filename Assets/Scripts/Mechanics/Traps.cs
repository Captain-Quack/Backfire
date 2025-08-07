using System;
using UnityEngine;

public sealed class Traps : MonoBehaviour
{
    public GameObject trap;

    private void Start()
    {
        trap.GetComponent<Rigidbody2D>().gravityScale = 0;
        Color c = trap.GetComponent<SpriteRenderer>().color;
        c.a = 0f;
        trap.GetComponent<SpriteRenderer>().color = c;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            trap.GetComponent<Rigidbody2D>().gravityScale = 3;
            Color c = trap.GetComponent<SpriteRenderer>().color;
            c.a = 1f;
            trap.GetComponent<SpriteRenderer>().color = c;
            trap.GetComponent<TrapEndDetection>().Trigger();
        }
    }
}
