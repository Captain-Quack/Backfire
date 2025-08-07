using UnityEngine;

public class HollowPurpleSpawner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private GameObject orbitalBullet;
    [SerializeField] private int count = 10;
    [SerializeField] private GameObject orbitalBullet1;
    [SerializeField] private int count1 = 10;
    public Transform spawn1;
    public Transform spawn2;
    void Start()
    {
        
        for (int i = 0; i < count; i++)
        {
            GameObject bullet = Instantiate(orbitalBullet, spawn1.position, Quaternion.identity,transform);
            /*OrbittingBullet orbScript = bullet.GetComponent<OrbittingBullet>();
            orbScript.center = transform;
            orbScript.offSett = i * 360f / count;*/
        }
        for (int i = 0; i < count1; i++)
        {
            GameObject bullet = Instantiate(orbitalBullet1, spawn2.position, Quaternion.identity,transform);
            /*OrbittingBullet orbScript = bullet.GetComponent<OrbittingBullet>();
            orbScript.center = transform;
            orbScript.offSett = i * 360f / count;*/
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}