using System.Runtime.CompilerServices;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class ShootyThings : MonoBehaviour
{
    public float rayDistance = 50f;
    public LayerMask layerMask;
    public Animator anim;
    public GameObject spikePrefab;
    public float spikeSpeed = 10f;
    public float fireRate = 0.25f; // time between shots per direction

    public Transform[] shootPoints = new Transform[4]; // Up, Down, Left, Right
    private readonly Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
    private float[] fireTimers = new float[4];
    private void Update()
    {
        for (int i = 0; i < 4; i++)
        {
            fireTimers[i] -= Time.deltaTime;

            if (DetectPlayer(i) && fireTimers[i] <= 0f)
            {
                SpawnSpike(i);
                fireTimers[i] = fireRate;
            }
        }
    }

    private void SpawnSpike(int i)
    {
        
        Vector2 direction = dirs[i];
        Quaternion rot = Quaternion.LookRotation(Vector3.forward, direction);
        Vector3 spawnPos = (shootPoints.Length > i && shootPoints[i] != null) ? shootPoints[i].position : transform.position;
        anim.SetTrigger("Activate");
        GameObject spike = Instantiate(spikePrefab, spawnPos, rot);

        Rigidbody2D rb = spike.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * spikeSpeed;
        }
        else
        {
            Debug.LogWarning("Spawned spike has no Rigidbody2D!");
        }

        Destroy(spike, 5f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DetectPlayer(int i)
    {
        Vector2 direction = dirs[i];
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, layerMask);
        Debug.DrawRay(transform.position, direction * rayDistance, Color.yellow, 0.1f);

        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    private void OnDrawGizmos()
    {
        if (dirs == null || dirs.Length != 4) return;

        for (int i = 0; i < 4; i++)
        {
            Vector2 origin = transform.position;
            Vector2 direction = dirs[i];
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayDistance, layerMask);

            if (hit.collider != null)
            {
                Gizmos.color = hit.collider.CompareTag("Player") ? Color.green : Color.red;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.15f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, origin + direction * rayDistance);
            }
        }
    }
}
