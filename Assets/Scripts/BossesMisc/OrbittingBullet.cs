using Unity.Mathematics.Geometry;
using UnityEngine;

public class OrbittingBullet : MonoBehaviour
{
    public Transform center;
    public float offSett;
    [SerializeField] private float rotateSpeed;

    [SerializeField] private float maxRadius;
    [SerializeField] private float minRadius;
    [SerializeField] private float contractSpeed;

    [SerializeField] private float currentAngle;
    [SerializeField] private float pulseTime;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentAngle += rotateSpeed * Time.deltaTime;
        currentAngle %= 360f;
        //transform.RotateAround(center.position, Vector3.forward, currentAngle);
        
        pulseTime += Time.deltaTime + contractSpeed;
        float radius = Mathf.Lerp(minRadius, maxRadius, (Mathf.Sin(pulseTime)+1f)/2f);
        
        float radians = (currentAngle + offSett) * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
        transform.position = center.position + direction;
        
    }
}
