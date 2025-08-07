using Backfire;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private float shrinkSpeed = 10f;
    [SerializeField] public bool retractLeftOrRight = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("PlayerBullet"))
        {
            if (other.CompareTag("PlayerBullet"))
            {
                Destroy(other.gameObject);
            }

            if (other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>()?.TakeDamage();
            }
        };
    }

    public void Retract() => Retract(retractLeftOrRight);
    public void Retract(bool left)
    {
        print("retract");
        var newScale = Mathf.Max(0, transform.localScale.x - shrinkSpeed * Time.deltaTime);
        var deltaScale = transform.localScale.x - newScale;

        transform.localScale = new(0, transform.localScale.y, transform.localScale.z);
        transform.position += (left ? -1 : 1) * new Vector3(deltaScale * 0.5f, 0, 0);
        Destroy(gameObject);
    }
}