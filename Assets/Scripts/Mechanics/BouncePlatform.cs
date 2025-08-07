using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;      // for Handles.ArrowHandleCap
#endif

public sealed class BouncePlatform : MonoBehaviour
{
    [Header("Launch parameters")]
    [Tooltip("World‑space direction to launch the object (will be normalized).")]
    public Vector2 launchDirection = Vector2.up;

    [Tooltip("Speed to apply along the launchDirection.")]
    public float launchSpeed = 40f;

    public Animator anim;

    private void Awake()
    {
        var arrow = transform.GetChild(0).transform; 
        var forceDirection = launchDirection.normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, forceDirection);
        arrow.rotation = rotation;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") &&
            !collision.gameObject.CompareTag("EnemyBullet") &&
            !collision.gameObject.CompareTag("PlayerBullet")) return;
        if (!collision.gameObject.TryGetComponent(out Rigidbody2D rb)) return;
        rb.linearVelocity = launchDirection.normalized * launchSpeed;
        anim.Play("Bouncy",0,0f);
    }
    
    
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Vector3 origin     = transform.position;
        Vector3 dir3D      = ((Vector3)launchDirection).normalized;
        float  length      = Mathf.Max(0.5f, launchSpeed * 0.05f);

        // draw shaft
        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + dir3D * length);

        Handles.color = Gizmos.color;
        Handles.ArrowHandleCap(
            controlID: 0,
            position:  origin + dir3D * length,
            rotation:  Quaternion.LookRotation(Vector3.forward, dir3D),
            size:      length * 0.2f,
            eventType: EventType.Repaint);
    }
#endif
    
}