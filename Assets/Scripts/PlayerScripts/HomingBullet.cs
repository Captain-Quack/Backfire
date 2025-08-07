using UnityEngine;

public sealed class HomingBullet : MonoBehaviour
{
    public float initialspeed;
    public float homingSpeed;
    public float delay;
    public float turnSpeed;
    public float duration;
    
    private Transform player;
    private Rigidbody2D rb;
    private bool isHoming = false;
    private float timer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * initialspeed;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (!isHoming && timer > delay)
        {
            timer = 0;
            isHoming = true;
        }
        if (isHoming)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 currentDirection = rb.linearVelocity.normalized;
            
        }
    }
}
