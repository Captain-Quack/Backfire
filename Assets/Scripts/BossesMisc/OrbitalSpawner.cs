using UnityEngine;

public class OrbitalSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject orbitalBullet;
    [SerializeField] private int count = 10;
    void Start()
    {
        
        for (int i = 0; i < count; i++)
        {
            GameObject bullet = Instantiate(orbitalBullet, transform.position, Quaternion.identity,transform);
            OrbittingBullet orbScript = bullet.GetComponent<OrbittingBullet>();
            orbScript.center = transform;
            orbScript.offSett = i * 360f / count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}