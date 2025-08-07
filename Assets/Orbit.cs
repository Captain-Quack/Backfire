using UnityEngine;

public class Orbit : MonoBehaviour
{
    public Transform center;
    public float radius = 5f;
    public float rotateSpeed = 50f;

    private float angle = 0f;

    void Start()
    {
        center = GameObject.Find("Hollow Purple").transform;
    }
    void Update()
    {
        if (center == null) return;
        
        angle += rotateSpeed * Time.deltaTime;
        if (angle > 360f) angle -= 360f;

        float rad = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        transform.position = center.position + offset;
    }
}