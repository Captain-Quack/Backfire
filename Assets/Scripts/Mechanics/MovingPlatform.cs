using UnityEngine;

public sealed class MovingPlatform : MonoBehaviour
{
    public float speed;
    private Transform pointA;
    private Transform pointB;
    private Transform currentTarget;

    public GameObject boundary1;
    public GameObject boundary2;

    void Start()
    {
        pointA = boundary1.transform;
        pointB = boundary2.transform;

        currentTarget = pointB;
    }

    void Update()
    {
        if (!pointA || !pointB) return;

        transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTarget.position) < 0.05f)
        {
            currentTarget = currentTarget == pointA ? pointB : pointA;
        }
    }
}
